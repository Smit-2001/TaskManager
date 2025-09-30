using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using System.Text;
using TaskManagerShared.Models;
using TaskManagerMVC.Repositories;

namespace TaskManagerMVC.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthService _auth;

        public AccountController(IAuthService auth)
        {
            _auth = auth;
        }

        public IActionResult Register() => View();
        public IActionResult Login() => View();
        public IActionResult AccessDenied() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _auth.RegisterAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError("", "Registration failed: " + result.ErrorMessage);
                return View(model);
            }

            return RedirectToAction("Login");
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var result = await _auth.LoginAsync(model);
            if (!result.Success)
            {
                ModelState.AddModelError("", result.ErrorMessage);
                return View(model);
            }

            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(result.Token);

            var claims = new List<Claim>();

            var nameClaim = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)
                           ?? jwt.Claims.FirstOrDefault(c => c.Type == "unique_name")
                           ?? jwt.Claims.FirstOrDefault(c => c.Type == "sub");
            if (nameClaim != null)
                claims.Add(new Claim(ClaimTypes.Name, nameClaim.Value));

            var fullNameClaim = jwt.Claims.FirstOrDefault(c => c.Type == "FullName");
            if (fullNameClaim != null)
                claims.Add(new Claim("FullName", fullNameClaim.Value));

            claims.AddRange(jwt.Claims.Where(c => c.Type == ClaimTypes.Role).Select(rc => new Claim(ClaimTypes.Role, rc.Value)));

            claims.Add(new Claim("JwtToken", result.Token));

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            return RedirectToAction("Index", "Tasks");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }
    }
}
