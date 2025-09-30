using Microsoft.AspNetCore.Identity;
using TaskManagerShared.Models;

namespace TaskManagerAPI.Data
{
    public class RoleSeeder
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;

        public RoleSeeder(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _config = config;
        }

        public async Task SeedAsync()
        {
            var roles = new[] { "Admin", "User" };

            foreach (var role in roles)
                if (!await _roleManager.RoleExistsAsync(role))
                    await _roleManager.CreateAsync(new IdentityRole(role));

            var adminEmail = _config["Seed:AdminEmail"];
            var adminPassword = _config["Seed:AdminPassword"];

            if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
            {
                var admin = await _userManager.FindByEmailAsync(adminEmail);
                if (admin == null)
                {
                    admin = new ApplicationUser
                    {
                        Email = adminEmail,
                        UserName = adminEmail,
                        EmailConfirmed = true,
                        FullName = _config["Seed:AdminFullName"] ?? "Admin",
                        ContactNo = _config["Seed:AdminContactNo"] ?? "0000000000"
                    };

                    var result = await _userManager.CreateAsync(admin, adminPassword);
                    if (result.Succeeded)
                    {
                        await _userManager.AddToRoleAsync(admin, "Admin");
                    }
                }
            }
        }
    }
}
