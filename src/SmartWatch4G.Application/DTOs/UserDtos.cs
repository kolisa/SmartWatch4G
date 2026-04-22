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

    /// <summary>Optionally link this user to a company on creation.</summary>
    public int? CompanyId { get; set; }
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

public sealed class LinkUserToCompanyRequest
{
    /// <summary>Set to null to remove the user from their current company.</summary>
    public int? CompanyId { get; set; }
}

public sealed class UserResponse
{
    public string DeviceId { get; init; } = string.Empty;
    public int UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Surname { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Cell { get; init; }
    public string? EmpNo { get; init; }
    public string? Address { get; init; }
    public int? CompanyId { get; init; }
    public string? CompanyName { get; init; }
    public DateTime UpdatedAt { get; init; }
}

// ── Company DTOs ──────────────────────────────────────────────────────────────

public sealed class CreateCompanyRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? RegistrationNumber { get; set; }

    [EmailAddress]
    [StringLength(200)]
    public string? ContactEmail { get; set; }

    [StringLength(50)]
    public string? ContactPhone { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }
}

public sealed class UpdateCompanyRequest
{
    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [StringLength(100)]
    public string? RegistrationNumber { get; set; }

    [EmailAddress]
    [StringLength(200)]
    public string? ContactEmail { get; set; }

    [StringLength(50)]
    public string? ContactPhone { get; set; }

    [StringLength(500)]
    public string? Address { get; set; }
}

public sealed class CompanyResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? RegistrationNumber { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public string? Address { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
