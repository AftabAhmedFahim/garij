using Garij.Domain.Enums;

namespace Garij.Application.DTOs;

public class UserDto
{
    public int Id { get; set; }

    public string IdentityUserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PhoneNumber { get; set; } = string.Empty;

    public UserRole Role { get; set; }
}
