using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagmentTool.Data;
using System.Linq;
using System.Threading.Tasks;
using ProjectManagmentTool.DTOs;
using Microsoft.EntityFrameworkCore;

namespace ProjectManagmentTool.Controllers
{
    // Allow both Administrator and CEO to access admin endpoints.
    [Route("api/admin")]
    [ApiController]
    [Authorize(Policy = "AdminPanelAccess")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly ApplicationDbContext _context;

        public AdminController(UserManager<User> userManager, RoleManager<Role> roleManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        // GET: api/admin/users
        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var users = _userManager.Users.Select(u => new
            {
                u.Id,
                u.FirstName,
                u.LastName,
                u.Email,
                Role = u.Role != null ? u.Role.Name : "Not Assigned",
                u.CompanyID
            }).ToList();

            return Ok(users);
        }

        // GET: api/admin/roles
        [HttpGet("roles")]
        public IActionResult GetAllRoles()
        {
            var roles = _roleManager.Roles.Select(r => new
            {
                r.Id,
                r.Name,
                r.CompanyID,
                r.IsCompanyRole
            }).ToList();

            return Ok(roles);
        }

        // PUT: api/admin/users/{userId}/role
        [HttpPut("users/{userId}/role")]
        public async Task<IActionResult> UpdateUserRole(string userId, [FromBody] UpdateUserRoleDTO model)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound("User not found");

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
                return StatusCode(500, "Error removing existing roles");

            if (!await _roleManager.RoleExistsAsync(model.NewRole))
                return BadRequest("The specified role does not exist");

            var addResult = await _userManager.AddToRoleAsync(user, model.NewRole);
            if (!addResult.Succeeded)
                return StatusCode(500, "Error assigning new role");

            // 🛠 FIXED: Look up role by name to get its ID
            var targetRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == model.NewRole);
            if (targetRole == null)
                return BadRequest("Could not find role ID by name.");

            user.RoleID = targetRole.Id;
            await _userManager.UpdateAsync(user);

            return Ok(new { message = "User role updated successfully" });
        }

        [HttpPut("roles/{roleId}")]
        public async Task<IActionResult> UpdateRole(string roleId, [FromBody] UpdateRoleDTO model)
        {
            // Find the role by Id
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
            {
                return NotFound("Role not found");
            }

            // Update role properties from the DTO
            role.Name = model.Name;
            role.IsCompanyRole = model.IsCompanyRole;
            role.CompanyID = model.CompanyID;

            // Update role using RoleManager
            var result = await _roleManager.UpdateAsync(role);
            if (!result.Succeeded)
            {
                return StatusCode(500, "Error updating role");
            }

            return Ok(new { message = "Role updated successfully" });
        }

        [HttpGet("permissions")]
        public IActionResult GetAllPermissions()
        {
            var permissions = _context.Permissions
                .Select(p => new
                {
                    p.PermissionID,
                    p.PermissionName
                })
                .ToList();

            return Ok(permissions);
        }

        [HttpPost("permissions")]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDTO model)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.Name))
            {
                return BadRequest("Permission name is required.");
            }

            // Optional: Check if the permission already exists
            var existing = _context.Permissions.FirstOrDefault(p => p.PermissionName == model.Name);
            if (existing != null)
            {
                return BadRequest("Permission already exists.");
            }

            var permission = new Permission
            {
                PermissionName = model.Name,
                Description = model.Description ?? string.Empty,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Permissions.Add(permission);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Permission created successfully.", permission });
        }

        [HttpGet("roles/{roleId}/permissions")]
        public IActionResult GetPermissionsForRole(string roleId)
        {
            var permissionIds = _context.RolePermissions
                .Where(rp => rp.RoleID == roleId)
                .Select(rp => rp.PermissionID)
                .ToList();

            return Ok(permissionIds);
        }

        [HttpPut("roles/{roleId}/permissions")]
        public async Task<IActionResult> UpdateRolePermissions(string roleId, [FromBody] UpdateRolePermissionsDTO dto)
        {
            var role = await _roleManager.FindByIdAsync(roleId);
            if (role == null)
                return NotFound("Role not found.");

            var existingPermissions = _context.RolePermissions.Where(rp => rp.RoleID == roleId);
            _context.RolePermissions.RemoveRange(existingPermissions);

            var newPermissions = dto.PermissionIds.Select(id => new RolePermission
            {
                RoleID = roleId,
                PermissionID = id
            });

            _context.RolePermissions.AddRange(newPermissions);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Permissions updated successfully" });
        }
    }

    // DTO for updating the user's role
    public class UpdateUserRoleDTO
    {
        public string NewRole { get; set; }
    }
}
