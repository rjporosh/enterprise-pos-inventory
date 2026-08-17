using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PosService.Application.Sales.Events;
using PosService.Domain.Sales;
using RabbitMQ.Client;
using SharedKernel.IntegrationEvents;

namespace PosService.Infrastructure.Messaging;

/// <summary>
/// Publishes Sale integration events to the shared "pos.events" topic exchange. Connects lazily on first
/// publish and reuses a single connection/channel for the lifetime of the process. Any connection or
/// publish failure is allowed to bubble up to the caller (CompleteSaleHandler/VoidSaleHandler), which
/// already treats publish failures as non-fatal to the already-committed sale — so a RabbitMQ outage
/// never blocks POS checkout.
/// </summary>
public sealed class RabbitMqSaleEventPublisher : ISaleEventPublisher, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqSaleEventPublisher> _logger;
    private readonly object _connectionLock = new();
    private IConnection? _connection;
    private IModel? _channel;

    public RabbitMqSaleEventPublisher(IOptions<RabbitMqOptions> options, ILogger<RabbitMqSaleEventPublisher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task PublishSaleCompletedAsync(Sale sale, CancellationToken ct = default)
    {
        var evt = new SaleCompletedIntegrationEvent(
            EventId: Guid.NewGuid(),
            CorrelationId: sale.Id,
            OccurredAtUtc: DateTime.UtcNow,
            SaleId: sale.Id,
            SaleNumber: sale.SaleNumber,
            StoreId: sale.StoreId,
            Items: sale.Items.Select(i => new SaleLineItem(i.ProductId, i.Sku, i.Quantity)).ToList());

        Publish(SaleCompletedIntegrationEvent.RoutingKey, evt);
        return Task.CompletedTask;
    }

    public Task PublishSaleVoidedAsync(Sale sale, CancellationToken ct = default)
    {
        var evt = new SaleVoidedIntegrationEvent(
            EventId: Guid.NewGuid(),
            CorrelationId: sale.Id,
            OccurredAtUtc: DateTime.UtcNow,
            SaleId: sale.Id,
            SaleNumber: sale.SaleNumber,
            StoreId: sale.StoreId,
            Items: sale.Items.Select(i => new SaleLineItem(i.ProductId, i.Sku, i.Quantity)).ToList());

        Publish(SaleVoidedIntegrationEvent.RoutingKey, evt);
        return Task.CompletedTask;
    }

    private void Publish<TEvent>(string routingKey, TEvent evt)
    {
        var channel = GetOrCreateChannel();

        var body = JsonSerializer.SerializeToUtf8Bytes(evt, JsonOptions);

        var props = channel.CreateBasicProperties();
        props.Persistent = true;
        props.ContentType = "application/json";
        props.ContentEncoding = Encoding.UTF8.WebName;
        props.Type = routingKey;

        if (evt is SaleCompletedIntegrationEvent completed)
        {
            props.MessageId = completed.EventId.ToString();
            props.CorrelationId = completed.CorrelationId.ToString();
        }
        else if (evt is SaleVoidedIntegrationEvent voided)
        {
            props.MessageId = voided.EventId.ToString();
            props.CorrelationId = voided.CorrelationId.ToString();
        }

        channel.BasicPublish(
            exchange: _options.Exchange,
            routingKey: routingKey,
            mandatory: false,
            basicProperties: props,
            body: body);

        _logger.LogInformation("Published {RoutingKey} event {MessageId} to exchange {Exchange}", routingKey, props.MessageId, _options.Exchange);
    }

    private IModel GetOrCreateChannel()
    {
        if (_channel is { IsOpen: true })
        {
            return _channel;
        }

        lock (_connectionLock)
        {
            if (_channel is { IsOpen: true })
            {
                return _channel;
            }

            var factory = new ConnectionFactory
            {
                HostName = _options.Host,
                Port = _options.Port,
                VirtualHost = _options.VirtualHost,
                UserName = _options.UserName,
                Password = _options.Password,
                DispatchConsumersAsync = false
            };

            _connection = factory.CreateConnection("pos-service-publisher");
            _channel = _connection.CreateModel();
            _channel.ExchangeDeclare(exchange: _options.Exchange, type: ExchangeType.Topic, durable: true, autoDelete: false);

            _logger.LogInformation("Connected to RabbitMQ at {Host}:{Port} and declared exchange {Exchange}", _options.Host, _options.Port, _options.Exchange);

            return _channel;
        }
    }

    public void Dispose()
    {
        try
        {
            _channel?.Close();
            _connection?.Close();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error closing RabbitMQ connection during publisher disposal");
        }
        finally
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}
