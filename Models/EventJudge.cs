// event judge 

namespace CollegeEventPortal.Models
{
    public class EventJudge
    {
        public int Id { get; set; }
        
        public int EventId { get; set; }
        public virtual Event Event { get; set; } = null!;
        
        public string JudgeId { get; set; } = string.Empty;
        public virtual ApplicationUser Judge { get; set; } = null!;
        
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    }
}
