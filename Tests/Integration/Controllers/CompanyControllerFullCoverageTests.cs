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
using ProjectManagmentTool.Controllers;
using ProjectManagmentTool.Data;
using Tests.Integration;
using Xunit;

namespace ProjectManagmentTool.Tests.Integration.Controllers
{
    public class CompanyControllerFullCoverageTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;

        public CompanyControllerFullCoverageTests(TestApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        // Helper method for deserializing responses.
        private async Task<T> DeserializeJsonResponse<T>(HttpResponseMessage response)
        {
            return await JsonSerializer.DeserializeAsync<T>(
                await response.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        // Clears Companies and Users for test isolation.
        private void ClearCompaniesAndUsers()
        {
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                db.Companies.RemoveRange(db.Companies);
                db.Users.RemoveRange(db.Users);
                db.SaveChanges();
            }
        }

        [Fact]
        public async Task CreateCompany_MissingRequiredFields_ReturnsBadRequest()
        {
            ClearCompaniesAndUsers();
            // Missing CompanyName and invalid IndustryId (0)
            var invalidRequest = new CreateCompanyDTO
            {
                CompanyName = "", // Missing company name
                IndustryId = 0,   // Invalid
                Email = "ceo@example.com",
                FirstName = "CEO",
                LastName = "User",
                Password = "Password123!"
            };

            var content = new StringContent(JsonSerializer.Serialize(invalidRequest), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/company/create", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("All required fields must be filled", json);
        }

        [Fact]
        public async Task CreateCompany_CompanyAlreadyExists_ReturnsBadRequest()
        {
            ClearCompaniesAndUsers();
            // First, create a valid company.
            var request = new CreateCompanyDTO
            {
                CompanyName = "Test Company",
                IndustryId = 1,
                Email = "ceo@example.com",
                FirstName = "CEO",
                LastName = "User",
                Password = "Password123!"
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/company/create", content);
            response.EnsureSuccessStatusCode();

            // Try creating another company with the same name.
            var duplicateRequest = new CreateCompanyDTO
            {
                CompanyName = "Test Company", // same name as before
                IndustryId = 1,
                Email = "ceo2@example.com",
                FirstName = "CEO2",
                LastName = "User",
                Password = "Password123!"
            };

            content = new StringContent(JsonSerializer.Serialize(duplicateRequest), Encoding.UTF8, "application/json");
            response = await _client.PostAsync("/api/company/create", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var json = await response.Content.ReadAsStringAsync();
            Assert.Contains("A company with this name already exists", json);
        }

        [Fact]
        public async Task CreateCompany_CEORoleMissing_ReturnsBadRequest()
        {
            ClearCompaniesAndUsers();
            // Remove CEO role from the database if it exists.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var ceoRole = await db.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "CEO");
                if (ceoRole != null)
                {
                    db.Roles.Remove(ceoRole);
                    await db.SaveChangesAsync();
                }
            }

            var request = new CreateCompanyDTO
            {
                CompanyName = "Another Company",
                IndustryId = 1,
                Email = "ceo@example.com",
                FirstName = "CEO",
                LastName = "User",
                Password = "Password123!"
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/company/create", content);
            // Expect BadRequest because CEO role is missing.
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("CEO role is missing", jsonResponse);
        }

        [Fact]
        public async Task CreateCompany_FailingUserCreation_ReturnsBadRequest()
        {
            ClearCompaniesAndUsers();
            // Provide a password that does not meet requirements (simulate failure)
            var request = new CreateCompanyDTO
            {
                CompanyName = "Fail Company",
                IndustryId = 1,
                Email = "failceo@example.com",
                FirstName = "Fail",
                LastName = "User",
                Password = "pass" // Intentionally weak
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/company/create", content);
            // Expect a 400 BadRequest since user creation should fail.
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("errors", jsonResponse);
        }

        [Fact]
        public async Task CreateCompany_Success_ReturnsOk()
        {
            ClearCompaniesAndUsers();
            // Ensure that CEO role exists.
            using (var scope = _factory.Services.CreateScope())
            {
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();
                if (!await roleManager.Roles.AnyAsync(r => r.NormalizedName == "CEO"))
                {
                    var ceoRole = new Role
                    {
                        Id = "ceo-role-id", // Use fixed value for testing
                        Name = "CEO",
                        NormalizedName = "CEO",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsCompanyRole = false
                    };
                    await roleManager.CreateAsync(ceoRole);
                }
            }

            var request = new CreateCompanyDTO
            {
                CompanyName = "New Company",
                IndustryId = 1,
                Email = "newceo@example.com",
                FirstName = "New",
                LastName = "CEO",
                Password = "Password123!"
            };

            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var response = await _client.PostAsync("/api/company/create", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await DeserializeJsonResponse<JsonElement>(response);
            Assert.True(jsonResponse.TryGetProperty("companyID", out _), "Response should contain companyID");
            Assert.True(jsonResponse.TryGetProperty("token", out _), "Response should contain token");
            Assert.Contains("Company created successfully", jsonResponse.GetProperty("message").GetString());

            // Verify that the company is created and the CEO has been updated with the CompanyID.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var company = await db.Companies.FirstOrDefaultAsync(c => c.CompanyName == "New Company");
                Assert.NotNull(company);

                var ceo = await db.Users.FirstOrDefaultAsync(u => u.Email == "newceo@example.com");
                Assert.NotNull(ceo);
                Assert.Equal(company.CompanyID, ceo.CompanyID);
            }
        }
    }
}
