using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Web.Controllers;

[Authorize]
public class BillsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var bills = await dbContext.Bills
            .Include(b => b.Patient)
            .Include(b => b.Appointment)
            .OrderByDescending(b => b.IssuedOn)
            .ToListAsync();
        return View(bills);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateLookupsAsync();
        return View(new Bill());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Bill bill)
    {
        await ValidateAppointmentOwnershipAsync(bill);
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(bill.PatientId, bill.AppointmentId);
            return View(bill);
        }

        dbContext.Bills.Add(bill);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var bill = await dbContext.Bills.FindAsync(id);
        if (bill is null)
        {
            return NotFound();
        }
        await PopulateLookupsAsync(bill.PatientId, bill.AppointmentId);
        return View(bill);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Bill bill)
    {
        if (id != bill.Id)
        {
            return BadRequest();
        }

        await ValidateAppointmentOwnershipAsync(bill);
        if (!ModelState.IsValid)
        {
            await PopulateLookupsAsync(bill.PatientId, bill.AppointmentId);
            return View(bill);
        }

        dbContext.Update(bill);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var bill = await dbContext.Bills
            .Include(b => b.Patient)
            .Include(b => b.Appointment)
            .FirstOrDefaultAsync(b => b.Id == id);

        return bill is null ? NotFound() : View(bill);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var bill = await dbContext.Bills.FindAsync(id);
        if (bill is not null)
        {
            dbContext.Bills.Remove(bill);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateLookupsAsync(int? selectedPatientId = null, int? selectedAppointmentId = null)
    {
        var patients = await dbContext.Patients
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync();

        var appointments = await dbContext.Appointments
            .Include(a => a.Patient)
            .OrderByDescending(a => a.StartTime)
            .ToListAsync();

        ViewBag.Patients = new SelectList(patients, "Id", "FullName", selectedPatientId);
        ViewBag.Appointments = new SelectList(
            appointments.Select(a => new
            {
                a.Id,
                Label = $"{a.Id} - {a.Patient!.FullName} - {a.StartTime:g}"
            }),
            "Id",
            "Label",
            selectedAppointmentId);
    }

    private async Task ValidateAppointmentOwnershipAsync(Bill bill)
    {
        if (!bill.AppointmentId.HasValue)
        {
            return;
        }

        var appointment = await dbContext.Appointments
            .AsNoTracking()
            .Where(a => a.Id == bill.AppointmentId.Value)
            .Select(a => new { a.PatientId })
            .FirstOrDefaultAsync();

        if (appointment is null)
        {
            ModelState.AddModelError(nameof(Bill.AppointmentId), "Selected appointment does not exist.");
            return;
        }

        if (appointment.PatientId != bill.PatientId)
        {
            ModelState.AddModelError(nameof(Bill.AppointmentId),
                "Selected appointment must belong to the selected patient.");
        }
    }
}
