using MediatR;
using SharedKernel;
using NotificationService.Application.Common.Models;

namespace NotificationService.Application.Features.Notifications.CancelNotification;

public sealed record CancelNotificationCommand(Guid NotificationId, string Reason) : IRequest<Result>;
