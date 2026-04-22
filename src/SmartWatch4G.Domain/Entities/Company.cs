namespace SmartWatch4G.Domain.Entities;

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? RegistrationNumber { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
