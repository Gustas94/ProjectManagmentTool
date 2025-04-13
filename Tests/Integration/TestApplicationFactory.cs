using System;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectManagmentTool;
using ProjectManagmentTool.Data;

namespace Tests.Integration
{
    public class TestApplicationFactory : WebApplicationFactory<Program>
    {
        // Generate a unique database name once for the lifetime of the factory instance.
        private readonly string _dbName = "TestDb_" + Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove any existing ApplicationDbContext registrations.
                var dbContextDescriptors = services.Where(
                    d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>)).ToList();
                foreach (var descriptor in dbContextDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Use a unique in-memory database using the generated name.
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase(_dbName);
                });

                // Remove any previously registered authentication options.
                var authDescriptors = services.Where(
                    d => d.ServiceType == typeof(IConfigureOptions<AuthenticationOptions>)).ToList();
                foreach (var descriptor in authDescriptors)
                {
                    services.Remove(descriptor);
                }

                // Add the test authentication scheme along with a cookie scheme for Identity.
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                    options.DefaultSignInScheme = "Test";  // Set the default sign-in scheme.
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { })
                .AddCookie("Identity.Application", options => { });

                // Set the default authorization policy to require the test scheme.
                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder("Test")
                        .RequireAuthenticatedUser()
                        .Build();
                });

                // Build the service provider and seed the database.
                var sp = services.BuildServiceProvider();
                using (var scope = sp.CreateScope())
                {
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    // Ensure a fresh database by deleting and re-creating it.
                    db.Database.EnsureDeleted();
                    db.Database.EnsureCreated();

                    // Clear out Permissions (if any) to avoid duplicate key issues.
                    if (db.Permissions.Any())
                    {
                        db.Permissions.RemoveRange(db.Permissions);
                        db.SaveChanges();
                    }

                    // Seed required permissions.
                    db.Permissions.Add(new Permission
                    {
                        PermissionID = 3,
                        PermissionName = "ADMIN_PANEL_ACCESS",
                        Description = "Admin Panel Access",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    db.Permissions.Add(new Permission
                    {
                        PermissionID = 4,
                        PermissionName = "VIEW_TASK_DETAILS",
                        Description = "View Task Details",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    db.SaveChanges();
                }
            });
        }
    }

    // Test authentication handler that retrieves the ADMIN_PANEL_ACCESS permission from the database.
    public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public TestAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)  // ISystemClock is marked obsolete; you can ignore the warning for now.
            : base(options, logger, encoder, clock)
        { }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            // Retrieve the ApplicationDbContext from the current request services.
            var dbContext = Context.RequestServices.GetService(typeof(ApplicationDbContext)) as ApplicationDbContext;
            string permissionClaimValue = "ADMIN_PANEL_ACCESS"; // default value

            if (dbContext != null)
            {
                var permission = await dbContext.Permissions
                    .FirstOrDefaultAsync(p => p.PermissionName == "ADMIN_PANEL_ACCESS");
                if (permission != null)
                {
                    permissionClaimValue = permission.PermissionName;
                }
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "TestUser"),
                new Claim(ClaimTypes.Name, "Test User"),
                new Claim("Permission", permissionClaimValue),
                new Claim(ClaimTypes.Role, "Administrator")
            };

            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");
            return AuthenticateResult.Success(ticket);
        }
    }
}
