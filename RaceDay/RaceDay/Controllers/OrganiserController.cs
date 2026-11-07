using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Contracts;
using RaceDay.Mvc.Services;

namespace RaceDay.Mvc.Controllers
{
    public class OrganiserController : Controller
    {
        private readonly RaceDayApiClient _api;

        public OrganiserController(RaceDayApiClient api)
        {
            _api = api;
        }

        private bool IsOrganiser() => HttpContext.Session.GetString("Role") == "Organiser";

        public async Task<IActionResult> Dashboard()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            var res = await _api.SendAsync(HttpMethod.Get, "api/events");
            var events = await _api.ReadAsync<List<EventDto>>(res) ?? new();
            return View(events);
        }

        [HttpGet]
        public IActionResult CreateEvent()
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventRequest req, IFormFile? bannerImage)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");

            // Upload banner through API if supplied
            if (bannerImage != null && bannerImage.Length > 0)
            {
                var uploadRes = await _api.UploadFileAsync("api/uploads/banner", bannerImage);
                if (uploadRes.IsSuccessStatusCode)
                {
                    var result = await _api.ReadAsync<UploadResult>(uploadRes);
                    req.BannerUrl = result?.Url;
                }
            }

            var res = await _api.SendAsync(HttpMethod.Post, "api/events", req);
            if (!res.IsSuccessStatusCode)
            {
                ModelState.AddModelError(string.Empty, "Failed to create event.");
                return View(req);
            }

            TempData["Success"] = "Event created successfully.";
            return RedirectToAction(nameof(Dashboard));
        }

        public async Task<IActionResult> Categories(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            ViewBag.EventId = eventId;
            var res = await _api.SendAsync(HttpMethod.Get, $"api/events/{eventId}/categories");
            var categories = await _api.ReadAsync<List<CategoryDto>>(res) ?? new();
            return View(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory(int eventId, CreateCategoryRequest req)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            await _api.SendAsync(HttpMethod.Post, $"api/events/{eventId}/categories", req);
            return RedirectToAction(nameof(Categories), new { eventId });
        }

        public async Task<IActionResult> Enrolments(int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            ViewBag.EventId = eventId;
            var res = await _api.SendAsync(HttpMethod.Get, $"api/enrolments/event/{eventId}");
            var list = await _api.ReadAsync<List<EnrolmentDto>>(res) ?? new();
            return View(list);
        }

        [HttpGet]
        public IActionResult CaptureResult(int enrolmentId, int participantId, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            ViewBag.EventId = eventId;
            return View(new CreateResultRequest { EnrolmentID = enrolmentId, ParticipantID = participantId });
        }

        [HttpPost]
        public async Task<IActionResult> CaptureResult(CreateResultRequest req, int eventId)
        {
            if (!IsOrganiser()) return RedirectToAction("Login", "Auth");
            var res = await _api.SendAsync(HttpMethod.Post, "api/results", req);
            if (!res.IsSuccessStatusCode)
            {
                TempData["Error"] = "Unable to record result.";
                return View(req);
            }
            TempData["Success"] = "Participant result published.";
            return RedirectToAction(nameof(Enrolments), new { eventId });
        }

        private class UploadResult { public string Url { get; set; } = string.Empty; }
    }
}