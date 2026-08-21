using Garij.Application.DTOs;
using Garij.Domain.Enums;

namespace Garij.Application.Interfaces;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync();

    Task<NotificationDto?> GetNotificationByIdAsync(int id);

    Task<IEnumerable<NotificationDto>> GetNotificationsByServiceJobAsync(int serviceJobId);

    Task<IEnumerable<NotificationDto>> GetPendingNotificationsAsync();

    Task<NotificationDto> CreateNotificationAsync(NotificationDto notification);

    Task<NotificationDto> RespondToNotificationAsync(int notificationId, NotificationStatus status);
}
