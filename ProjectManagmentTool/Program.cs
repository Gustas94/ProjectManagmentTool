using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManagmentTool.Data;
using ProjectManagmentTool.Repositories;
using MediatR;
using System.Reflection;
using System.Text;
using ProjectManagmentTool.Observers;
using Microsoft.AspNetCore.Authorization;
using ProjectManagmentTool.Security;

namespace ProjectManagmentTool
{
    public partial class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Load configuration
            var jwtSettings = builder.Configuration.GetSection("Jwt");

            if (!builder.Environment.IsEnvironment("Testing"))
            {
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            }

            // Identity Configuration
            builder.Services.AddIdentity<User, Role>() // Use your custom Role class
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // JWT Authentication
            var key = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);
            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false; // For development only—set to true in production
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = jwtSettings["Issuer"],
                    ValidAudience = jwtSettings["Audience"],
                    ClockSkew = TimeSpan.Zero
                };
            });

            // Enable Authorization and define policies
            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPanelAccess", policy =>
                    policy.Requirements.Add(new PermissionRequirement("ADMIN_PANEL_ACCESS")));
            });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminPanelAccess", policy =>
                {
                    policy.Requirements.Add(new AdminPanelAccessRequirement());
                });
            });

            // Register the handler
            builder.Services.AddSingleton<IAuthorizationHandler, AdminPanelAccessHandler>();

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Register CORS Policy (adjust the origin as needed)
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowReactApp", policy =>
                {
                    policy.WithOrigins("http://localhost:5173")
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            // Register Repository (Dependency Injection)
            builder.Services.AddScoped<IGroupRepository, GroupRepository>();

            // Register MediatR (Automatically discovers all handlers)
            builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

            // Register ProjectRepository
            builder.Services.AddScoped<IProjectRepository, ProjectRepository>();

            // Use Scrutor to apply the Decorator Pattern
            builder.Services.Decorate<IProjectRepository, LoggingProjectRepositoryDecorator>();

            // Register Observer & Subject
            var taskSubject = new TaskSubject();
            taskSubject.Attach(new ConsoleTaskObserver()); // Attach observer
            builder.Services.AddSingleton(taskSubject); // Register as a Singleton

            var app = builder.Build();

            // Role Seeding: This method ensures that essential roles exist at startup.
            // It is not part of the admin panel, but runs automatically during application startup.
            await SeedRolesAsync(app);

            // Apply CORS BEFORE authentication & authorization middleware
            app.UseCors("AllowReactApp");

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.MapControllers();
            app.Run();
        }

        private static async Task SeedRolesAsync(IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var roleManager = services.GetRequiredService<RoleManager<Role>>();

                // Define the roles you want to initialize
                string[] roleNames = { "CEO", "Administrator", "ProjectManager", "Developer" };

                foreach (var roleName in roleNames)
                {
                    if (!await roleManager.RoleExistsAsync(roleName))
                    {
                        var role = new Role
                        {
                            Id = Guid.NewGuid().ToString(),
                            Name = roleName,
                            NormalizedName = roleName.ToUpper(),
                            // For example, you might treat "Administrator" as a global role.
                            IsCompanyRole = roleName != "Administrator",
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };

                        var result = await roleManager.CreateAsync(role);
                        if (!result.Succeeded)
                        {
                            Console.WriteLine($"Error creating role {roleName}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                        }
                        else
                        {
                            Console.WriteLine($"{roleName} role created successfully.");
                        }
                    }
                    else
                    {
                        Console.WriteLine($"{roleName} role already exists. Skipping creation.");
                    }
                }
            }
        }
    }
}
