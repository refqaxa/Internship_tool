using BPV_app.Models;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.EntityFrameworkCore;

namespace BPV_app.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Models.File> Files { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<BPVProcess> BPVProcesses { get; set; }
        public DbSet<BPVApproval> BPVApprovals { get; set; }
        public DbSet<Log> Logs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relationship: Student -> BPVProcess
            modelBuilder.Entity<BPVProcess>()
                .HasOne(p => p.Student)
                .WithMany(u => u.StartedProcesses)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relationship: Supervisor -> BPVProcess
            modelBuilder.Entity<BPVProcess>()
                .HasOne(p => p.Supervisor)
                .WithMany(u => u.SupervisedProcesses)
                .HasForeignKey(p => p.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Restrict relational deleting
            modelBuilder.Entity<Feedback>()
                .HasOne(f => f.Reviewer)
                .WithMany(u => u.GivenFeedback)
                .HasForeignKey(f => f.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Models.File>()
                .HasOne(f => f.Student)
                .WithMany(u => u.UploadedFiles)
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BPVProcess>()
                .HasOne(p => p.Student)
                .WithMany(u => u.StartedProcesses)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BPVProcess>()
                .HasOne(p => p.Supervisor)
                .WithMany(u => u.SupervisedProcesses)
                .HasForeignKey(p => p.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<BPVApproval>()
                .HasOne(a => a.Reviewer)
                .WithMany(u => u.BPVApprovals)
                .HasForeignKey(a => a.ReviewerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed roles
            var adminRoleId = Guid.NewGuid();
            var teacherRoleId = Guid.NewGuid();
            var studentRoleId = Guid.NewGuid();

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = adminRoleId, RoleName = "Admin" },
                new Role { Id = teacherRoleId, RoleName = "Teacher" },
                new Role { Id = studentRoleId, RoleName = "Student" }
            );

            // Seed admin user
            var adminUserId = Guid.NewGuid();
            var passwordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123");

            modelBuilder.Entity<AppUser>().HasData(new AppUser
            {
                Id = adminUserId,
                FirstName = "Admin",
                MiddleName = null,
                LastName = "User",
                Email = "admin@bpv.local",
                PasswordHash = passwordHash,
                RoleId = adminRoleId
            });


        }
    }

}
