using System.ComponentModel.DataAnnotations;

namespace CollegeEventPortal.Models
{
    public class Event
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;
        
        public string Description { get; set; } = string.Empty;
        
        [Required]
        public EventType EventType { get; set; }
        
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime RegistrationDeadline { get; set; }
        
        public int MaxParticipants { get; set; }
        public int MaxTeamSize { get; set; } = 1;
        public bool AllowSoloRegistration { get; set; } = true;
        public bool AllowTeamRegistration { get; set; } = false;
        
        public string? Venue { get; set; }
        public string? Rules { get; set; }
        public string? ImageUrl { get; set; }
        
        public EventStatus Status { get; set; } = EventStatus.Draft;
        public int CurrentRound { get; set; } = 1;
        public int TotalRounds { get; set; } = 1;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        // Navigation properties
        public virtual ICollection<Registration> Registrations { get; set; } = new List<Registration>();
        public virtual ICollection<Team> Teams { get; set; } = new List<Team>();
        public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
        public virtual ICollection<EventJudge> EventJudges { get; set; } = new List<EventJudge>();
    }
    
    public enum EventType
    {
        DebuggingCompetition = 1,
        CodeInTheDark = 2,
        UIUXDesignChallenge = 3
    }
    
    public enum EventStatus
    {
        Draft = 0,
        Published = 1,
        RegistrationOpen = 2,
        RegistrationClosed = 3,
        Ongoing = 4,
        Completed = 5,
        Cancelled = 6
    }
}
