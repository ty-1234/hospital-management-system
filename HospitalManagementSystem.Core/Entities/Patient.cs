using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Core.Entities;

public class Patient
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime DateOfBirth { get; set; }

    [MaxLength(10)]
    public string Gender { get; set; } = "Unknown";

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    [EmailAddress]
    [MaxLength(120)]
    public string? Email { get; set; }

    [MaxLength(250)]
    public string? Address { get; set; }

    [MaxLength(120)]
    public string? EmergencyContactName { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? EmergencyContactPhone { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<Bill> Bills { get; set; } = new List<Bill>();
    public ICollection<MedicalRecord> MedicalRecords { get; set; } = new List<MedicalRecord>();

    public string FullName => $"{FirstName} {LastName}";
}

