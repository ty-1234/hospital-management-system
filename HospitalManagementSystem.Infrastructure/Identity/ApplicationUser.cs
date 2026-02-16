using Microsoft.AspNetCore.Identity;

namespace HospitalManagementSystem.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
}

