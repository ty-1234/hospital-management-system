using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Web.Controllers;

[Authorize]
public class DoctorsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        var doctors = await dbContext.Doctors
            .Include(d => d.Department)
            .OrderBy(d => d.LastName)
            .ThenBy(d => d.FirstName)
            .ToListAsync();

        return View(doctors);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateDepartmentsAsync();
        return View(new Doctor());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Doctor doctor)
    {
        if (!ModelState.IsValid)
        {
            await PopulateDepartmentsAsync(doctor.DepartmentId);
            return View(doctor);
        }

        dbContext.Doctors.Add(doctor);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var doctor = await dbContext.Doctors.FindAsync(id);
        if (doctor is null)
        {
            return NotFound();
        }

        await PopulateDepartmentsAsync(doctor.DepartmentId);
        return View(doctor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Doctor doctor)
    {
        if (id != doctor.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            await PopulateDepartmentsAsync(doctor.DepartmentId);
            return View(doctor);
        }

        dbContext.Update(doctor);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var doctor = await dbContext.Doctors.Include(d => d.Department).FirstOrDefaultAsync(d => d.Id == id);
        return doctor is null ? NotFound() : View(doctor);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var doctor = await dbContext.Doctors.FindAsync(id);
        if (doctor is not null)
        {
            dbContext.Doctors.Remove(doctor);
            await dbContext.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateDepartmentsAsync(int? selectedDepartmentId = null)
    {
        var departments = await dbContext.Departments.OrderBy(d => d.Name).ToListAsync();
        ViewBag.Departments = new SelectList(departments, "Id", "Name", selectedDepartmentId);
    }
}
