using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.Core.Enums;
using HospitalManagementSystem.Infrastructure.Data;
using HospitalManagementSystem.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Tests;

public class AppointmentsControllerTests
{
    [Fact]
    public async Task Create_WithInvalidTimes_ReturnsViewAndDoesNotPersist()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var patient = new Patient { FirstName = "A", LastName = "Patient" };
        var department = new Department { Name = "Cardiology" };
        var doctor = new Doctor { FirstName = "B", LastName = "Doctor", Specialization = "Cardio", Department = department };

        context.Patients.Add(patient);
        context.Departments.Add(department);
        context.Doctors.Add(doctor);
        await context.SaveChangesAsync();

        var controller = new AppointmentsController(context);
        var start = DateTime.UtcNow.AddDays(1);
        var model = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            StartTime = start,
            EndTime = start,
            Reason = "Consultation",
            Status = AppointmentStatus.Scheduled
        };

        var result = await controller.Create(model);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(model, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(Appointment.EndTime)));
        Assert.Equal(0, await context.Appointments.CountAsync());
    }

    [Fact]
    public async Task Create_WithValidModel_PersistsAndRedirects()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var patient = new Patient { FirstName = "A", LastName = "Patient" };
        var department = new Department { Name = "Neurology" };
        var doctor = new Doctor { FirstName = "B", LastName = "Doctor", Specialization = "Neuro", Department = department };

        context.Patients.Add(patient);
        context.Departments.Add(department);
        context.Doctors.Add(doctor);
        await context.SaveChangesAsync();

        var controller = new AppointmentsController(context);
        var start = DateTime.UtcNow.AddDays(1);
        var model = new Appointment
        {
            PatientId = patient.Id,
            DoctorId = doctor.Id,
            StartTime = start,
            EndTime = start.AddMinutes(30),
            Reason = "Follow-up",
            Status = AppointmentStatus.Scheduled
        };

        var result = await controller.Create(model);

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);

        var saved = await context.Appointments.SingleAsync();
        Assert.Equal(patient.Id, saved.PatientId);
        Assert.Equal(doctor.Id, saved.DoctorId);
        Assert.Equal("Follow-up", saved.Reason);
    }

    [Fact]
    public async Task Create_WithOverlappingDoctorAppointment_ReturnsViewAndDoesNotPersist()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var patient1 = new Patient { FirstName = "A", LastName = "Patient1" };
        var patient2 = new Patient { FirstName = "B", LastName = "Patient2" };
        var department = new Department { Name = "Radiology" };
        var doctor = new Doctor { FirstName = "C", LastName = "Doctor", Specialization = "Radio", Department = department };

        var existing = new Appointment
        {
            Patient = patient1,
            Doctor = doctor,
            StartTime = DateTime.UtcNow.AddDays(1).AddHours(10),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(10).AddMinutes(30),
            Reason = "Existing"
        };

        context.AddRange(patient1, patient2, department, doctor, existing);
        await context.SaveChangesAsync();

        var controller = new AppointmentsController(context);
        var overlapping = new Appointment
        {
            PatientId = patient2.Id,
            DoctorId = doctor.Id,
            StartTime = existing.StartTime.AddMinutes(15),
            EndTime = existing.EndTime.AddMinutes(15),
            Reason = "Overlapping"
        };

        var result = await controller.Create(overlapping);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(overlapping, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(Appointment.StartTime)));
        Assert.Equal(1, await context.Appointments.CountAsync());
    }

    [Fact]
    public async Task Edit_WithOverlappingDoctorAppointment_ReturnsViewAndDoesNotPersistChanges()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        await using var context = CreateContext(connection);
        await context.Database.EnsureCreatedAsync();

        var patient1 = new Patient { FirstName = "A", LastName = "Patient1" };
        var patient2 = new Patient { FirstName = "B", LastName = "Patient2" };
        var department = new Department { Name = "ENT" };
        var doctor = new Doctor { FirstName = "C", LastName = "Doctor", Specialization = "ENT", Department = department };

        var existing = new Appointment
        {
            Patient = patient1,
            Doctor = doctor,
            StartTime = DateTime.UtcNow.AddDays(1).AddHours(10),
            EndTime = DateTime.UtcNow.AddDays(1).AddHours(10).AddMinutes(30),
            Reason = "Existing"
        };

        var toEditStart = DateTime.UtcNow.AddDays(1).AddHours(11);
        var toEditEnd = toEditStart.AddMinutes(30);
        var toEdit = new Appointment
        {
            Patient = patient2,
            Doctor = doctor,
            StartTime = toEditStart,
            EndTime = toEditEnd,
            Reason = "Original"
        };

        context.AddRange(patient1, patient2, department, doctor, existing, toEdit);
        await context.SaveChangesAsync();

        var controller = new AppointmentsController(context);
        var edited = new Appointment
        {
            Id = toEdit.Id,
            PatientId = patient2.Id,
            DoctorId = doctor.Id,
            StartTime = existing.StartTime.AddMinutes(10),
            EndTime = existing.EndTime.AddMinutes(10),
            Reason = "Edited overlap"
        };

        var result = await controller.Edit(toEdit.Id, edited);

        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Same(edited, viewResult.Model);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(Appointment.StartTime)));

        var persisted = await context.Appointments.AsNoTracking().SingleAsync(a => a.Id == toEdit.Id);
        Assert.Equal("Original", persisted.Reason);
        Assert.Equal(toEditStart, persisted.StartTime);
        Assert.Equal(toEditEnd, persisted.EndTime);
    }

    private static ApplicationDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;
        return new ApplicationDbContext(options);
    }
}
