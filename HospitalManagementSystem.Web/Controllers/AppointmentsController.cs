using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.Core.Enums;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Web.Controllers;

[Authorize]
public class AppointmentsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var appointments = await dbContext.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .OrderBy(a => a.StartTime)
            .ToListAsync();

        return View(appointments);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        var baseTime = DateTime.UtcNow.AddDays(1).Date.AddHours(10);
        return View(new Appointment
        {
            StartTime = baseTime,
            EndTime = baseTime.AddMinutes(30),
            Status = AppointmentStatus.Scheduled
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Appointment appointment)
    {
        ValidateAppointmentTimes(appointment);
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }

        dbContext.Appointments.Add(appointment);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var appointment = await dbContext.Appointments.FindAsync(id);
        if (appointment is null)
        {
            return NotFound();
        }
        await PopulateLookupsAsync(appointment.PatientId, appointment.DoctorId);
        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Appointment appointment)
    {
        if (id != appointment.Id)
        {
            return BadRequest();
        }

        ValidateAppointmentTimes(appointment);
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(appointment.PatientId, appointment.DoctorId);
            return View(appointment);
        }

        dbContext.Update(appointment);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var appointment = await dbContext.Appointments
            .Include(a => a.Patient)
            .Include(a => a.Doctor)
            .FirstOrDefaultAsync(a => a.Id == id);

        return appointment is null ? NotFound() : View(appointment);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var appointment = await dbContext.Appointments.FindAsync(id);
        if (appointment is not null)
        {
            dbContext.Appointments.Remove(appointment);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private void ValidateAppointmentTimes(Appointment appointment)
    {
        if (appointment.EndTime <= appointment.StartTime)
        {
            ModelState.AddModelError(nameof(Appointment.EndTime), "End time must be after start time.");
        }
    }

    private async Task PopulateLookupsAsync(int? selectedPatientId = null, int? selectedDoctorId = null)
    {
        var patients = await dbContext.Patients
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();

        var doctors = await dbContext.Doctors
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .ToListAsync();

        ViewBag.Patients = new SelectList(patients, "Id", "FullName", selectedPatientId);
        ViewBag.Doctors = new SelectList(doctors, "Id", "FullName", selectedDoctorId);
        ViewBag.Statuses = new SelectList(Enum.GetValues<AppointmentStatus>());
    }
}
