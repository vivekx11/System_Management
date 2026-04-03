namespace CollegeEventPortal.Models
{
    public class ActivityLog
    {
        public int Id { get; set; }
        
        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }
        
        public string Action { get; set; } = string.Empty;
        public string Entity { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string IpAddress { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
