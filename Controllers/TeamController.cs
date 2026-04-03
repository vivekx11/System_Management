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
    public class TeamController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;

        public TeamController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var teams = await _context.TeamMembers
                .Include(tm => tm.Team)
                    .ThenInclude(t => t.Event)
                .Include(tm => tm.Team)
                    .ThenInclude(t => t.Members)
                        .ThenInclude(m => m.User)
                .Where(tm => tm.UserId == userId)
                .Select(tm => tm.Team)
                .ToListAsync();

            return View(teams);
        }

        [HttpGet]
        public async Task<IActionResult> Create(int eventId)
        {
            var eventData = await _context.Events.FindAsync(eventId);
            if (eventData == null || !eventData.AllowTeamRegistration)
                return NotFound();

            ViewBag.Event = eventData;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int eventId, string teamName)
        {
            var userId = _userManager.GetUserId(User);
            var eventData = await _context.Events.FindAsync(eventId);

            if (eventData == null || !eventData.AllowTeamRegistration)
                return NotFound();

            var inviteCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            var team = new Team
            {
                Name = teamName,
                EventId = eventId,
                LeaderId = userId!,
                InviteCode = inviteCode,
                Status = TeamStatus.Active,
                CreatedAt = DateTime.UtcNow
            };

            _context.Teams.Add(team);
            await _context.SaveChangesAsync();

            var teamMember = new TeamMember
            {
                TeamId = team.Id,
                UserId = userId!,
                IsLeader = true,
                JoinedAt = DateTime.UtcNow
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            var user = await _userManager.FindByIdAsync(userId!);
            if (!await _userManager.IsInRoleAsync(user!, "TeamLeader"))
            {
                await _userManager.AddToRoleAsync(user!, "TeamLeader");
            }

            TempData["Success"] = $"Team created successfully! Invite Code: {inviteCode}";
            return RedirectToAction(nameof(Details), new { id = team.Id });
        }

        public async Task<IActionResult> Details(int id)
        {
            var team = await _context.Teams
                .Include(t => t.Event)
                .Include(t => t.Leader)
                .Include(t => t.Members)
                    .ThenInclude(m => m.User)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (team == null)
                return NotFound();

            return View(team);
        }

        [HttpGet]
        public IActionResult Join()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Join(string inviteCode)
        {
            var team = await _context.Teams
                .Include(t => t.Event)
                .Include(t => t.Members)
                .FirstOrDefaultAsync(t => t.InviteCode == inviteCode.ToUpper());

            if (team == null)
            {
                TempData["Error"] = "Invalid invite code.";
                return View();
            }

            if (team.Members.Count >= team.Event.MaxTeamSize)
            {
                TempData["Error"] = "Team is full.";
                return View();
            }

            var userId = _userManager.GetUserId(User);
            var existingMember = await _context.TeamMembers
                .AnyAsync(tm => tm.TeamId == team.Id && tm.UserId == userId);

            if (existingMember)
            {
                TempData["Error"] = "You are already a member of this team.";
                return View();
            }

            var teamMember = new TeamMember
            {
                TeamId = team.Id,
                UserId = userId!,
                IsLeader = false,
                JoinedAt = DateTime.UtcNow
            };

            _context.TeamMembers.Add(teamMember);
            await _context.SaveChangesAsync();

            await _notificationService.SendNotificationAsync(userId!, "Joined Team", 
                $"You have successfully joined team {team.Name}", NotificationType.Success);

            TempData["Success"] = "Successfully joined the team!";
            return RedirectToAction(nameof(Details), new { id = team.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Leave(int teamId)
        {
            var userId = _userManager.GetUserId(User);
            var teamMember = await _context.TeamMembers
                .Include(tm => tm.Team)
                .FirstOrDefaultAsync(tm => tm.TeamId == teamId && tm.UserId == userId);

            if (teamMember == null)
                return NotFound();

            if (teamMember.IsLeader)
            {
                TempData["Error"] = "Team leader cannot leave the team. Please delete the team instead.";
                return RedirectToAction(nameof(Details), new { id = teamId });
            }

            _context.TeamMembers.Remove(teamMember);
            await _context.SaveChangesAsync();

            TempData["Success"] = "You have left the team.";
            return RedirectToAction(nameof(Index));
        }
    }
}
