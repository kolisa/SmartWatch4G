using System.ComponentModel.DataAnnotations;

namespace SmartWatch4G.Application.DTOs;

public sealed class CreateUserRequest
{
    [Required]
    [StringLength(50)]
    public string DeviceId { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Surname { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Cell { get; set; }

    [StringLength(50)]
    public string? EmpNo { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }
}

public sealed class UpdateUserRequest
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Surname { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(200)]
    public string? Email { get; set; }

    [StringLength(30)]
    public string? Cell { get; set; }

    [StringLength(50)]
    public string? EmpNo { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }
}

public sealed class UserResponse
{
    public string DeviceId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Cell { get; init; }
    public string? EmpNo { get; init; }
    public string? Address { get; init; }
    public DateTime UpdatedAt { get; init; }
}
