using Garij.Application.DTOs;
using Garij.Application.Interfaces;
using Garij.Domain.Enums;

namespace Garij.Application.Services;

public class NotificationService : INotificationService
{
    public Task<IEnumerable<NotificationDto>> GetAllNotificationsAsync() => throw new NotImplementedException();

    public Task<NotificationDto?> GetNotificationByIdAsync(int id) => throw new NotImplementedException();

    public Task<IEnumerable<NotificationDto>> GetNotificationsByServiceJobAsync(int serviceJobId) => throw new NotImplementedException();

    public Task<IEnumerable<NotificationDto>> GetPendingNotificationsAsync() => throw new NotImplementedException();

    public Task<NotificationDto> CreateNotificationAsync(NotificationDto notification) => throw new NotImplementedException();

    public Task<NotificationDto> RespondToNotificationAsync(int notificationId, NotificationStatus status) => throw new NotImplementedException();
}
