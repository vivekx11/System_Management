using CollegeEventPortal.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CollegeEventPortal.Models;

namespace CollegeEventPortal.Areas.Judge.Controllers
{
    [Area("Judge")]
    [Authorize(Roles = "Judge")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            
            var assignedEvents = await _context.EventJudges
                .Include(ej => ej.Event)
                .Where(ej => ej.JudgeId == userId)
                .Select(ej => ej.Event)
                .ToListAsync();

            return View(assignedEvents);
        }

        public async Task<IActionResult> EventDetails(int id)
        {
            var eventData = await _context.Events
                .Include(e => e.Registrations)
                    .ThenInclude(r => r.User)
                .Include(e => e.Teams)
                    .ThenInclude(t => t.Members)
                        .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (eventData == null)
                return NotFound();

            return View(eventData);
        }

        public async Task<IActionResult> ScoreParticipants(int eventId)
        {
            var eventData = await _context.Events
                .Include(e => e.Registrations)
                    .ThenInclude(r => r.User)
                .Include(e => e.Teams)
                    .ThenInclude(t => t.Members)
                        .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (eventData == null)
                return NotFound();

            ViewBag.Event = eventData;
            return View(eventData);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitScore(int eventId, string? participantId, int? teamId, int round, decimal points, string? remarks)
        {
            var userId = _userManager.GetUserId(User);

            var score = new Score
            {
                EventId = eventId,
                ParticipantId = participantId,
                TeamId = teamId,
                Round = round,
                Points = points,
                Remarks = remarks,
                JudgeId = userId!,
                ScoredAt = DateTime.UtcNow
            };

            _context.Scores.Add(score);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Score submitted successfully!";
            return RedirectToAction(nameof(ScoreParticipants), new { eventId });
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
                    TotalScore = g.Sum(s => s.Points),
                    Rounds = g.Select(s => s.Round).Distinct().Count()
                })
                .OrderByDescending(x => x.TotalScore)
                .ToList();

            ViewBag.Event = eventData;
            return View(leaderboard);
        }
    }
}
