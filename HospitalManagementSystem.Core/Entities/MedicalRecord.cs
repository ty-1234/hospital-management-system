using System.ComponentModel.DataAnnotations;

namespace HospitalManagementSystem.Core.Entities;

public class MedicalRecord
{
    public int Id { get; set; }

    public int PatientId { get; set; }
    public Patient? Patient { get; set; }

    [Required]
    [MaxLength(250)]
    public string Diagnosis { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? TreatmentPlan { get; set; }

    [DataType(DataType.Date)]
    public DateTime RecordedOn { get; set; } = DateTime.UtcNow.Date;

    [MaxLength(500)]
    public string? Notes { get; set; }
}

