namespace SmartWatch4G.Domain.Entities;

public class UserProfile
{
    public string DeviceId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Cell { get; set; }
    public string? EmpNo { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}
