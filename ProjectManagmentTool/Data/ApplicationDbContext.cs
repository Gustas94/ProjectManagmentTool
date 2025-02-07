using Microsoft.EntityFrameworkCore;

namespace ProjectManagmentTool.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Define DbSets for all entities
        public DbSet<User> Users { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<Task> Tasks { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure RolePermission relationships
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleID, rp.PermissionID });

                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleID);

                entity.HasOne(rp => rp.Permission)
                    .WithMany(p => p.RolePermissions)
                    .HasForeignKey(rp => rp.PermissionID);
            });

            // Configure Company-CEO relationship
            modelBuilder.Entity<Company>()
                .HasOne(c => c.CEO)
                .WithOne(u => u.Company)
                .HasForeignKey<User>(u => u.CompanyID)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure UserRole composite key
            modelBuilder.Entity<UserRole>()
                .HasKey(ur => new { ur.UserID, ur.RoleID, ur.ProjectID });

            // Configure Role-Company relationship
            modelBuilder.Entity<Role>()
                .HasOne(r => r.Company)
                .WithMany()
                .HasForeignKey(r => r.CompanyID)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Invitation relationships
            modelBuilder.Entity<Invitation>(entity =>
            {
                entity.HasOne(i => i.Company)
                    .WithMany()
                    .HasForeignKey(i => i.CompanyID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(i => i.Role)
                    .WithMany()
                    .HasForeignKey(i => i.RoleID)
                    .OnDelete(DeleteBehavior.NoAction);  // Prevent cascade from Role
            });

            // Configure Project relationships
            modelBuilder.Entity<Project>(entity =>
            {
                entity.HasOne(p => p.Company)
                    .WithMany()
                    .HasForeignKey(p => p.CompanyID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.ProjectManager)
                    .WithMany()
                    .HasForeignKey(p => p.ProjectManagerID)
                    .OnDelete(DeleteBehavior.NoAction);  // Changed to prevent multiple cascade paths
            });

            // Configure Group relationships
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasOne(g => g.Project)
                    .WithMany()
                    .HasForeignKey(g => g.ProjectID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(g => g.GroupLead)
                    .WithMany()
                    .HasForeignKey(g => g.GroupLeadID)
                    .OnDelete(DeleteBehavior.NoAction);  // Prevent multiple cascade paths
            });

            // Configure UserRole relationships
            modelBuilder.Entity<UserRole>(entity =>
            {
                // Configure composite key
                entity.HasKey(ur => new { ur.UserID, ur.RoleID, ur.ProjectID });

                // Configure relationships
                entity.HasOne(ur => ur.User)
                    .WithMany()
                    .HasForeignKey(ur => ur.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ur => ur.Role)
                    .WithMany()
                    .HasForeignKey(ur => ur.RoleID)
                    .OnDelete(DeleteBehavior.NoAction);  // Prevent cascade from Role

                entity.HasOne(ur => ur.Project)
                    .WithMany()
                    .HasForeignKey(ur => ur.ProjectID)
                    .OnDelete(DeleteBehavior.NoAction);  // Prevent cascade from Project
            });

            // Configure Discussions relationships
            modelBuilder.Entity<Discussion>()
                .HasOne(d => d.Project)
                .WithMany()
                .HasForeignKey(d => d.ProjectID)
                .OnDelete(DeleteBehavior.NoAction);  // Prevent cascading delete for Project

            modelBuilder.Entity<Discussion>()
                .HasOne(d => d.Group)
                .WithMany()
                .HasForeignKey(d => d.GroupID)
                .OnDelete(DeleteBehavior.NoAction);  // Prevent cascading delete for Group

            modelBuilder.Entity<Discussion>()
                .HasOne(d => d.User)
                .WithMany()
                .HasForeignKey(d => d.UserID)
                .OnDelete(DeleteBehavior.NoAction);  // Prevent cascading delete for User

            modelBuilder.Entity<Task>(entity =>
            {
                entity.HasOne(t => t.Project)
                    .WithMany()
                    .HasForeignKey(t => t.ProjectID)
                    .OnDelete(DeleteBehavior.NoAction);  // Prevent cascading delete for Project

                entity.HasOne(t => t.Group)
                    .WithMany()
                    .HasForeignKey(t => t.GroupID)
                    .OnDelete(DeleteBehavior.Cascade);  // Cascade delete for Group (no issue here)

                entity.HasOne(t => t.AssignedUser)
                    .WithMany()
                    .HasForeignKey(t => t.AssignedTo)  // Use AssignedTo as the foreign key
                    .OnDelete(DeleteBehavior.NoAction);  // Prevent cascading delete for AssignedUser
            });
        }
    }
}
