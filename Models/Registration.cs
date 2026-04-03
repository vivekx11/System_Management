namespace CollegeEventPortal.Models
{
    public class Registration
    {
        public int Id { get; set; }
        
        public int EventId { get; set; }
        public virtual Event Event { get; set; } = null!;
        
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;
        
        public int? TeamId { get; set; }
        public virtual Team? Team { get; set; }
        
        public RegistrationStatus Status { get; set; } = RegistrationStatus.Pending;
        public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedBy { get; set; }
    }
    
    public enum RegistrationStatus
    {
        Pending = 0,
        Approved = 1,
        Rejected = 2,
        Cancelled = 3
    }
}
