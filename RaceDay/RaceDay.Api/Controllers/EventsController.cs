using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Filters;
using RaceDay.Api.Models;
using RaceDay.Contracts;

namespace RaceDay.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public EventsController(RaceDayDbContext context)
        {
            _context = context;
        }

        // GET: api/events (Public to view)
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            List<Event> events = await _context.Events.ToListAsync();
            List<EventDto> eventDtos = new List<EventDto>();

            foreach (Event item in events)
            {
                eventDtos.Add(new EventDto
                {
                    EventID = item.EventID,
                    OrganiserID = item.OrganiserID,
                    EventName = item.EventName,
                    Description = item.Description,
                    EventDate = item.EventDate,
                    Location = item.Location,
                    DistanceKm = item.DistanceKm,
                    EventType = item.EventType,
                    BannerUrl = item.BannerUrl
                });
            }

            return Ok(eventDtos);
        }

        // GET: api/events/{id} (Public to view)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            Event item = await _context.Events.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            EventDto dto = new EventDto
            {
                EventID = item.EventID,
                OrganiserID = item.OrganiserID,
                EventName = item.EventName,
                Description = item.Description,
                EventDate = item.EventDate,
                Location = item.Location,
                DistanceKm = item.DistanceKm,
                EventType = item.EventType,
                BannerUrl = item.BannerUrl
            };

            return Ok(dto);
        }

        // POST: api/events (Organiser only)
        [HttpPost]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid event data." });
            }

            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (!sessionUserId.HasValue)
            {
                return Unauthorized(new { message = "Organiser session expired or invalid." });
            }

            Event newEvent = new Event
            {
                OrganiserID = sessionUserId.Value,
                EventName = request.EventName,
                Description = request.Description,
                EventDate = request.EventDate,
                Location = request.Location,
                DistanceKm = request.DistanceKm,
                EventType = request.EventType,
                BannerUrl = request.BannerUrl ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            EventDto dto = new EventDto
            {
                EventID = newEvent.EventID,
                OrganiserID = newEvent.OrganiserID,
                EventName = newEvent.EventName,
                Description = newEvent.Description,
                EventDate = newEvent.EventDate,
                Location = newEvent.Location,
                DistanceKm = newEvent.DistanceKm,
                EventType = newEvent.EventType,
                BannerUrl = newEvent.BannerUrl
            };

            return StatusCode(StatusCodes.Status201Created, dto);
        }

        // PUT: api/events/{id} (Organiser only)
        [HttpPut("{id}")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventRequest request)
        {
            if (request == null)
            {
                return BadRequest(new { message = "Invalid event update data." });
            }

            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (!sessionUserId.HasValue)
            {
                return Unauthorized(new { message = "Organiser session expired or invalid." });
            }

            Event existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (existingEvent.OrganiserID != sessionUserId.Value)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only edit events you created." });
            }

            existingEvent.EventName = request.EventName;
            existingEvent.Description = request.Description;
            existingEvent.EventDate = request.EventDate;
            existingEvent.Location = request.Location;
            existingEvent.DistanceKm = request.DistanceKm;
            existingEvent.EventType = request.EventType;
            existingEvent.BannerUrl = request.BannerUrl ?? existingEvent.BannerUrl;

            await _context.SaveChangesAsync();

            EventDto dto = new EventDto
            {
                EventID = existingEvent.EventID,
                OrganiserID = existingEvent.OrganiserID,
                EventName = existingEvent.EventName,
                Description = existingEvent.Description,
                EventDate = existingEvent.EventDate,
                Location = existingEvent.Location,
                DistanceKm = existingEvent.DistanceKm,
                EventType = existingEvent.EventType,
                BannerUrl = existingEvent.BannerUrl
            };

            return Ok(dto);
        }

        // DELETE: api/events/{id} (Organiser only)
        [HttpDelete("{id}")]
        [AuthorizeRole("Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            if (!sessionUserId.HasValue)
            {
                return Unauthorized(new { message = "Organiser session expired or invalid." });
            }

            Event existingEvent = await _context.Events.FindAsync(id);
            if (existingEvent == null)
            {
                return NotFound(new { message = "Event not found." });
            }

            if (existingEvent.OrganiserID != sessionUserId.Value)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "You can only delete events you created." });
            }

            _context.Events.Remove(existingEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}