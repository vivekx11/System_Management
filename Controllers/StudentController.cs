using CollegeEventPortal.Data;
using CollegeEventPortal.Models;
using CollegeEventPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeEventPortal.Controllers
{
    [Authorize(Roles = "Student,TeamLeader")]
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public StudentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            var upcomingEvents = await _context.Events
                .Where(e => e.Status == EventStatus.RegistrationOpen && e.RegistrationDeadline > DateTime.UtcNow)
                .OrderBy(e => e.StartDate)
                .Take(6)
                .ToListAsync();

            var myRegistrations = await _context.Registrations
                .Include(r => r.Event)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RegisteredAt)
                .Take(5)
                .ToListAsync();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId!, 5);

            ViewBag.User = user;
            ViewBag.UpcomingEvents = upcomingEvents;
            ViewBag.MyRegistrations = myRegistrations;
            ViewBag.Notifications = notifications;

            return View();
        }

        public async Task<IActionResult> Events()
        {
            var events = await _context.Events
                .Where(e => e.Status == EventStatus.RegistrationOpen || e.Status == EventStatus.Published)
                .OrderBy(e => e.StartDate)
                .ToListAsync();

            return View(events);
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            var eventData = await _context.Events
                .Include(e => e.Registrations)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventData == null)
                return NotFound();

            var userId = _userManager.GetUserId(User);
            var isRegistered = await _context.Registrations
                .AnyAsync(r => r.EventId == id && r.UserId == userId);

            ViewBag.IsRegistered = isRegistered;

            return View(eventData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterForEvent(int eventId)
        {
            var userId = _userManager.GetUserId(User);
            var eventData = await _context.Events.FindAsync(eventId);

            if (eventData == null)
                return NotFound();

            var existingRegistration = await _context.Registrations
                .FirstOrDefaultAsync(r => r.EventId == eventId && r.UserId == userId);

            if (existingRegistration != null)
            {
                TempData["Error"] = "You are already registered for this event.";
                return RedirectToAction(nameof(EventDetails), new { id = eventId });
            }

            var registration = new Registration
            {
                EventId = eventId,
                UserId = userId!,
                Status = RegistrationStatus.Pending,
                RegisteredAt = DateTime.UtcNow
            };

            _context.Registrations.Add(registration);
            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(userId!, "Registration Successful", 
                $"You have successfully registered for {eventData.Name}", NotificationType.Success);

            TempData["Success"] = "Registration submitted successfully!";
            return RedirectToAction(nameof(MyRegistrations));
        }

        public async Task<IActionResult> MyRegistrations()
        {
            var userId = _userManager.GetUserId(User);
            var registrations = await _context.Registrations
                .Include(r => r.Event)
                .Include(r => r.Team)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.RegisteredAt)
                .ToListAsync();

            return View(registrations);
        }

        public async Task<IActionResult> Profile()
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId!);

            return View(user);
        }

        public async Task<IActionResult> Certificates()
        {
            var userId = _userManager.GetUserId(User);
            var certificates = await _context.Certificates
                .Include(c => c.Event)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.IssuedAt)
                .ToListAsync();

            return View(certificates);
        }

        public async Task<IActionResult> Leaderboard(int eventId)
        {
            var eventData = await _context.Events.FindAsync(eventId);
            if (eventData == null)
                return NotFound();

            var scores = await _context.Scores
                .Include(s => s.Participant)
                .Include(s => s.Team)
                .Where(s => s.EventId == eventId)
                .ToListAsync();

            var leaderboard = scores
                .GroupBy(s => eventData.AllowTeamRegistration ? (object?)s.TeamId : (object?)s.ParticipantId)
                .Select(g => new
                {
                    Id = g.Key,
                    Name = eventData.AllowTeamRegistration ? g.First().Team!.Name : g.First().Participant!.FullName,
                    TotalScore = g.Sum(s => s.Points)
                })
                .OrderByDescending(x => x.TotalScore)
                .ToList();

            ViewBag.Event = eventData;
            return View(leaderboard);
        }
    }
}
