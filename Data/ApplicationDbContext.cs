using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CollegeEventPortal.Models;

namespace CollegeEventPortal.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Event> Events { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<TeamMember> TeamMembers { get; set; }
        public DbSet<Registration> Registrations { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<Submission> Submissions { get; set; }
        public DbSet<Certificate> Certificates { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ActivityLog> ActivityLogs { get; set; }
        public DbSet<EventJudge> EventJudges { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Event configuration
            builder.Entity<Event>()
                .HasIndex(e => e.Name);

            // Team configuration
            builder.Entity<Team>()
                .HasIndex(t => t.InviteCode)
                .IsUnique();

            builder.Entity<Team>()
                .HasOne(t => t.Leader)
                .WithMany()
                .HasForeignKey(t => t.LeaderId)
                .OnDelete(DeleteBehavior.Restrict);

            // TeamMember configuration
            builder.Entity<TeamMember>()
                .HasOne(tm => tm.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(tm => tm.TeamId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<TeamMember>()
                .HasOne(tm => tm.User)
                .WithMany(u => u.TeamMemberships)
                .HasForeignKey(tm => tm.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Registration configuration
            builder.Entity<Registration>()
                .HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Score configuration
            builder.Entity<Score>()
                .HasOne(s => s.Judge)
                .WithMany()
                .HasForeignKey(s => s.JudgeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Score>()
                .HasOne(s => s.Participant)
                .WithMany()
                .HasForeignKey(s => s.ParticipantId)
                .OnDelete(DeleteBehavior.Restrict);

            // Certificate configuration
            builder.Entity<Certificate>()
                .HasIndex(c => c.CertificateNumber)
                .IsUnique();

            builder.Entity<Certificate>()
                .HasOne(c => c.User)
                .WithMany(u => u.Certificates)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification configuration
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // EventJudge configuration
            builder.Entity<EventJudge>()
                .HasOne(ej => ej.Judge)
                .WithMany()
                .HasForeignKey(ej => ej.JudgeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
