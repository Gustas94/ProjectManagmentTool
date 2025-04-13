using System;
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

public class InvitationControllerFullCoverageTests : IClassFixture<TestApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestApplicationFactory _factory;

    public InvitationControllerFullCoverageTests(TestApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    private async Task<T> DeserializeJsonResponse<T>(HttpResponseMessage response)
    {
        return await JsonSerializer.DeserializeAsync<T>(
            await response.Content.ReadAsStreamAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    // Clear Users and Invitations.
    private void ClearUsersAndInvitations()
    {
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Users.RemoveRange(db.Users);
            db.Invitations.RemoveRange(db.Invitations);
            db.SaveChanges();
        }
    }

    [Fact]
    public async Task CreateInvitation_UserWithoutCompany_ReturnsUnauthorized()
    {
        ClearUsersAndInvitations();
        // Seed a TestUser with NO CompanyID.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Users.Add(new User
            {
                Id = "TestUser",
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RoleID = "dummy",
                CompanyID = null
            });
            db.SaveChanges();
        }
        var invitationRequest = new { Email = "invitee@example.com", RoleID = "some-role-id" };
        var content = new StringContent(JsonSerializer.Serialize(invitationRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/invitations/create", content);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        Assert.Contains("You must be part of a company", jsonResponse);
    }

    [Fact]
    public async Task CreateInvitation_UserWithCompany_Success()
    {
        ClearUsersAndInvitations();
        // Seed a TestUser with a valid CompanyID.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var company = new Company
            {
                CompanyName = "Inviting Company",
                CEOID = "TestUser",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IndustryId = 1
            };
            db.Companies.Add(company);
            db.SaveChanges();
            db.Users.Add(new User
            {
                Id = "TestUser",
                UserName = "test@example.com",
                Email = "test@example.com",
                FirstName = "Test",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RoleID = "dummy",
                CompanyID = company.CompanyID
            });
            db.SaveChanges();
        }
        var invitationRequest = new { Email = "invitee@example.com", RoleID = "role-for-invite" };
        var content = new StringContent(JsonSerializer.Serialize(invitationRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/invitations/create", content);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        // Check for "inviteCode" (note the lower-case 'i') as it's serialized that way.
        Assert.Contains("inviteCode", jsonResponse);
    }

    [Fact]
    public async Task AcceptInvitation_InvalidOrExpired_ReturnsBadRequest()
    {
        ClearUsersAndInvitations();
        // Seed an expired invitation.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var company = new Company
            {
                CompanyName = "Expired Co",
                CEOID = "someone",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IndustryId = 1
            };
            db.Companies.Add(company);
            db.SaveChanges();
            db.Invitations.Add(new Invitation
            {
                InviteCode = "EXPIRED123",
                CompanyID = company.CompanyID,
                Email = "expired@example.com",
                RoleID = "role-id",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ExpiresAt = DateTime.UtcNow.AddDays(-5)
            });
            db.SaveChanges();
        }
        var acceptRequest = new { InviteCode = "EXPIRED123", Email = "newuser@example.com", FirstName = "New", LastName = "User", Password = "Password123!" };
        var content = new StringContent(JsonSerializer.Serialize(acceptRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/invitations/accept", content);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid or expired invitation", jsonResponse);
    }

    [Fact]
    public async Task AcceptInvitation_EmailAlreadyRegistered_ReturnsBadRequest()
    {
        ClearUsersAndInvitations();
        // Seed a valid invitation and use UserManager to create a user with the same email.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var company = new Company
            {
                CompanyName = "Existing Co",
                CEOID = "someone",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IndustryId = 1
            };
            db.Companies.Add(company);
            db.SaveChanges();
            db.Invitations.Add(new Invitation
            {
                InviteCode = "VALIDINVITE",
                CompanyID = company.CompanyID,
                Email = "existing@example.com",
                RoleID = "role-id",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            await db.SaveChangesAsync();

            // Create a user using UserManager so that email normalization takes place.
            var user = new User
            {
                UserName = "existing@example.com",
                Email = "existing@example.com",
                FirstName = "Existing",
                LastName = "User",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                RoleID = "role-id"
            };
            await userManager.CreateAsync(user, "Password123!");
        }
        var acceptRequest = new { InviteCode = "VALIDINVITE", Email = "existing@example.com", FirstName = "New", LastName = "User", Password = "Password123!" };
        var content = new StringContent(JsonSerializer.Serialize(acceptRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/invitations/accept", content);
        // Expecting BadRequest because the email is already registered.
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task AcceptInvitation_Success_ReturnsOk()
    {
        ClearUsersAndInvitations();
        // Seed a valid invitation.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var company = new Company
            {
                CompanyName = "Acceptance Co",
                CEOID = "someone",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IndustryId = 1
            };
            db.Companies.Add(company);
            db.SaveChanges();
            db.Invitations.Add(new Invitation
            {
                InviteCode = "ACCEPT123",
                CompanyID = company.CompanyID,
                Email = "accept@example.com",
                RoleID = "role-id",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            db.SaveChanges();
        }
        var acceptRequest = new { InviteCode = "ACCEPT123", Email = "accept@example.com", FirstName = "Accept", LastName = "User", Password = "Password123!" };
        var content = new StringContent(JsonSerializer.Serialize(acceptRequest), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/invitations/accept", content);
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        Assert.Contains("Registration successful", jsonResponse);

        // Verify that the invitation has been removed.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var invite = await db.Invitations.FirstOrDefaultAsync(i => i.InviteCode == "ACCEPT123");
            Assert.Null(invite);
        }
    }

    [Fact]
    public async Task ValidateInvitation_Invalid_ReturnsBadRequest()
    {
        ClearUsersAndInvitations();
        var response = await _client.GetAsync("/api/invitations/validate?inviteCode=NONEXISTENT");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var jsonResponse = await response.Content.ReadAsStringAsync();
        Assert.Contains("Invalid or expired invitation", jsonResponse);
    }

    [Fact]
    public async Task ValidateInvitation_Success_ReturnsOk()
    {
        ClearUsersAndInvitations();
        int companyId;
        // Seed a valid invitation.
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var company = new Company
            {
                CompanyName = "Validation Co",
                CEOID = "someone",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                IndustryId = 1
            };
            db.Companies.Add(company);
            db.SaveChanges();
            companyId = company.CompanyID;
            db.Invitations.Add(new Invitation
            {
                InviteCode = "VALIDATE123",
                CompanyID = company.CompanyID,
                Email = "validate@example.com",
                RoleID = "role-id",
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            });
            db.SaveChanges();
        }
        var response = await _client.GetAsync("/api/invitations/validate?inviteCode=VALIDATE123");
        response.EnsureSuccessStatusCode();
        var jsonResponse = await response.Content.ReadAsStringAsync();
        Assert.Contains(companyId.ToString(), jsonResponse);
    }
}
