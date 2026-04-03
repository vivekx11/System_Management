namespace CollegeEventPortal.Models
{
    public class Submission
    {
        public int Id { get; set; }
        
        public int EventId { get; set; }
        public virtual Event Event { get; set; } = null!;
        
        public string? ParticipantId { get; set; }
        public virtual ApplicationUser? Participant { get; set; }
        
        public int? TeamId { get; set; }
        public virtual Team? Team { get; set; }
        
        public int Round { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public long FileSize { get; set; }
        
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
    }
}
