using CollegeEventPortal.Data;
using CollegeEventPortal.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CollegeEventPortal.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var upcomingEvents = await _context.Events
                .Where(e => e.Status == EventStatus.Published || e.Status == EventStatus.RegistrationOpen)
                .OrderBy(e => e.StartDate)
                .Take(6)
                .ToListAsync();

            var totalEvents = await _context.Events.CountAsync();
            var totalParticipants = await _context.Users.CountAsync(u => u.IsActive);
            var ongoingEvents = await _context.Events.CountAsync(e => e.Status == EventStatus.Ongoing);

            ViewBag.UpcomingEvents = upcomingEvents;
            ViewBag.TotalEvents = totalEvents;
            ViewBag.TotalParticipants = totalParticipants;
            ViewBag.OngoingEvents = ongoingEvents;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
