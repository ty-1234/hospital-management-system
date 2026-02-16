using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Core.Entities;

public class Doctor
{
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(80)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string Specialization { get; set; } = string.Empty;

    [EmailAddress]
    [MaxLength(120)]
    public string? Email { get; set; }

    [Phone]
    [MaxLength(20)]
    public string? PhoneNumber { get; set; }

    public int DepartmentId { get; set; }
    public Department? Department { get; set; }

    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

    public string FullName => $"{FirstName} {LastName}";
}

