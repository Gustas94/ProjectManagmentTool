using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public TaskController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Get all tasks for a project
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProject(int projectId)
        {
            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (currentUserId == null)
                return Unauthorized("User is not authenticated.");

            var user = await _context.Users.FindAsync(currentUserId);
            if (user == null)
                return NotFound("User not found.");

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null || project.CompanyID != user.CompanyID)
                return NotFound("Project not found or you don't have access.");

            var tasks = await _context.Tasks
                .Where(t => t.ProjectID == projectId)
                .Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    t.Description,
                    t.Deadline,
                    t.Status,
                    t.Priority,
                    // Retrieve assigned user names via the join table
                    AssignedTo = _context.UserTasks
                                    .Where(ut => ut.TaskID == t.TaskID)
                                    .Join(_context.Users,
                                          ut => ut.UserID,
                                          u => u.Id,
                                          (ut, u) => u.FirstName + " " + u.LastName)
                                    .ToList()
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] TaskRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.TaskName))
                return BadRequest("Task Name is required.");

            if ((request.AssignedUserIDs == null || !request.AssignedUserIDs.Any()) &&
                (request.AssignedGroupIDs == null || !request.AssignedGroupIDs.Any()))
                return BadRequest("At least one assignee (user or group) is required.");

            var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(currentUserId);
            if (user == null)
                return Unauthorized("User not found.");

            var project = await _context.Projects.FindAsync(request.ProjectID);
            if (project == null || project.CompanyID != user.CompanyID)
                return NotFound("Project not found or you don't have access.");

            var task = new ProjectTask
            {
                TaskName = request.TaskName,
                Description = request.Description,
                Deadline = request.Deadline,
                ProjectID = request.ProjectID,
                // Remove direct GroupID usage since we use a join table now
                Status = request.Status ?? "To Do",
                Priority = request.Priority ?? "Medium",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Assign individual users
            if (request.AssignedUserIDs != null)
            {
                foreach (var userId in request.AssignedUserIDs)
                {
                    _context.UserTasks.Add(new UserTask { UserID = userId, TaskID = task.TaskID });
                }
            }

            // Assign groups via a join table (TaskGroup)
            if (request.AssignedGroupIDs != null)
            {
                foreach (var groupId in request.AssignedGroupIDs)
                {
                    _context.TaskGroups.Add(new TaskGroup { TaskID = task.TaskID, GroupID = groupId });
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = "Task created successfully", taskId = task.TaskID });
        }

        [HttpGet("{taskId}")]
        public async Task<IActionResult> GetTaskById(int taskId)
        {
            var task = await _context.Tasks
                .Where(t => t.TaskID == taskId)
                .Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    t.Description,
                    t.Deadline,
                    t.Status,
                    t.Priority,
                    t.ProjectID,
                    ProjectName = _context.Projects
                                        .Where(p => p.ProjectID == t.ProjectID)
                                        .Select(p => p.ProjectName)
                                        .FirstOrDefault(),
                    // Individually assigned users
                    individualAssignedUsers = _context.UserTasks
                        .Where(ut => ut.TaskID == t.TaskID)
                        .Join(_context.Users,
                              ut => ut.UserID,
                              u => u.Id,
                              (ut, u) => new { u.Id, u.FirstName, u.LastName })
                        .ToList(),
                    // Assigned groups (from TaskGroups join)
                    assignedGroups = _context.TaskGroups
                        .Where(tg => tg.TaskID == t.TaskID)
                        .Join(_context.Groups,
                              tg => tg.GroupID,
                              g => g.GroupID,
                              (tg, g) => new { g.GroupID, g.GroupName })
                        .ToList(),
                    // Users assigned via groups – join TaskGroups with GroupMembers and Users.
                    groupAssignedUsers = _context.TaskGroups
                        .Where(tg => tg.TaskID == t.TaskID)
                        .SelectMany(tg =>
                            _context.GroupMembers
                                .Where(gm => gm.GroupID == tg.GroupID)
                                .Join(_context.Users,
                                      gm => gm.UserID,
                                      u => u.Id,
                                      (gm, u) => new { u.Id, u.FirstName, u.LastName, GroupName = tg.Group.GroupName })
                        )
                        .Distinct()
                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (task == null)
                return NotFound("Task not found.");

            return Ok(task);
        }

        // NEW: POST endpoint to assign users and groups to an existing task
        [HttpPost("{taskId}/assign")]
        public async Task<IActionResult> AssignUsersAndGroups(int taskId, [FromBody] TaskAssignDTO request)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound("Task not found.");

            // Add individual assignments if not already present
            if (request.AssignedUserIDs != null)
            {
                foreach (var userId in request.AssignedUserIDs)
                {
                    if (!await _context.UserTasks.AnyAsync(ut => ut.TaskID == taskId && ut.UserID == userId))
                    {
                        _context.UserTasks.Add(new UserTask { TaskID = taskId, UserID = userId });
                    }
                }
            }

            // Add group assignments via TaskGroups join table if not already present
            if (request.AssignedGroupIDs != null)
            {
                foreach (var groupId in request.AssignedGroupIDs)
                {
                    if (!await _context.TaskGroups.AnyAsync(tg => tg.TaskID == taskId && tg.GroupID == groupId))
                    {
                        _context.TaskGroups.Add(new TaskGroup { TaskID = taskId, GroupID = groupId });
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Assignment updated successfully" });
        }
    }
}

public class TaskRequestDTO
{
    public string TaskName { get; set; }
    public string Description { get; set; }
    public DateTime Deadline { get; set; }
    public int ProjectID { get; set; }
    // List of individually assigned user IDs.
    public List<string> AssignedUserIDs { get; set; }
    // List of group IDs to assign to the task.
    public List<int> AssignedGroupIDs { get; set; }
    public string Status { get; set; }
    public string Priority { get; set; }
}

public class TaskAssignDTO
{
    public List<string> AssignedUserIDs { get; set; }
    public List<int> AssignedGroupIDs { get; set; }
}
