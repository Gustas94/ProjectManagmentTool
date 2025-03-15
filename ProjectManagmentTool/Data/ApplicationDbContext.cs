using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagmentTool.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Company> Companies { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<UserProject> UserProjects { get; set; }
        public DbSet<ProjectTask> Tasks { get; set; }
        public DbSet<UserTask> UserTasks { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<Invitation> Invitations { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<ProjectGroup> ProjectGroups { get; set; }
        public DbSet<GroupMember> GroupMembers { get; set; }
        public DbSet<TaskGroup> TaskGroups { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure RolePermission relationships
            modelBuilder.Entity<RolePermission>(entity =>
            {
                entity.HasKey(rp => new { rp.RoleID, rp.PermissionID });

                entity.HasOne(rp => rp.Role)
                    .WithMany(r => r.RolePermissions)
                    .HasForeignKey(rp => rp.RoleID)
                    .HasPrincipalKey(r => r.Id); // ✅ Use 'Id' instead of 'RoleID'

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
                    .OnDelete(DeleteBehavior.NoAction); // Prevents cascading delete
            });


            // Configure Group relationships
            modelBuilder.Entity<Group>(entity =>
            {
                entity.HasOne(g => g.GroupLead)
                    .WithMany()
                    .HasForeignKey(g => g.GroupLeadID)
                    .OnDelete(DeleteBehavior.NoAction); 
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

            modelBuilder.Entity<ProjectTask>(entity =>
            {
                entity.HasKey(t => t.TaskID);

                entity.HasOne(t => t.Project)
                      .WithMany()
                      .HasForeignKey(t => t.ProjectID)
                      .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(t => t.Group)
                      .WithMany(g => g.Tasks) // Navigation property in Group
                      .HasForeignKey(t => t.GroupID)
                      .OnDelete(DeleteBehavior.NoAction); // Disable cascade delete
            });


            modelBuilder.Entity<User>()
                    .HasOne(u => u.Role)
                    .WithMany()
                    .HasForeignKey(u => u.RoleID)  // Foreign key in User
                    .HasPrincipalKey(r => r.Id)    // Use 'Id' instead of 'RoleID'
                    .OnDelete(DeleteBehavior.Restrict);


            modelBuilder.Entity<UserProject>(entity =>
            {
                entity.HasKey(up => new { up.UserID, up.ProjectID });

                entity.HasOne(up => up.User)
                    .WithMany()
                    .HasForeignKey(up => up.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(up => up.Project)
                    .WithMany(p => p.UserProjects)
                    .HasForeignKey(up => up.ProjectID)
                    .OnDelete(DeleteBehavior.Cascade);
            });


            modelBuilder.Entity<UserTask>(entity =>
            {
                entity.HasKey(ut => new { ut.UserID, ut.TaskID });

                entity.HasOne(ut => ut.User)
                    .WithMany()
                    .HasForeignKey(ut => ut.UserID)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(ut => ut.Task)
                    .WithMany()
                    .HasForeignKey(ut => ut.TaskID)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ProjectUser>()
                .HasKey(pu => new { pu.UserID, pu.ProjectID }); // Composite Key

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.User)
                .WithMany()
                .HasForeignKey(pu => pu.UserID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectUser>()
                .HasOne(pu => pu.Project)
                .WithMany()
                .HasForeignKey(pu => pu.ProjectID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Invitation>()
                .HasOne(i => i.Company)
                .WithMany()
                .HasForeignKey(i => i.CompanyID)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ProjectGroup>(entity =>
            {
                entity.HasKey(pg => new { pg.ProjectID, pg.GroupID });

                entity.HasOne(pg => pg.Project)
                      .WithMany(p => p.ProjectGroups)
                      .HasForeignKey(pg => pg.ProjectID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(pg => pg.Group)
                      .WithMany(g => g.ProjectGroups)
                      .HasForeignKey(pg => pg.GroupID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.HasKey(gm => new { gm.GroupID, gm.UserID });

                entity.HasOne(gm => gm.Group)
                      .WithMany(g => g.GroupMembers)
                      .HasForeignKey(gm => gm.GroupID)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(gm => gm.User)
                      .WithMany()
                      .HasForeignKey(gm => gm.UserID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<TaskGroup>(entity =>
            {
                entity.HasKey(tg => new { tg.TaskID, tg.GroupID }); // Composite primary key

                entity.HasOne(tg => tg.Task)
                      .WithMany(t => t.TaskGroups) // Navigation property in ProjectTask
                      .HasForeignKey(tg => tg.TaskID)
                      .OnDelete(DeleteBehavior.NoAction); // Disable cascade delete

                entity.HasOne(tg => tg.Group)
                      .WithMany(g => g.TaskGroups) // Navigation property in Group
                      .HasForeignKey(tg => tg.GroupID)
                      .OnDelete(DeleteBehavior.NoAction); // Disable cascade delete
            });
        }
    }
}
