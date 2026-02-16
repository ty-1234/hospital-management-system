using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.Infrastructure.Data;
using HospitalManagementSystem.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Tests;

public class BillsControllerTests
{
    [Fact]
    public async Task Create_WithValidModel_PersistsAndRedirects()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var patient = new Patient { FirstName = "A", LastName = "Patient" };
        var department = new Department { Name = "Orthopedics" };
        var doctor = new Doctor { FirstName = "B", LastName = "Doctor", Specialization = "Ortho", Department = department };
        var appointment = new Appointment
        {
            Patient = patient,
            Doctor = doctor,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
            Reason = "Consultation"
        };

        context.AddRange(patient, department, doctor, appointment);
        await context.SaveChangesAsync();

        var controller = new BillsController(context);
        var bill = new Bill
        {
            PatientId = patient.Id,
            AppointmentId = appointment.Id,
            Amount = 199.99m,
            IssuedOn = DateTime.UtcNow.Date,
            PaymentDueDate = DateTime.UtcNow.Date.AddDays(7),
            Description = "Visit fee"
        };

        var result = await controller.Create(bill);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var saved = await context.Bills.SingleAsync();
        Assert.Equal(199.99m, saved.Amount);
        Assert.Equal(patient.Id, saved.PatientId);
        Assert.Equal(appointment.Id, saved.AppointmentId);
    }

    [Fact]
    public async Task Create_WithMismatchedAppointmentPatient_ReturnsViewAndDoesNotPersist()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var patient1 = new Patient { FirstName = "A", LastName = "Patient1" };
        var patient2 = new Patient { FirstName = "B", LastName = "Patient2" };
        var department = new Department { Name = "General" };
        var doctor = new Doctor { FirstName = "D", LastName = "Doctor", Specialization = "General", Department = department };
        var appointment = new Appointment
        {
            Patient = patient1,
            Doctor = doctor,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
            Reason = "Consultation"
        };

        context.AddRange(patient1, patient2, department, doctor, appointment);
        await context.SaveChangesAsync();

        var controller = new BillsController(context);
        var bill = new Bill
        {
            PatientId = patient2.Id,
            AppointmentId = appointment.Id,
            Amount = 99m,
            IssuedOn = DateTime.UtcNow.Date,
            PaymentDueDate = DateTime.UtcNow.Date.AddDays(7)
        };

        var result = await controller.Create(bill);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(bill, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(Bill.AppointmentId)));
        Assert.Equal(0, await context.Bills.CountAsync());
    }

    [Fact]
    public async Task Edit_WithMismatchedAppointmentPatient_ReturnsViewAndDoesNotPersistChanges()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var patient1 = new Patient { FirstName = "A", LastName = "Patient1" };
        var patient2 = new Patient { FirstName = "B", LastName = "Patient2" };
        var department = new Department { Name = "Surgery" };
        var doctor = new Doctor { FirstName = "D", LastName = "Doctor", Specialization = "Surgeon", Department = department };
        var appointment = new Appointment
        {
            Patient = patient1,
            Doctor = doctor,
            StartTime = DateTime.UtcNow.AddDays(1),
            EndTime = DateTime.UtcNow.AddDays(1).AddMinutes(30),
            Reason = "Consultation"
        };

        var bill = new Bill
        {
            Patient = patient1,
            Appointment = appointment,
            Amount = 150m,
            IssuedOn = DateTime.UtcNow.Date,
            PaymentDueDate = DateTime.UtcNow.Date.AddDays(7),
            Description = "Original"
        };

        context.AddRange(patient1, patient2, department, doctor, appointment, bill);
        await context.SaveChangesAsync();

        var controller = new BillsController(context);
        var edited = new Bill
        {
            Id = bill.Id,
            PatientId = patient2.Id,
            AppointmentId = appointment.Id,
            Amount = 175m,
            IssuedOn = bill.IssuedOn,
            PaymentDueDate = bill.PaymentDueDate,
            Description = "Edited mismatch"
        };

        var result = await controller.Edit(bill.Id, edited);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(edited, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(Bill.AppointmentId)));

        var persisted = await context.Bills.AsNoTracking().SingleAsync(b => b.Id == bill.Id);
        Assert.Equal(patient1.Id, persisted.PatientId);
        Assert.Equal(150m, persisted.Amount);
        Assert.Equal("Original", persisted.Description);
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        return new ApplicationDbContext(options);
    }
}
