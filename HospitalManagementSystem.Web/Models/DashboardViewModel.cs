using HospitalManagementSystem.Core.Entities;

namespace HospitalManagementSystem.Web.Models;

public class DashboardViewModel
{
    public int DepartmentCount { get; set; }
    public int DoctorCount { get; set; }
    public int PatientCount { get; set; }
    public int AppointmentCount { get; set; }
    public int PendingBillsCount { get; set; }
    public IReadOnlyList<Appointment> UpcomingAppointments { get; set; } = [];
}
