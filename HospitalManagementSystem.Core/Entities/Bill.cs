using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Core.Entities;

public class Bill
{
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    public int? AppointmentId { get; set; }
    public Appointment? Appointment { get; set; }

    [Range(0.01, 1_000_000)]
    public decimal Amount { get; set; }

    [DataType(DataType.Date)]
    public DateTime IssuedOn { get; set; } = DateTime.UtcNow.Date;

    [DataType(DataType.Date)]
    public DateTime PaymentDueDate { get; set; } = DateTime.UtcNow.Date.AddDays(30);

    public bool IsPaid { get; set; }

    [MaxLength(200)]
    public string? Description { get; set; }
}

