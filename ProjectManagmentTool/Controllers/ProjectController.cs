using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/projects")]
    [ApiController]
    [Authorize]
    public class ProjectController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public ProjectController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProject([FromBody] ProjectRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.ProjectManagerID))
                return BadRequest("Project Manager ID is required.");

            if (request.CompanyID <= 0)
                return BadRequest("Company ID is required.");

            var project = new Project
            {
                ProjectName = request.ProjectName,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CompanyID = request.CompanyID,  // ✅ Use CompanyID directly
                ProjectManagerID = request.ProjectManagerID, // ✅ Use ProjectManagerID directly
                Visibility = request.Visibility,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Project created successfully", projectId = project.ProjectID });
        }

        [HttpGet("managers/{companyId}")]
        public async Task<IActionResult> GetManagers(int companyId)
        {
            var users = await _context.Users
                .Where(u => u.CompanyID == companyId)
                .Select(u => new { u.Id, u.FirstName, u.LastName })
                .ToListAsync();

            return Ok(users);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllProjects()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User is not authenticated.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var projects = await _context.Projects
                .Where(p => p.CompanyID == user.CompanyID)  // ✅ Only show projects from user's company
                .Select(p => new
                {
                    p.ProjectID,
                    p.ProjectName,
                    p.Description,
                    p.StartDate,
                    p.EndDate,
                    p.Visibility,
                    ProjectManagerName = _context.Users
                        .Where(u => u.Id == p.ProjectManagerID)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault(),  // ✅ Get Project Manager Name

                    MembersCount = _context.UserProjects
                        .Where(up => up.ProjectID == p.ProjectID)
                        .Count(),  // ✅ Count Members
                    p.CreatedAt,
                    p.UpdatedAt
                })
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{projectId}")]
        public async Task<IActionResult> GetProjectById(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User is not authenticated.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var project = await _context.Projects
                .Where(p => p.ProjectID == projectId && p.CompanyID == user.CompanyID) // Only allow access to the same company
                .Select(p => new
                {
                    p.ProjectID,
                    p.ProjectName,
                    p.Description,
                    p.StartDate,
                    p.EndDate,
                    p.Visibility,
                    ProjectManagerName = _context.Users
                        .Where(u => u.Id == p.ProjectManagerID)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault(),

                    MembersCount = _context.UserProjects
                        .Where(up => up.ProjectID == p.ProjectID)
                        .Count(),

                    p.CreatedAt,
                    p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            if (project == null)
                return NotFound("Project not found or you do not have access to it.");

            return Ok(project);
        }
    }
}
