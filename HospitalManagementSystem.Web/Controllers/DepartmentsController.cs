using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HospitalManagementSystem.Web.Controllers;

[Authorize]
public class DepartmentsController(ApplicationDbContext dbContext) : Controller
{
    public async Task<IActionResult> Index()
    {
        return View(await dbContext.Departments.OrderBy(d => d.Name).ToListAsync());
    }

    public IActionResult Create() => View(new Department());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Department department)
    {
        if (!ModelState.IsValid)
        {
            return View(department);
        }

        dbContext.Departments.Add(department);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var department = await dbContext.Departments.FindAsync(id);
        return department is null ? NotFound() : View(department);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Department department)
    {
        if (id != department.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(department);
        }

        dbContext.Update(department);
        await dbContext.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var department = await dbContext.Departments.FindAsync(id);
        return department is null ? NotFound() : View(department);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var department = await dbContext.Departments.FindAsync(id);
        if (department is not null)
        {
            dbContext.Departments.Remove(department);
            await dbContext.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
