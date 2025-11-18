using HoursTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HoursTracker.Data
{
    /// <summary>
    /// DbContext cho ứng dụng Hours Tracker
    /// </summary>
    public class HoursTrackerDbContext : DbContext
    {
        public HoursTrackerDbContext(DbContextOptions<HoursTrackerDbContext> options)
            : base(options)
        {
        }

        public DbSet<Skill> Skills { get; set; }
        public DbSet<PracticeLog> PracticeLogs { get; set; }
        public DbSet<Milestone> Milestones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình Skill
            modelBuilder.Entity<Skill>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Color).IsRequired().HasMaxLength(20);
                entity.Property(e => e.TargetHours).HasDefaultValue(1000);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
            });

            // Cấu hình PracticeLog
            modelBuilder.Entity<PracticeLog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Notes).HasMaxLength(1000);
                entity.Property(e => e.CreatedDate).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Skill)
                    .WithMany(s => s.PracticeLogs)
                    .HasForeignKey(e => e.SkillId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình Milestone
            modelBuilder.Entity<Milestone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AchievedDate).HasDefaultValueSql("GETDATE()");
                entity.HasOne(e => e.Skill)
                    .WithMany()
                    .HasForeignKey(e => e.SkillId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}

