using System.Diagnostics;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HospitalManagementSystem.Web.Models;

namespace HospitalManagementSystem.Web.Controllers;

public class HomeController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var model = new DashboardViewModel
        {
            DepartmentCount = await dbContext.Departments.CountAsync(),
            DoctorCount = await dbContext.Doctors.CountAsync(),
            PatientCount = await dbContext.Patients.CountAsync(),
            AppointmentCount = await dbContext.Appointments.CountAsync(),
            PendingBillsCount = await dbContext.Bills.CountAsync(b => !b.IsPaid),
            UpcomingAppointments = await dbContext.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Where(a => a.StartTime >= DateTime.UtcNow)
                .OrderBy(a => a.StartTime)
                .Take(8)
                .ToListAsync()
        };
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
