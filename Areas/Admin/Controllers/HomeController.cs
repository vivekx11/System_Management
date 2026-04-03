using CollegeEventPortal.Data;
using CollegeEventPortal.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CollegeEventPortal.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var totalUsers = await _context.Users.CountAsync();
            var totalEvents = await _context.Events.CountAsync();
            var activeEvents = await _context.Events.CountAsync(e => e.Status == EventStatus.Ongoing);
            var totalRegistrations = await _context.Registrations.CountAsync();

            var recentActivities = await _context.ActivityLogs
                .Include(a => a.User)
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();

            var upcomingEvents = await _context.Events
                .Where(e => e.StartDate > DateTime.UtcNow)
                .OrderBy(e => e.StartDate)
                .Take(5)
                .ToListAsync();

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalEvents = totalEvents;
            ViewBag.ActiveEvents = activeEvents;
            ViewBag.TotalRegistrations = totalRegistrations;
            ViewBag.RecentActivities = recentActivities;
            ViewBag.UpcomingEvents = upcomingEvents;

            return View();
        }

        public async Task<IActionResult> Analytics()
        {
            var eventStats = await _context.Events
                .Select(e => new
                {
                    e.Name,
                    RegistrationCount = e.Registrations.Count,
                    e.EventType
                })
                .ToListAsync();

            var registrationsByMonth = await _context.Registrations
                .GroupBy(r => new { r.RegisteredAt.Year, r.RegisteredAt.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .OrderBy(x => x.Year)
                .ThenBy(x => x.Month)
                .ToListAsync();

            ViewBag.EventStats = eventStats;
            ViewBag.RegistrationsByMonth = registrationsByMonth;

            return View();
        }
    }
}
