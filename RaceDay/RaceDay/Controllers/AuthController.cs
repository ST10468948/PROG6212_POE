using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;
using RaceDay.Mvc.ViewModels;

namespace RaceDay.Mvc.Controllers
{
    public class AuthController : Controller
    {
        private readonly RaceDayApiClient _api;

        public AuthController(RaceDayApiClient api)
        {
            _api = api;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var req = new RegisterRequest
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Password = vm.Password,
                Role = vm.Role
            };

            var res = await _api.SendAsync(HttpMethod.Post, "api/auth/register", req);
            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, await res.Content.ReadAsStringAsync());
                return View(vm);
            }

            TempData["Success"] = "Account created successfully. Please login.";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var req = new LoginRequest { Email = vm.Email, Password = vm.Password };
            var res = await _api.SendAsync(HttpMethod.Post, "api/auth/login", req);

            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Invalid login credentials.");
                return View(vm);
            }

            var auth = await _api.ReadAsync<AuthResponse>(res);
            if (auth == null)
            {
                ModelState.AddModelError(string.Empty, "Unable to read server response.");
                return View(vm);
            }

            // Capture and preserve the API session cookie
            string? apiCookie = RaceDayApiClient.ExtractSessionCookie(res);
            if (!string.IsNullOrWhiteSpace(apiCookie))
            {
                HttpContext.Session.SetString("ApiSessionCookie", apiCookie);
            }

            HttpContext.Session.SetInt32("UserId", auth.UserId);
            HttpContext.Session.SetString("Role", auth.Role);
            HttpContext.Session.SetString("DisplayName", auth.FullName);

            if (auth.Role.Equals("Organiser", StringComparison.OrdinalIgnoreCase))
                return RedirectToAction("Dashboard", "Organiser");

            return RedirectToAction("Index", "Participant");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _api.SendAsync(HttpMethod.Post, "api/auth/logout");
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}