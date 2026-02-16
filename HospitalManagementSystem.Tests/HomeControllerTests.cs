using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.Core.Enums;
using HospitalManagementSystem.Infrastructure.Data;
using HospitalManagementSystem.Web.Controllers;
using HospitalManagementSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Tests;

public class HomeControllerTests
{
    [Fact]
    public async Task Index_ReturnsExpectedDashboardCounts()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var department = new Department { Name = "Pediatrics" };
        var doctor = new Doctor { FirstName = "John", LastName = "Doe", Specialization = "Peds", Department = department };
        var patient = new Patient { FirstName = "Jane", LastName = "Smith" };

        var upcoming = new Appointment
        {
            Patient = patient,
            Doctor = doctor,
            StartTime = DateTime.UtcNow.AddHours(2),
            EndTime = DateTime.UtcNow.AddHours(2.5),
            Reason = "Checkup",
            Status = AppointmentStatus.Scheduled
        };

        var bill = new Bill
        {
            Patient = patient,
            Appointment = upcoming,
            Amount = 120m,
            IssuedOn = DateTime.UtcNow.Date,
            PaymentDueDate = DateTime.UtcNow.Date.AddDays(10),
            IsPaid = false
        };

        context.AddRange(department, doctor, patient, upcoming, bill);
        await context.SaveChangesAsync();

        var controller = new HomeController(context);

        var result = await controller.Index();

        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<DashboardViewModel>(viewResult.Model);

        Assert.Equal(1, model.DepartmentCount);
        Assert.Equal(1, model.DoctorCount);
        Assert.Equal(1, model.PatientCount);
        Assert.Equal(1, model.AppointmentCount);
        Assert.Equal(1, model.PendingBillsCount);
        Assert.Single(model.UpcomingAppointments);
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        return new ApplicationDbContext(options);
    }
}
