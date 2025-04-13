using System.Net.Http;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;
using Tests.Integration;
using ProjectManagmentTool.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace ProjectManagmentTool.Tests.Integration.Controllers
{
    public class AdminControllerFullCoverageTests : IClassFixture<TestApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly TestApplicationFactory _factory;

        public AdminControllerFullCoverageTests(TestApplicationFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        // Helper method to deserialize JSON responses if needed.
        private async Task<T> DeserializeResponseAsync<T>(HttpResponseMessage response)
        {
            return await JsonSerializer.DeserializeAsync<T>(
                await response.Content.ReadAsStreamAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        /// <summary>
        /// GET /api/admin/users should return full user information.
        /// </summary>
        [Fact]
        public async Task GetAllUsers_ReturnsFullUserInfo()
        {
            // Seed a user with ID "TestUser" if one does not exist.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Check for required Identity fields: UserName and NormalizedUserName.
                if (!await db.Users.AnyAsync(u => u.Id == "TestUser"))
                {
                    db.Users.Add(new User
                    {
                        Id = "TestUser",
                        UserName = "test.user@example.com",
                        NormalizedUserName = "TEST.USER@EXAMPLE.COM",
                        FirstName = "Test",
                        LastName = "User",
                        Email = "test.user@example.com",
                        CompanyID = 1,
                        RoleID = "TestRole",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
            }

            var response = await _client.GetAsync("/api/admin/users");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            // Validate that response includes the seeded user data.
            Assert.Contains("Test", json);
            Assert.Contains("User", json);
            Assert.Contains("test.user@example.com", json);
            Assert.Contains("1", json);
        }

        /// <summary>
        /// GET /api/admin/roles should return roles.
        /// </summary>
        [Fact]
        public async Task GetAllRoles_ReturnsRoles()
        {
            // Seed a role if none exists.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (!await db.Roles.AnyAsync())
                {
                    db.Roles.Add(new Role
                    {
                        Id = "TestRole",
                        Name = "TestRole",
                        NormalizedName = "TESTROLE",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsCompanyRole = false
                    });
                    await db.SaveChangesAsync();
                }
            }

            var response = await _client.GetAsync("/api/admin/roles");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();

            // Validate that at least one role is returned.
            Assert.Contains("TestRole", json);
        }

        /// <summary>
        /// PUT /api/admin/users/{userId}/role should update the user's role.
        /// </summary>
        [Fact]
        public async Task UpdateUserRole_ReturnsSuccess()
        {
            // Ensure role "TestRole" exists.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (!await db.Roles.AnyAsync(r => r.Name == "TestRole"))
                {
                    db.Roles.Add(new Role
                    {
                        Id = "TestRole",
                        Name = "TestRole",
                        NormalizedName = "TESTROLE",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsCompanyRole = false
                    });
                    await db.SaveChangesAsync();
                }
            }

            // Ensure the user "TestUser" exists with required Identity fields.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (!await db.Users.AnyAsync(u => u.Id == "TestUser"))
                {
                    db.Users.Add(new User
                    {
                        Id = "TestUser",
                        UserName = "test.user@example.com",
                        NormalizedUserName = "TEST.USER@EXAMPLE.COM",
                        FirstName = "Test",
                        LastName = "User",
                        Email = "test.user@example.com",
                        CompanyID = 1,
                        RoleID = "", // Initially blank.
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
            }

            // Construct the update payload.
            var updateDto = new { NewRole = "TestRole" };
            var content = new StringContent(JsonSerializer.Serialize(updateDto), Encoding.UTF8, "application/json");

            // Call the endpoint with user id "TestUser".
            var response = await _client.PutAsync("/api/admin/users/TestUser/role", content);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("User role updated successfully", jsonResponse);
        }

        /// <summary>
        /// PUT /api/admin/roles/{roleId} should update role details.
        /// </summary>
        [Fact]
        public async Task UpdateRole_ReturnsSuccess()
        {
            // Seed a role for updating.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var role = await db.Roles.FirstOrDefaultAsync(r => r.Id == "RoleToUpdate");
                if (role == null)
                {
                    role = new Role
                    {
                        Id = "RoleToUpdate",
                        Name = "RoleToUpdate",
                        NormalizedName = "ROLETOUPDATE",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsCompanyRole = false
                    };
                    db.Roles.Add(role);
                    await db.SaveChangesAsync();
                }
            }

            // Create update DTO with new details.
            var updateRoleDto = new { Name = "UpdatedRole", IsCompanyRole = true, CompanyID = (int?)1 };
            var content = new StringContent(JsonSerializer.Serialize(updateRoleDto), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync("/api/admin/roles/RoleToUpdate", content);
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("Role updated successfully", jsonResponse);
        }

        /// <summary>
        /// GET /api/admin/permissions should return all permissions.
        /// </summary>
        [Fact]
        public async Task GetAllPermissions_ReturnsPermissions()
        {
            // Seed a permission if not already present.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (!await db.Permissions.AnyAsync(p => p.PermissionName == "ADMIN_PANEL_ACCESS"))
                {
                    db.Permissions.Add(new Permission
                    {
                        PermissionID = 3, // Use your appropriate key type/value.
                        PermissionName = "ADMIN_PANEL_ACCESS",
                        Description = "Admin Panel Access",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
            }

            var response = await _client.GetAsync("/api/admin/permissions");
            response.EnsureSuccessStatusCode();
            string json = await response.Content.ReadAsStringAsync();
            Assert.Contains("ADMIN_PANEL_ACCESS", json);
        }

        /// <summary>
        /// POST /api/admin/permissions should create a new permission.
        /// </summary>
        [Fact]
        public async Task CreatePermission_ReturnsSuccess()
        {
            // Create DTO for a new permission.
            var newPermissionDto = new { Name = "NEW_PERMISSION", Description = "New permission for testing" };
            var content = new StringContent(JsonSerializer.Serialize(newPermissionDto), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync("/api/admin/permissions", content);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("Permission created successfully", jsonResponse);
        }

        /// <summary>
        /// GET /api/admin/roles/{roleId}/permissions should return permission IDs for the role.
        /// </summary>
        [Fact]
        public async Task GetPermissionsForRole_ReturnsPermissionIds()
        {
            string roleId = null;

            // Seed role and assign a permission.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Ensure a role exists.
                var role = await db.Roles.FirstOrDefaultAsync();
                if (role == null)
                {
                    role = new Role
                    {
                        Id = "TestRoleForPermissions",
                        Name = "TestRoleForPermissions",
                        NormalizedName = "TESTROLEFORPERMISSIONS",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsCompanyRole = false
                    };
                    db.Roles.Add(role);
                    await db.SaveChangesAsync();
                }
                roleId = role.Id;

                // Ensure the permission exists.
                var permission = await db.Permissions.FirstOrDefaultAsync(p => p.PermissionName == "ADMIN_PANEL_ACCESS");
                if (permission == null)
                {
                    permission = new Permission
                    {
                        PermissionID = 3,
                        PermissionName = "ADMIN_PANEL_ACCESS",
                        Description = "Admin Panel Access",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    db.Permissions.Add(permission);
                    await db.SaveChangesAsync();
                }

                // Seed a role permission if not already assigned.
                if (!await db.RolePermissions.AnyAsync(rp => rp.RoleID == role.Id && rp.PermissionID == permission.PermissionID))
                {
                    db.RolePermissions.Add(new RolePermission
                    {
                        RoleID = role.Id,
                        PermissionID = permission.PermissionID
                    });
                    await db.SaveChangesAsync();
                }
            }

            // Now call the endpoint.
            var response = await _client.GetAsync($"/api/admin/roles/{roleId}/permissions");
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("3", jsonResponse);
        }

        /// <summary>
        /// PUT /api/admin/roles/{roleId}/permissions should update the permissions assigned to a role.
        /// </summary>
        [Fact]
        public async Task UpdateRolePermissions_ReturnsSuccess()
        {
            string roleId = null;

            // Seed a role and at least one permission.
            using (var scope = _factory.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                var role = await db.Roles.FirstOrDefaultAsync();
                if (role == null)
                {
                    role = new Role
                    {
                        Id = "RoleForPermissionUpdate",
                        Name = "RoleForPermissionUpdate",
                        NormalizedName = "ROLEFORPERMISSIONUPDATE",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        IsCompanyRole = false
                    };
                    db.Roles.Add(role);
                    await db.SaveChangesAsync();
                }
                roleId = role.Id;

                // Ensure a permission exists.
                if (!await db.Permissions.AnyAsync())
                {
                    db.Permissions.Add(new Permission
                    {
                        PermissionID = 100,
                        PermissionName = "TEST_PERMISSION",
                        Description = "Test Permission",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                    await db.SaveChangesAsync();
                }
            }

            // Prepare DTO to assign two permissions to the role.
            var dto = new { PermissionIds = new int[] { 3, 100 } };
            var content = new StringContent(JsonSerializer.Serialize(dto), Encoding.UTF8, "application/json");

            var response = await _client.PutAsync($"/api/admin/roles/{roleId}/permissions", content);
            response.EnsureSuccessStatusCode();
            string jsonResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains("Permissions updated successfully", jsonResponse);
        }
    }
}
