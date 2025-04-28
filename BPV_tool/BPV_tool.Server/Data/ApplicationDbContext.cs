using BPV_app.Models;
using BPV_tool.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace BPV_app.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<AppUser> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Models.File> Files { get; set; }
        public DbSet<BPVProcess> BPVProcesses { get; set; }
        public DbSet<BPVStep> BPVSteps { get; set; }
        public DbSet<BPVApproval> BPVApprovals { get; set; }
        public DbSet<Log> Logs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Restrict relational deleting
            //  ————— User ↔ Role —————
            modelBuilder.Entity<AppUser>()
               .HasOne(u => u.Role)
               .WithMany(r => r.Users)
               .HasForeignKey(u => u.RoleId)
               .OnDelete(DeleteBehavior.Restrict);

            // ————— File ↔ Student —————
            modelBuilder.Entity<Models.File>()
                .HasOne(f => f.Student)
                .WithMany(u => u.UploadedFiles)
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            // ————— BPVProcess ↔ Student & Supervisor —————
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

            // ————— Log ↔ Student & BPVProcess —————
            modelBuilder.Entity<Log>()
                .HasOne(l => l.Student)
                .WithMany(u => u.Logs)
                .HasForeignKey(l => l.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Log>()
                .HasOne(l => l.BPVProcess)
                .WithMany(p => p.Logs)
                .HasForeignKey(l => l.BPVProcessId)
                .OnDelete(DeleteBehavior.Cascade);

            // ————— BPVStep ↔ BPVProcess (1–many) —————
            modelBuilder.Entity<BPVStep>()
               .HasOne(s => s.BPVProcess)
               .WithMany(p => p.Steps)
               .HasForeignKey(s => s.BPVProcessId)
               .OnDelete(DeleteBehavior.Cascade);

            // ————— BPVStep ↔ File (many–1, optional) —————
            modelBuilder.Entity<BPVStep>()
                .HasOne(s => s.File)
                .WithMany(f => f.ProcessSteps)
                .HasForeignKey(s => s.FileId)
                .OnDelete(DeleteBehavior.SetNull);

            // ————— BPVStep ↔ BPVApproval (1–1, optional) —————
            modelBuilder.Entity<BPVStep>()
                .HasOne(s => s.Approval)
                .WithOne(a => a.BPVStep)
                .HasForeignKey<BPVApproval>(a => a.BPVStepId)
                .OnDelete(DeleteBehavior.SetNull);

            // ————— BPVApproval ↔ Reviewer —————
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
