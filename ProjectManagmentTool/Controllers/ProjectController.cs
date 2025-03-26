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

            // 1. Assign the group to the project
            var projectGroup = new ProjectGroup
            {
                ProjectID = projectId,
                GroupID = request.GroupID
            };
            _context.ProjectGroups.Add(projectGroup);

            // 2. Fetch group members
            var groupMemberUserIds = await _context.GroupMembers
                .Where(gm => gm.GroupID == request.GroupID)
                .Select(gm => gm.UserID)
                .ToListAsync();

            // 3. Fetch existing project users
            var existingUserProjects = await _context.UserProjects
                .Where(up => up.ProjectID == projectId && groupMemberUserIds.Contains(up.UserID))
                .ToListAsync();

            var existingUserIds = existingUserProjects.Select(up => up.UserID).ToHashSet();

            // 4. Add new user-project entries for members not already in the project
            var newUserProjects = groupMemberUserIds
                .Where(userID => !existingUserIds.Contains(userID))
                .Select(userID => new UserProject
                {
                    ProjectID = projectId,
                    UserID = userID,
                    GroupID = request.GroupID
                })
                .ToList();

            _context.UserProjects.AddRange(newUserProjects);

            // 5. Update existing entries (if any) to set the correct GroupID
            foreach (var existingEntry in existingUserProjects)
            {
                if (existingEntry.GroupID != request.GroupID)
                {
                    existingEntry.GroupID = request.GroupID;
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Group assigned to project successfully, including its members." });
        }



        [HttpDelete("{projectId}/remove-group/{groupId}")]
        public async Task<IActionResult> RemoveGroupFromProject(int projectId, int groupId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return Unauthorized();

            var user = await _context.Users.FindAsync(userId);
            if (user == null) return NotFound();

            var projectGroup = await _context.ProjectGroups
                .FirstOrDefaultAsync(pg => pg.ProjectID == projectId && pg.GroupID == groupId);

            if (projectGroup == null)
                return NotFound("Group not assigned to project.");

            _context.ProjectGroups.Remove(projectGroup);

            // Remove users only assigned via this group
            var groupMembers = await _context.GroupMembers
                .Where(gm => gm.GroupID == groupId)
                .Select(gm => gm.UserID)
                .ToListAsync();

            var usersToRemove = await _context.ProjectUsers
                .Where(pu => pu.ProjectID == projectId && groupMembers.Contains(pu.UserID))
                .ToListAsync();

            foreach (var pu in usersToRemove)
            {
                bool isInOtherAssignedGroups = await _context.ProjectGroups
                    .AnyAsync(pg =>
                        pg.ProjectID == projectId &&
                        _context.GroupMembers.Any(gm =>
                            gm.GroupID == pg.GroupID &&
                            gm.UserID == pu.UserID
                        )
                    );

                bool isIndividuallyAssigned = await _context.ProjectUsers
                    .AnyAsync(p =>
                        p.ProjectID == projectId &&
                        p.UserID == pu.UserID &&
                        !_context.GroupMembers.Any(gm => gm.GroupID == groupId && gm.UserID == pu.UserID)
                    );

                if (!isInOtherAssignedGroups && !isIndividuallyAssigned)
                {
                    _context.ProjectUsers.Remove(pu);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Group and relevant users removed from project." });
        }



        public class AssignGroupRequest
        {
            public int GroupID { get; set; }
        }

    }
}
