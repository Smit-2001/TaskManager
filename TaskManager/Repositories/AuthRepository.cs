using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TaskManagerShared.Models;

namespace TaskManagerAPI.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly UserManager<ApplicationUser> _users;
        private readonly RoleManager<IdentityRole> _roles;
        private readonly IConfiguration _config;

        public AuthRepository(UserManager<ApplicationUser> users, RoleManager<IdentityRole> roles, IConfiguration config)
        {
            _users = users;
            _roles = roles;
            _config = config;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model)
        {
            var user = new ApplicationUser
            {
                Email = model.Email,
                UserName = model.Email,
                FullName = model.FullName,
                ContactNo = model.ContactNo,
                EmailConfirmed = true
            };

            var result = await _users.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return (false, string.Join("; ", result.Errors.Select(e => e.Description)));

            await _users.AddToRoleAsync(user, "User");
            return (true, "Registered");
        }

        public async Task<(bool Success, string Token)> LoginAsync(LoginViewModel model)
        {
            var user = await _users.FindByEmailAsync(model.Email);
            if (user == null) return (false, "Invalid credentials");

            if (!await _users.CheckPasswordAsync(user, model.Password))
                return (false, "Invalid credentials");

            var roles = await _users.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email ?? ""),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FullName", user.FullName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                expires: DateTime.UtcNow.AddHours(2),
                claims: claims,
                signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
            );

            return (true, new JwtSecurityTokenHandler().WriteToken(token));
        }
    }
}
