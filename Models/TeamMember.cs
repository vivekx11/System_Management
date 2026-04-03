namespace CollegeEventPortal.Models
{
    public class TeamMember
    {
        public int Id { get; set; }
        
        public int TeamId { get; set; }
        public virtual Team Team { get; set; } = null!;
        
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;
        
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
        public bool IsLeader { get; set; } = false;
    }
}
