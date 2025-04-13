using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectManagmentTool.Data;
using Tests.Integration;
using Xunit;

namespace ProjectManagmentTool.Tests.Integration.Controllers
{
    public class AuthControllerFullCoverageTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;

        public AuthControllerFullCoverageTests(TestApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        // Clear Users table to improve isolation.
        private void ClearUsers()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();
            }
        }

        [Fact]
        public async Task Register_MissingEmailOrPassword_ReturnsBadRequest()
        {
            // Missing email.
            var reqMissingEmail = new { Email = "", Password = "Password123!", FirstName = "Test", LastName = "User" };
            var content1 = new StringContent(JsonSerializer.Serialize(reqMissingEmail), Encoding.UTF8, "application/json");
            var response1 = await _client.PostAsync("/api/auth/register", content1);
            Assert.Equal(HttpStatusCode.BadRequest, response1.StatusCode);
            var json1 = await response1.Content.ReadAsStringAsync();
            Assert.Contains("Email and password are required", json1);

            // Missing password.
            var reqMissingPass = new { Email = "test@example.com", Password = "", FirstName = "Test", LastName = "User" };
            var content2 = new StringContent(JsonSerializer.Serialize(reqMissingPass), Encoding.UTF8, "application/json");
            var response2 = await _client.PostAsync("/api/auth/register", content2);
            Assert.Equal(HttpStatusCode.BadRequest, response2.StatusCode);
            var json2 = await response2.Content.ReadAsStringAsync();
            Assert.Contains("Email and password are required", json2);
        }

        [Fact]
        public async Task Register_EmailAlreadyTaken_ReturnsBadRequest()
        {
            ClearUsers();

            // Seed a user with a specific email.
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var user = new User
                {
                    UserName = "taken@example.com",
                    Email = "taken@example.com",
                    FirstName = "Existing",
                    LastName = "User",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RoleID = "dummy"
                };
                await userManager.CreateAsync(user, "Password123!");
            }

            var registerRequest = new { Email = "taken@example.com", Password = "Password123!", FirstName = "New", LastName = "User" };
            var content = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/register", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("Email is already taken", json);
        }

        [Fact]
        public async Task Register_NormalRegistration_Success()
        {
            ClearUsers();

            // Ensure the CEO role exists.
            using (var scope = _factory.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                if (!await roleManager.Roles.AnyAsync(r => r.NormalizedName == "CEO"))
                {
                    var ceoRole = new Role
                    {
                        Id = "ceo-role-id", // Fixed value for testing purposes.
                        Name = "CEO",
                        NormalizedName = "CEO",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsCompanyRole = false
                    };
                    await roleManager.CreateAsync(ceoRole);
                }
            }

            var registerRequest = new { Email = "ceo@example.com", Password = "Password123!", FirstName = "Ceo", LastName = "User" };
            var content = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/register", content);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("User registered successfully and assigned as CEO", jsonResponse);

            // Verify the user was created with the CEO role.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "ceo@example.com");
                Assert.NotNull(user);
                // Instead of asserting a literal RoleID, verify role membership.
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                Assert.True(await userManager.IsInRoleAsync(user, "CEO"));
            }
        }

        [Fact]
        public async Task Register_WithInvite_Success()
        {
            ClearUsers();

            // Seed a role for invitation and an invitation record.
            string inviteRoleId = "invite-role-id";
            using (var scope = _factory.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                if (await roleManager.FindByIdAsync(inviteRoleId) == null)
                {
                    var inviteRole = new Role
                    {
                        Id = inviteRoleId,
                        Name = "User",
                        NormalizedName = "USER",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsCompanyRole = false
                    };
                    await roleManager.CreateAsync(inviteRole);
                }
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                // Seed a company.
                var company = new Company
                {
                    CompanyName = "Test Company",
                    CEOID = "ceo",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    IndustryId = 1
                };
                db.Companies.Add(company);
                await db.SaveChangesAsync();
                // Seed an invitation.
                var invitation = new Invitation
                {
                    InviteCode = "INVITE123",
                    CompanyID = company.CompanyID,
                    Email = "invited@example.com",
                    RoleID = inviteRoleId,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                };
                db.Invitations.Add(invitation);
                await db.SaveChangesAsync();
            }

            var registerRequest = new { Email = "invited@example.com", Password = "Password123!", FirstName = "Invited", LastName = "User", InviteCode = "INVITE123" };
            var contentReq = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
            var responseReq = await _client.PostAsync("/api/auth/register", contentReq);
            responseReq.EnsureSuccessStatusCode();
            var jsonResponse = await responseReq.Content.ReadAsStringAsync();
            Assert.Contains("User registered successfully via invite", jsonResponse);

            // Verify that the user is created and that the invitation has been removed.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "invited@example.com");
                Assert.NotNull(user);
                var invite = await db.Invitations.FirstOrDefaultAsync(i => i.InviteCode == "INVITE123");
                Assert.Null(invite); // Invitation should have been removed.
            }
        }

        [Fact]
        public async Task Register_WithInvalidInvite_ReturnsBadRequest()
        {
            ClearUsers();

            var registerRequest = new { Email = "invited@example.com", Password = "Password123!", FirstName = "Invited", LastName = "User", InviteCode = "NONEXISTENT" };
            var content = new StringContent(JsonSerializer.Serialize(registerRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/register", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid or expired invite link", jsonResponse);
        }

        [Fact]
        public async Task Login_UserNotFound_ReturnsUnauthorized()
        {
            ClearUsers();

            var loginRequest = new { Email = "nonexistent@example.com", Password = "Password123!" };
            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", content);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid credentials", jsonResponse);
        }

        [Fact]
        public async Task Login_InvalidPassword_ReturnsUnauthorized()
        {
            ClearUsers();

            // Seed a user with a known password.
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var user = new User
                {
                    UserName = "user@example.com",
                    Email = "user@example.com",
                    FirstName = "Test",
                    LastName = "User",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RoleID = "dummy"
                };
                await userManager.CreateAsync(user, "CorrectPassword123!");
            }

            var loginRequest = new { Email = "user@example.com", Password = "WrongPassword!" };
            var content = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/auth/login", content);
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("Invalid credentials", jsonResponse);
        }

        [Fact]
        public async Task Login_Success_ReturnsToken()
        {
            ClearUsers();

            // Seed a user with a known password using UserManager.
            using (var scope = _factory.Services.CreateScope())
            {
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                // Ensure a dummy role exists.
                if (!await roleManager.Roles.AnyAsync())
                {
                    var role = new Role
                    {
                        Id = "dummy-role",
                        Name = "Dummy",
                        NormalizedName = "DUMMY",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsCompanyRole = false
                    };
                    await roleManager.CreateAsync(role);
                }
                var user = new User
                {
                    UserName = "login@example.com",
                    Email = "login@example.com",
                    FirstName = "Login",
                    LastName = "User",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    RoleID = "dummy-role"
                };
                await userManager.CreateAsync(user, "CorrectPassword123!");
            }

            var loginRequest = new { Email = "login@example.com", Password = "CorrectPassword123!" };
            var contentReq = new StringContent(JsonSerializer.Serialize(loginRequest), Encoding.UTF8, "application/json");
            var responseReq = await _client.PostAsync("/api/auth/login", contentReq);
            responseReq.EnsureSuccessStatusCode();
            var jsonResponse = await responseReq.Content.ReadAsStringAsync();
            Assert.Contains("token", jsonResponse);
        }
    }
}