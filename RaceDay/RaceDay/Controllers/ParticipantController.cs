using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class ParticipantController : Controller
    {
        private readonly RaceDayApiClient _api;

        public ParticipantController(RaceDayApiClient api)
        {
            _api = api;
        }

        private bool IsParticipant() =>
            string.Equals(HttpContext.Session.GetString("Role"), "Participant", StringComparison.OrdinalIgnoreCase);

        
        public async Task<IActionResult> Index(string? eventType, string? search)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            var res = await _api.SendAsync(HttpMethod.Get, "api/events");
            var events = await _api.ReadAsync<List<EventDto>>(res) ?? new();

            
            events = events.Where(e => e.EventDate >= DateTime.Today).ToList();

            if (!string.IsNullOrWhiteSpace(eventType))
                events = events.Where(e => e.EventType.Equals(eventType, StringComparison.OrdinalIgnoreCase)).ToList();

            if (!string.IsNullOrWhiteSpace(search))
                events = events.Where(e => e.EventName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                           e.Location.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();

            return View(events);
        }

        
        public async Task<IActionResult> EventDetails(int id)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            var eventRes = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}");
            if (!eventRes.IsSuccessStatusCode)
            {
                TempData["Error"] = "Event not found.";
                return RedirectToAction(nameof(Index));
            }

            var eventDto = await _api.ReadAsync<EventDto>(eventRes);

            var catRes = await _api.SendAsync(HttpMethod.Get, $"api/events/{id}/categories");
            ViewBag.Categories = await _api.ReadAsync<List<CategoryDto>>(catRes) ?? new();

            return View(eventDto);
        }

        
        [HttpPost]
        public async Task<IActionResult> Enrol(int eventId, int categoryId)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            var req = new CreateEnrolmentRequest { EventID = eventId, CategoryID = categoryId };
            var res = await _api.SendAsync(HttpMethod.Post, "api/enrolments", req);

            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to enter event. You may already be registered.";
                return RedirectToAction(nameof(EventDetails), new { id = eventId });
            }

            TempData["Success"] = "Enrolled successfully!";
            return RedirectToAction(nameof(MyEvents));
        }

       
        public async Task<IActionResult> MyEvents()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            var res = await _api.SendAsync(HttpMethod.Get, "api/enrolments/mine");
            var list = await _api.ReadAsync<List<EnrolmentDto>>(res) ?? new();
            return View(list);
        }

        
        public async Task<IActionResult> MyResults()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            var res = await _api.SendAsync(HttpMethod.Get, "api/results/mine");
            var list = await _api.ReadAsync<List<ResultDto>>(res) ?? new();
            return View(list);
        }

      
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            var res = await _api.SendAsync(HttpMethod.Get, "api/profile/me");
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = "Could not load profile.";
                return RedirectToAction(nameof(Index));
            }

            var profile = await _api.ReadAsync<UserProfileDto>(res);
            return View(profile);
        }

       
        [HttpPost]
        public async Task<IActionResult> Profile(UserProfileDto model, IFormFile? profilePicture)
        {
            if (!IsParticipant()) return RedirectToAction("Login", "Auth");

            
            if (profilePicture != null && profilePicture.Length > 0)
            {
                var uploadRes = await _api.UploadFileAsync("api/uploads/avatar", profilePicture);
                if (uploadRes.IsSuccessStatusCode)
                {
                    var uploadResult = await _api.ReadAsync<UploadResult>(uploadRes);
                    if (uploadResult != null && !string.IsNullOrWhiteSpace(uploadResult.Url))
                    {
                        model.ProfilePicUrl = uploadResult.Url;
                    }
                }
            }

            var updateRes = await _api.SendAsync(HttpMethod.Put, "api/profile/me", model);
            if (!updateRes.IsSuccessStatusCode)
            {
                TempData["Error"] = "Failed to update profile.";
                return View(model);
            }

            
            HttpContext.Session.SetString("DisplayName", model.FullName);

            TempData["Success"] = "Profile updated successfully!";
            return RedirectToAction(nameof(Profile));
        }

        private class UploadResult
        {
            public string Url { get; set; } = string.Empty;
        }
    }
}