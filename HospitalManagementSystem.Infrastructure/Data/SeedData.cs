using HospitalManagementSystem.Core.Entities;
using HospitalManagementSystem.Core.Enums;
using HospitalManagementSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalManagementSystem.Infrastructure.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
        await context.Database.EnsureCreatedAsync();

        if (!await context.Departments.AnyAsync())
        {
            var departments = new[]
            {
                new Department { Name = "Cardiology", Location = "Block A - Floor 2" },
                new Department { Name = "Neurology", Location = "Block B - Floor 3" },
                new Department { Name = "Orthopedics", Location = "Block C - Floor 1" },
                new Department { Name = "Pediatrics", Location = "Block A - Floor 1" }
            };
            context.Departments.AddRange(departments);
            await context.SaveChangesAsync();
        }

        if (!await context.Doctors.AnyAsync())
        {
            var cardiologyId = await context.Departments.Where(d => d.Name == "Cardiology").Select(d => d.Id).FirstAsync();
            var neurologyId = await context.Departments.Where(d => d.Name == "Neurology").Select(d => d.Id).FirstAsync();

            var doctors = new[]
            {
                new Doctor
                {
                    FirstName = "Ayesha",
                    LastName = "Khan",
                    Specialization = "Cardiologist",
                    Email = "ayesha.khan@hospital.local",
                    PhoneNumber = "555-1001",
                    DepartmentId = cardiologyId
                },
                new Doctor
                {
                    FirstName = "Daniel",
                    LastName = "Nguyen",
                    Specialization = "Neurologist",
                    Email = "daniel.nguyen@hospital.local",
                    PhoneNumber = "555-1002",
                    DepartmentId = neurologyId
                }
            };
            context.Doctors.AddRange(doctors);
            await context.SaveChangesAsync();
        }

        if (!await context.Patients.AnyAsync())
        {
            var patients = new[]
            {
                new Patient
                {
                    FirstName = "Noah",
                    LastName = "Ali",
                    DateOfBirth = new DateTime(1994, 4, 21),
                    Gender = "Male",
                    PhoneNumber = "555-2001",
                    Email = "noah.ali@example.com",
                    Address = "401 Main Street",
                    EmergencyContactName = "Sara Ali",
                    EmergencyContactPhone = "555-2002"
                },
                new Patient
                {
                    FirstName = "Zara",
                    LastName = "Ibrahim",
                    DateOfBirth = new DateTime(1988, 9, 5),
                    Gender = "Female",
                    PhoneNumber = "555-2003",
                    Email = "zara.ibrahim@example.com",
                    Address = "89 Oak Avenue",
                    EmergencyContactName = "Yusuf Ibrahim",
                    EmergencyContactPhone = "555-2004"
                }
            };

            context.Patients.AddRange(patients);
            await context.SaveChangesAsync();
        }

        if (!await context.Appointments.AnyAsync())
        {
            var firstPatientId = await context.Patients.Select(p => p.Id).FirstAsync();
            var firstDoctorId = await context.Doctors.Select(d => d.Id).FirstAsync();
            var startTime = DateTime.UtcNow.AddDays(1).AddHours(2);

            context.Appointments.Add(new Appointment
            {
                PatientId = firstPatientId,
                DoctorId = firstDoctorId,
                StartTime = startTime,
                EndTime = startTime.AddMinutes(30),
                Reason = "Initial consultation",
                Status = AppointmentStatus.Scheduled,
                Notes = "Bring previous reports"
            });
            await context.SaveChangesAsync();
        }

        if (!await context.Bills.AnyAsync())
        {
            var firstPatientId = await context.Patients.Select(p => p.Id).FirstAsync();
            var firstAppointmentId = await context.Appointments.Select(a => a.Id).FirstAsync();

            context.Bills.Add(new Bill
            {
                PatientId = firstPatientId,
                AppointmentId = firstAppointmentId,
                Amount = 250.00m,
                IssuedOn = DateTime.UtcNow.Date,
                PaymentDueDate = DateTime.UtcNow.Date.AddDays(14),
                IsPaid = false,
                Description = "Consultation fee"
            });
            await context.SaveChangesAsync();
        }

        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var adminEmail = "admin@hospital.local";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FullName = "System Administrator"
            };

            await userManager.CreateAsync(adminUser, "Admin@12345");
        }
    }
}
