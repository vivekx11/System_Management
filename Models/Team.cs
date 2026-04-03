using System.ComponentModel.DataAnnotations;

namespace CollegeEventPortal.Models
{
    public class Team
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string InviteCode { get; set; } = string.Empty;
        
        public int EventId { get; set; }
        public virtual Event Event { get; set; } = null!;
        
        public string LeaderId { get; set; } = string.Empty;
        public virtual ApplicationUser Leader { get; set; } = null!;
        
        public TeamStatus Status { get; set; } = TeamStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public virtual ICollection<TeamMember> Members { get; set; } = new List<TeamMember>();
        public virtual ICollection<Score> Scores { get; set; } = new List<Score>();
        public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();
    }
    
    public enum TeamStatus
    {
        Active = 1,
        Disqualified = 2,
        Withdrawn = 3
    }
}
