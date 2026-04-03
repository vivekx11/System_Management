namespace CollegeEventPortal.Models
{
    public class Certificate
    {
        public int Id { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser User { get; set; } = null!;
        
        public int EventId { get; set; }
        public virtual Event Event { get; set; } = null!;
        
        public string CertificateNumber { get; set; } = string.Empty;
        public int Rank { get; set; }
        public string? FilePath { get; set; }
        public string QRCode { get; set; } = string.Empty;
        
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}
