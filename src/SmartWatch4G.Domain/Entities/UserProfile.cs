namespace SmartWatch4G.Domain.Entities;

public class UserProfile
{
    public string DeviceId { get; set; } = string.Empty;
    /// <summary>Auto-generated surrogate key, unique per worker. Stable across device reassignments.</summary>
    public int UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Cell { get; set; }
    public string? EmpNo { get; set; }
    public string? Address { get; set; }
    /// <summary>FK to companies.id. Null when the worker has not been linked to a company yet.</summary>
    public int? CompanyId { get; set; }
    public string? CompanyName { get; set; } // populated by JOIN, not stored
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}
