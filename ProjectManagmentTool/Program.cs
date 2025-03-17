using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManagmentTool.Data;
using ProjectManagmentTool.Repositories;
using MediatR;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Load configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");

// Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration
builder.Services.AddIdentity<User, Role>() // Use your Role class
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
    options.RequireHttpsMetadata = false; // No HTTPS
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

// Enable Authorization
builder.Services.AddAuthorization();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register CORS Policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")  // Allow frontend
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();  // Required for authentication
    });
});

// Register Repository (Dependency Injection)
builder.Services.AddScoped<IGroupRepository, GroupRepository>();

// Register MediatR (Automatically finds all handlers)
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

var app = builder.Build();

// Ensure CEO Role Exists on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var roleManager = services.GetRequiredService<RoleManager<Role>>();

    string roleName = "CEO";
    var existingRole = await roleManager.FindByNameAsync(roleName);

    if (existingRole == null)
    {
        var role = new Role
        {
            Id = Guid.NewGuid().ToString(),  // Ensure unique ID
            Name = roleName,
            NormalizedName = roleName.ToUpper(),
            IsCompanyRole = true,
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
            Console.WriteLine($"CEO role created successfully.");
        }
    }
    else
    {
        Console.WriteLine($"CEO role already exists. Skipping creation.");
    }
}

// Apply CORS BEFORE authentication & authorization
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
