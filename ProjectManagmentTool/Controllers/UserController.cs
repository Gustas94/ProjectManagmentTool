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

        // 1. Get Current Logged-In User Info
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User is not authenticated.");

            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found.");

            return Ok(new
            {
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role != null ? user.Role.Name : "Unknown",
                companyID = user.CompanyID
            });
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
        public async Task<IActionResult> GetUsersByProject(int projectId)
        {
            var users = await _context.ProjectUsers
                .Where(pu => pu.ProjectID == projectId)
                .Select(pu => new
                {
                    pu.User.Id,
                    pu.User.FirstName,
                    pu.User.LastName
                })
                .ToListAsync();

            if (!users.Any())
            {
                return NotFound("No members found for this project.");
            }

            return Ok(users);
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
    }
}
