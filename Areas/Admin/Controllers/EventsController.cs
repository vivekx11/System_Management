using CollegeEventPortal.Data;
using CollegeEventPortal.Models;
using CollegeEventPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeEventPortal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public EventsController(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var events = await _context.Events
                .OrderByDescending(e => e.CreatedAt)
                .ToListAsync();

            return View(events);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Event model)
        {
            if (ModelState.IsValid)
            {
                model.CreatedAt = DateTime.UtcNow;
                _context.Events.Add(model);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Event created successfully!";
                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var eventData = await _context.Events.FindAsync(id);
            if (eventData == null)
                return NotFound();

            return View(eventData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Event model)
        {
            if (id != model.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    model.UpdatedAt = DateTime.UtcNow;
                    _context.Update(model);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Event updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await EventExists(model.Id))
                        return NotFound();
                    throw;
                }
            }

            return View(model);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var eventData = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventData == null)
                return NotFound();

            return View(eventData);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var eventData = await _context.Events.FindAsync(id);
            if (eventData != null)
            {
                _context.Events.Remove(eventData);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Event deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Registrations(int id)
        {
            var eventData = await _context.Events.FindAsync(id);
            if (eventData == null)
                return NotFound();

            var registrations = await _context.Registrations
                .Include(r => r.User)
                .Include(r => r.Team)
                .Where(r => r.EventId == id)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            ViewBag.Event = eventData;
            return View(registrations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveRegistration(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
                return NotFound();

            registration.Status = RegistrationStatus.Approved;
            registration.ApprovedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(registration.UserId, 
                "Registration Approved", 
                $"Your registration for {registration.Event.Name} has been approved!", 
                NotificationType.Success);

            TempData["Success"] = "Registration approved!";
            return RedirectToAction(nameof(Registrations), new { id = registration.EventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectRegistration(int id)
        {
            var registration = await _context.Registrations
                .Include(r => r.Event)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (registration == null)
                return NotFound();

            registration.Status = RegistrationStatus.Rejected;
            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(registration.UserId, 
                "Registration Rejected", 
                $"Your registration for {registration.Event.Name} has been rejected.", 
                NotificationType.Warning);

            TempData["Success"] = "Registration rejected!";
            return RedirectToAction(nameof(Registrations), new { id = registration.EventId });
        }

        public async Task<IActionResult> AssignJudges(int id)
        {
            var eventData = await _context.Events
                .Include(e => e.EventJudges)
                    .ThenInclude(ej => ej.Judge)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventData == null)
                return NotFound();

            var judges = await _context.Users
                .Where(u => u.IsActive)
                .ToListAsync();

            var judgeRoles = new List<ApplicationUser>();
            foreach (var user in judges)
            {
                var roles = await _context.UserRoles
                    .Where(ur => ur.UserId == user.Id)
                    .Join(_context.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync();

                if (roles.Contains("Judge"))
                    judgeRoles.Add(user);
            }

            ViewBag.Event = eventData;
            ViewBag.AvailableJudges = judgeRoles;

            return View(eventData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignJudge(int eventId, string judgeId)
        {
            var existing = await _context.EventJudges
                .AnyAsync(ej => ej.EventId == eventId && ej.JudgeId == judgeId);

            if (existing)
            {
                TempData["Error"] = "Judge already assigned to this event.";
                return RedirectToAction(nameof(AssignJudges), new { id = eventId });
            }

            var eventJudge = new EventJudge
            {
                EventId = eventId,
                JudgeId = judgeId,
                AssignedAt = DateTime.UtcNow
            };

            _context.EventJudges.Add(eventJudge);
            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(judgeId, 
                "Judge Assignment", 
                "You have been assigned as a judge for an event.", 
                NotificationType.Info);

            TempData["Success"] = "Judge assigned successfully!";
            return RedirectToAction(nameof(AssignJudges), new { id = eventId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PublishResults(int id)
        {
            var eventData = await _context.Events.FindAsync(id);
            if (eventData == null)
                return NotFound();

            eventData.Status = EventStatus.Completed;
            await _context.SaveChangesAsync();

            var participants = await _context.Registrations
                .Where(r => r.EventId == id && r.Status == RegistrationStatus.Approved)
                .Select(r => r.UserId)
                .ToListAsync();

            await _notificationService.SendBulkNotificationAsync(participants, 
                "Results Published", 
                $"Results for {eventData.Name} have been published!", 
                NotificationType.Success);

            TempData["Success"] = "Results published successfully!";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> EventExists(int id)
        {
            return await _context.Events.AnyAsync(e => e.Id == id);
        }
    }
}
