using MediatR;
using SharedKernel;
using NotificationService.Application.Common.Models;

namespace NotificationService.Application.Features.Notifications.GetNotificationById;

public sealed record GetNotificationByIdQuery(Guid NotificationId) : IRequest<Result<NotificationDto>>;
