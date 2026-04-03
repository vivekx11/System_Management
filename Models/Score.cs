namespace CollegeEventPortal.Models
{
    public class Score
    {
        public int Id { get; set; }
        
        public int EventId { get; set; }
        public virtual Event Event { get; set; } = null!;
        
        public string? ParticipantId { get; set; }
        public virtual ApplicationUser? Participant { get; set; }
        
        public int? TeamId { get; set; }
        public virtual Team? Team { get; set; }
        
        public int Round { get; set; }
        public decimal Points { get; set; }
        public string? Remarks { get; set; }
        
        public string JudgeId { get; set; } = string.Empty;
        public virtual ApplicationUser Judge { get; set; } = null!;
        
        public DateTime ScoredAt { get; set; } = DateTime.UtcNow;
    }
}
