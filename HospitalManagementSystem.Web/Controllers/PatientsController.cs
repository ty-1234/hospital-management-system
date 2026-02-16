using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Web.Controllers;

[Authorize]
public class PatientsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await dbContext.Patients
            .OrderBy(p => p.LastName)
            .ThenBy(p => p.FirstName)
            .ToListAsync());
    }

    public IActionResult Create()
    {
        return View(new Patient { DateOfBirth = DateTime.UtcNow.Date.AddYears(-25) });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Patient patient)
    {
        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        dbContext.Patients.Add(patient);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var patient = await dbContext.Patients.FindAsync(id);
        return patient is null ? NotFound() : View(patient);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Patient patient)
    {
        if (id != patient.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(patient);
        }

        dbContext.Update(patient);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var patient = await dbContext.Patients.FindAsync(id);
        return patient is null ? NotFound() : View(patient);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var patient = await dbContext.Patients.FindAsync(id);
        if (patient is not null)
        {
            dbContext.Patients.Remove(patient);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
