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

        [HttpGet("{projectId}/groups")]
        public async Task<IActionResult> GetGroupsForProject(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User is not authenticated.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == projectId && p.CompanyID == user.CompanyID);
            if (project == null)
                return NotFound("Project not found or you don't have access.");

            var assignedGroups = await _context.ProjectGroups
                .Where(pg => pg.ProjectID == projectId)
                .Select(pg => new
                {
                    pg.Group.GroupID,
                    pg.Group.GroupName,
                    pg.Group.Description,
                    GroupLeadName = pg.Group.GroupLead != null ? pg.Group.GroupLead.FirstName + " " + pg.Group.GroupLead.LastName : "N/A"
                })
                .ToListAsync();

            return Ok(assignedGroups);
        }

        [HttpPost("{projectId}/assign-group")]
        public async Task<IActionResult> AssignGroupToProject(int projectId, [FromBody] AssignGroupRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User is not authenticated.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var project = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == projectId && p.CompanyID == user.CompanyID);
            if (project == null)
                return NotFound("Project not found or you don't have access.");

            var group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupID == request.GroupID);
            if (group == null)
                return NotFound("Group not found.");

            bool alreadyAssigned = await _context.ProjectGroups
                .AnyAsync(pg => pg.ProjectID == projectId && pg.GroupID == request.GroupID);
            if (alreadyAssigned)
                return BadRequest("Group is already assigned to this project.");

            var projectGroup = new ProjectGroup
            {
                ProjectID = projectId,
                GroupID = request.GroupID
            };

            _context.ProjectGroups.Add(projectGroup);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Group assigned to project successfully." });
        }

        public class AssignGroupRequest
        {
            public int GroupID { get; set; }
        }

    }
}
