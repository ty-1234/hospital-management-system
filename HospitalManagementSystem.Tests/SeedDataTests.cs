using HospitalManagementSystem.Infrastructure.Data;
using HospitalManagementSystem.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HospitalManagementSystem.Tests;

public class SeedDataTests
{
    [Fact]
    public async Task InitializeAsync_CreatesBaselineData_And_IsIdempotent()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlite(connection));
        services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        using var provider = services.BuildServiceProvider();

        await SeedData.InitializeAsync(provider);
        await SeedData.InitializeAsync(provider);

        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        Assert.Equal(4, await context.Departments.CountAsync());
        Assert.Equal(2, await context.Doctors.CountAsync());
        Assert.Equal(2, await context.Patients.CountAsync());
        Assert.Equal(1, await context.Appointments.CountAsync());
        Assert.Equal(1, await context.Bills.CountAsync());

        var admin = await userManager.FindByEmailAsync("admin@hospital.local");
        Assert.NotNull(admin);
        Assert.Equal("System Administrator", admin!.FullName);
    }
}
