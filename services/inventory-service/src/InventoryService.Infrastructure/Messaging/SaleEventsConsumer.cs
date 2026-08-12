using System.Text;
using System.Text.Json;
using InventoryService.Application.Integration;
using InventoryService.Application.Stock;
using InventoryService.Application.Warehouses;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using SharedKernel.IntegrationEvents;

namespace InventoryService.Infrastructure.Messaging;

/// <summary>
/// Consumes SaleCompleted/SaleVoided events published by POS on the shared "pos.events" topic exchange
/// and translates them into stock-out/stock-in movements against the default warehouse. Idempotent via
/// ProcessedIntegrationEvent (dedupes on RabbitMQ's at-least-once redelivery). Entirely optional: if
/// RabbitMQ is not configured, or is unreachable, this service logs and backs off — it never prevents
/// Inventory's own API from serving requests (PRIMARY GOAL: RabbitMQ unavailable → Inventory still works).
/// </summary>
public class SaleEventsConsumer(
    IOptions<RabbitMqOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<SaleEventsConsumer> logger) : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly RabbitMqOptions _options = options.Value;

    private IConnection? _connection;
    private IModel? _channel;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.IsConfigured)
        {
            logger.LogInformation("RabbitMQ is not configured; SaleEventsConsumer will not start. Inventory runs standalone without POS integration.");
            return;
        }

        var delay = TimeSpan.FromSeconds(5);
        const int maxDelaySeconds = 60;

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                Connect();
                StartConsuming(stoppingToken);

                // Block here while the channel stays open; if the connection drops, ChannelShutdown/ConnectionShutdown
                // will be observed by the next loop iteration's Connect() call after the delay below.
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "SaleEventsConsumer could not connect to RabbitMQ at {Host}:{Port}; retrying in {Delay}s. Inventory API continues to serve requests normally.",
                    _options.Host, _options.Port, delay.TotalSeconds);

                CleanUp();

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }

                delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, maxDelaySeconds));
            }
        }

        CleanUp();
    }

    private void Connect()
    {
        var factory = new ConnectionFactory
        {
            HostName = _options.Host,
            Port = _options.Port,
            VirtualHost = _options.VirtualHost,
            UserName = _options.UserName,
            Password = _options.Password,
            DispatchConsumersAsync = true
        };

        _connection = factory.CreateConnection("inventory-service-consumer");
        _channel = _connection.CreateModel();

        _channel.ExchangeDeclare(exchange: _options.Exchange, type: ExchangeType.Topic, durable: true, autoDelete: false);

        _channel.QueueDeclare(
            queue: _options.DeadLetterQueue,
            durable: true,
            exclusive: false,
            autoDelete: false);

        var mainQueueArgs = new Dictionary<string, object>
        {
            ["x-dead-letter-exchange"] = string.Empty,
            ["x-dead-letter-routing-key"] = _options.DeadLetterQueue
        };

        _channel.QueueDeclare(
            queue: _options.Queue,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: mainQueueArgs);

        _channel.QueueBind(queue: _options.Queue, exchange: _options.Exchange, routingKey: "sale.*");
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 10, global: false);

        logger.LogInformation("SaleEventsConsumer connected to RabbitMQ at {Host}:{Port}, consuming queue {Queue}", _options.Host, _options.Port, _options.Queue);
    }

    private void StartConsuming(CancellationToken stoppingToken)
    {
        if (_channel is null) return;

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += async (_, ea) => await HandleMessageAsync(ea, stoppingToken);

        _channel.BasicConsume(queue: _options.Queue, autoAck: false, consumer: consumer);
    }

    private async Task HandleMessageAsync(BasicDeliverEventArgs ea, CancellationToken stoppingToken)
    {
        var routingKey = ea.RoutingKey;
        var body = Encoding.UTF8.GetString(ea.Body.ToArray());

        try
        {
            using var scope = scopeFactory.CreateScope();
            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
            var eventStore = scope.ServiceProvider.GetRequiredService<IProcessedEventStore>();
            var warehouseRepository = scope.ServiceProvider.GetRequiredService<IWarehouseRepository>();

            switch (routingKey)
            {
                case SaleCompletedIntegrationEvent.RoutingKey:
                    await HandleSaleCompletedAsync(body, mediator, eventStore, warehouseRepository, stoppingToken);
                    break;
                case SaleVoidedIntegrationEvent.RoutingKey:
                    await HandleSaleVoidedAsync(body, mediator, eventStore, warehouseRepository, stoppingToken);
                    break;
                default:
                    logger.LogWarning("Received message with unrecognized routing key {RoutingKey}; dead-lettering", routingKey);
                    _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
                    return;
            }

            _channel?.BasicAck(ea.DeliveryTag, multiple: false);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process {RoutingKey} message; sending to dead-letter queue", routingKey);
            _channel?.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
        }
    }

    private async Task HandleSaleCompletedAsync(string body, IMediator mediator, IProcessedEventStore eventStore, IWarehouseRepository warehouseRepository, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<SaleCompletedIntegrationEvent>(body, JsonOptions)
            ?? throw new InvalidOperationException("Could not deserialize SaleCompletedIntegrationEvent.");

        if (await eventStore.IsProcessedAsync(evt.EventId, ct))
        {
            logger.LogInformation("SaleCompleted event {EventId} already processed; skipping (idempotent)", evt.EventId);
            return;
        }

        var warehouse = await warehouseRepository.GetDefaultWarehouseAsync(ct);
        if (warehouse is null)
        {
            logger.LogWarning("No default warehouse configured; cannot apply stock deduction for sale {SaleNumber}", evt.SaleNumber);
            await eventStore.MarkProcessedAsync(evt.EventId, SaleCompletedIntegrationEvent.RoutingKey, ct);
            return;
        }

        foreach (var item in evt.Items)
        {
            var result = await mediator.Send(new StockOutCommand(
                item.ProductId, warehouse.Id, item.Quantity, "PosSale", evt.SaleId, $"POS sale {evt.SaleNumber}"), ct);

            if (!result.IsSuccess)
            {
                logger.LogWarning("Could not deduct stock for product {ProductId} on sale {SaleNumber}: {Error}", item.ProductId, evt.SaleNumber, result.Error.Description);
            }
        }

        await eventStore.MarkProcessedAsync(evt.EventId, SaleCompletedIntegrationEvent.RoutingKey, ct);
        logger.LogInformation("Applied stock deduction for sale {SaleNumber} ({ItemCount} lines)", evt.SaleNumber, evt.Items.Count);
    }

    private async Task HandleSaleVoidedAsync(string body, IMediator mediator, IProcessedEventStore eventStore, IWarehouseRepository warehouseRepository, CancellationToken ct)
    {
        var evt = JsonSerializer.Deserialize<SaleVoidedIntegrationEvent>(body, JsonOptions)
            ?? throw new InvalidOperationException("Could not deserialize SaleVoidedIntegrationEvent.");

        if (await eventStore.IsProcessedAsync(evt.EventId, ct))
        {
            logger.LogInformation("SaleVoided event {EventId} already processed; skipping (idempotent)", evt.EventId);
            return;
        }

        var warehouse = await warehouseRepository.GetDefaultWarehouseAsync(ct);
        if (warehouse is null)
        {
            logger.LogWarning("No default warehouse configured; cannot reverse stock deduction for sale {SaleNumber}", evt.SaleNumber);
            await eventStore.MarkProcessedAsync(evt.EventId, SaleVoidedIntegrationEvent.RoutingKey, ct);
            return;
        }

        foreach (var item in evt.Items)
        {
            var result = await mediator.Send(new StockInCommand(
                item.ProductId, warehouse.Id, item.Quantity, null, "PosSaleVoid", evt.SaleId, $"Reversal for voided POS sale {evt.SaleNumber}"), ct);

            if (!result.IsSuccess)
            {
                logger.LogWarning("Could not reverse stock for product {ProductId} on voided sale {SaleNumber}: {Error}", item.ProductId, evt.SaleNumber, result.Error.Description);
            }
        }

        await eventStore.MarkProcessedAsync(evt.EventId, SaleVoidedIntegrationEvent.RoutingKey, ct);
        logger.LogInformation("Reversed stock deduction for voided sale {SaleNumber} ({ItemCount} lines)", evt.SaleNumber, evt.Items.Count);
    }

    private void CleanUp()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Error while closing RabbitMQ connection");
        }
        finally
        {
            _channel?.Dispose();
            _connection?.Dispose();
            _channel = null;
            _connection = null;
        }
    }

    public override void Dispose()
    {
        CleanUp();
        base.Dispose();
    }
}
