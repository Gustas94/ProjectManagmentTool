using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Data;
using ProjectManagmentTool.DTOs;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/users")]
    [ApiController]
    [Authorize] // Ensure only authenticated users can access these endpoints
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public UserController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // 2. Get All Users in a Company (For Project Manager Selection)
        [HttpGet("company/{companyId}")]
        public async Task<IActionResult> GetUsersByCompany(int companyId)
        {
            var users = await _context.Users
                .Where(u => u.CompanyID == companyId)
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToListAsync();

            return Ok(users);
        }

        // 3. Get Specific User by ID
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById(string userId)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found.");

            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                role = user.Role != null ? user.Role.Name : "Unknown",
                companyID = user.CompanyID
            });
        }

        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetUsersForProject(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == projectId && p.CompanyID == user.CompanyID);

            if (project == null)
                return NotFound("Project not found or you don't have access.");

            // 🟩 Direct assignments
            var directAssignments = await _context.ProjectUsers
                .Where(pu => pu.ProjectID == projectId)
                .Include(pu => pu.User)
                .ToListAsync();

            var directUserMap = directAssignments
                .ToDictionary(pu => pu.UserID, pu => pu.User);

            // 🟩 Group-based assignments
            var groupAssignments = await _context.ProjectGroups
                .Where(pg => pg.ProjectID == projectId)
                .SelectMany(pg => _context.GroupMembers
                    .Where(gm => gm.GroupID == pg.GroupID)
                    .Select(gm => new
                    {
                        gm.UserID,
                        GroupName = _context.Groups
                            .Where(g => g.GroupID == gm.GroupID)
                            .Select(g => g.GroupName)
                            .FirstOrDefault(),
                        User = gm.User
                    }))
                .ToListAsync();

            // 🟩 Merge all users
            var merged = new Dictionary<string, (User user, List<string> sources)>();

            // Add group users first
            foreach (var g in groupAssignments)
            {
                if (!merged.ContainsKey(g.UserID))
                    merged[g.UserID] = (g.User, new List<string>());

                merged[g.UserID].sources.Add(g.GroupName);
            }

            // Add direct users and mark as "Direct"
            foreach (var kvp in directUserMap)
            {
                if (!merged.ContainsKey(kvp.Key))
                    merged[kvp.Key] = (kvp.Value, new List<string>());

                if (!merged[kvp.Key].sources.Any(s => s == "Direct"))
                    merged[kvp.Key].sources.Add("Direct");
            }

            var result = merged.Select(kvp => new
            {
                id = kvp.Value.user.Id,
                firstName = kvp.Value.user.FirstName,
                lastName = kvp.Value.user.LastName,
                assignedVia = kvp.Value.sources.Distinct().ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpPost("project/add")]
        public async Task<IActionResult> AddUserToProject([FromBody] AddUserToProjectRequest request)
        {
            var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserID);
            if (!userExists)
                return NotFound("User not found.");

            var projectExists = await _context.Projects.AnyAsync(p => p.ProjectID == request.ProjectID);
            if (!projectExists)
                return NotFound("Project not found.");

            var existingAssignment = await _context.ProjectUsers
                .AnyAsync(pu => pu.UserID == request.UserID && pu.ProjectID == request.ProjectID);

            if (existingAssignment)
                return BadRequest("User is already assigned to this project.");

            var projectUser = new ProjectUser
            {
                UserID = request.UserID,
                ProjectID = request.ProjectID
            };

            _context.ProjectUsers.Add(projectUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User added to project successfully." });
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value; // ✅ this is more reliable
            if (userId == null) return Unauthorized();

            var user = await _userManager.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            var permissions = await _context.RolePermissions
                .Where(rp => rp.RoleID == user.RoleID)
                .Select(rp => rp.Permission.PermissionName)
                .ToListAsync();

            return Ok(new
            {
                id = user.Id,
                firstName = user.FirstName,
                lastName = user.LastName,
                email = user.Email,
                role = user.Role?.Name,
                companyID = user.CompanyID,
                permissions
            });
        }
    }
}
