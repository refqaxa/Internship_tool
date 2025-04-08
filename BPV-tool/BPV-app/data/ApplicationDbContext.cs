using BPV_app.Models;
using Microsoft.EntityFrameworkCore;

namespace BPV_app.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<File> Files { get; set; }
        public DbSet<Feedback> Feedback { get; set; }
        public DbSet<BPVProcess> BPVProcesses { get; set; }
        public DbSet<BPVApproval> BPVApprovals { get; set; }
        public DbSet<Log> Logs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure optional properties, relationships, constraints, etc.
            base.OnModelCreating(modelBuilder);
        }
    }

}
