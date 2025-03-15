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
                GroupID = null, // not used in this scenario
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

            // Assign groups: retrieve group members and add them if not already added
            if (request.AssignedGroupIDs != null)
            {
                foreach (var groupId in request.AssignedGroupIDs)
                {
                    var groupMembers = await _context.GroupMembers
                        .Where(gm => gm.GroupID == groupId)
                        .Select(gm => gm.UserID)
                        .ToListAsync();

                    foreach (var memberId in groupMembers)
                    {
                        // Check if an assignment for this (TaskID, UserID) is already being tracked or exists in the database.
                        bool alreadyTracked = _context.ChangeTracker.Entries<UserTask>()
                            .Any(e => e.Entity.TaskID == task.TaskID && e.Entity.UserID == memberId);
                        bool alreadyInDb = await _context.UserTasks.AnyAsync(ut => ut.TaskID == task.TaskID && ut.UserID == memberId);

                        if (!alreadyTracked && !alreadyInDb)
                        {
                            _context.UserTasks.Add(new UserTask { UserID = memberId, TaskID = task.TaskID });
                        }
                    }
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
                    t.ProjectID, // ✅ Ensure ProjectID is included
                    ProjectName = _context.Projects
                                        .Where(p => p.ProjectID == t.ProjectID)
                                        .Select(p => p.ProjectName)
                                        .FirstOrDefault(),
                    AssignedUsers = _context.UserTasks
                                        .Where(ut => ut.TaskID == t.TaskID)
                                        .Join(_context.Users,
                                              ut => ut.UserID,
                                              u => u.Id,
                                              (ut, u) => new { u.Id, u.FirstName, u.LastName })
                                        .ToList(),
                    AssignedGroups = _context.Groups
                                        .Where(g => g.Tasks.Any(gt => gt.TaskID == t.TaskID))
                                        .Select(g => new { g.GroupID, g.GroupName })
                                        .ToList()
                })
                .FirstOrDefaultAsync();

            if (task == null)
                return NotFound("Task not found.");

            return Ok(task);
        }
    }
}

        // Single TaskRequestDTO definition with a list of assignee user IDs.
        public class TaskRequestDTO
    {
        public string TaskName { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public int ProjectID { get; set; }
        // New: list of individual user IDs to assign the task
        public List<string> AssignedUserIDs { get; set; }
        // New: list of group IDs to assign the task
        public List<int> AssignedGroupIDs { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
    }

