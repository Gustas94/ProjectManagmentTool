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
using ProjectManagmentTool.Observers;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/tasks")]
    [ApiController]
    [Authorize]
    public class TaskController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly TaskSubject _taskSubject;

        public TaskController(ApplicationDbContext context, UserManager<User> userManager, TaskSubject taskSubject)
        {
            _context = context;
            _userManager = userManager;
            _taskSubject = taskSubject;
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

        [Authorize(Policy = "CreateTaskPolicy")]
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
                Status = request.Status ?? "To Do",
                Priority = request.Priority ?? "Medium",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync(); // Task.TaskID is now set

            // Process individual user assignments
            if (request.AssignedUserIDs != null)
            {
                foreach (var userId in request.AssignedUserIDs)
                {
                    if (!await _context.UserTasks.AnyAsync(ut => ut.TaskID == task.TaskID && ut.UserID == userId))
                    {
                        _context.UserTasks.Add(new UserTask { TaskID = task.TaskID, UserID = userId });
                    }
                }
            }

            // Process group assignments
            if (request.AssignedGroupIDs != null)
            {
                foreach (var groupId in request.AssignedGroupIDs)
                {
                    if (!await _context.TaskGroups.AnyAsync(tg => tg.TaskID == task.TaskID && tg.GroupID == groupId))
                    {
                        _context.TaskGroups.Add(new TaskGroup { TaskID = task.TaskID, GroupID = groupId });
                    }
                }
            }

            await _context.SaveChangesAsync();

            // Optionally, requery the task with join data so you can see the assigned users/groups immediately.
            var createdTask = await _context.Tasks
                .Where(t => t.TaskID == task.TaskID)
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
                    individualAssignedUsers = _context.UserTasks
                        .Where(ut => ut.TaskID == t.TaskID)
                        .Join(_context.Users,
                              ut => ut.UserID,
                              u => u.Id,
                              (ut, u) => new { u.Id, u.FirstName, u.LastName })
                        .ToList(),
                    assignedGroups = _context.TaskGroups
                        .Where(tg => tg.TaskID == t.TaskID)
                        .Join(_context.Groups,
                              tg => tg.GroupID,
                              g => g.GroupID,
                              (tg, g) => new { g.GroupID, g.GroupName })
                        .ToList(),
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

            return Ok(new { message = "Task created successfully", task = createdTask });
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

        [HttpPost("{taskId}/clone")]
        public async Task<IActionResult> CloneTask(int taskId)
        {
            var existingTask = await _context.Tasks.FindAsync(taskId);
            if (existingTask == null)
                return NotFound("Task not found.");

            var clonedTask = existingTask.Clone(); // Use Prototype Pattern
            _context.Tasks.Add(clonedTask);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task cloned successfully", clonedTaskId = clonedTask.TaskID });
        }

        [HttpDelete("{taskId}/assign/group/{groupId}")]
        public async Task<IActionResult> RemoveGroupAssignment(int taskId, int groupId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) return NotFound("Task not found.");

            var taskGroup = await _context.TaskGroups.FirstOrDefaultAsync(tg => tg.TaskID == taskId && tg.GroupID == groupId);
            if (taskGroup == null)
                return NotFound("Group assignment not found.");

            _context.TaskGroups.Remove(taskGroup);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Group removed successfully" });
        }

        [HttpDelete("{taskId}/assign/user/{userId}")]
        public async Task<IActionResult> RemoveUserAssignment(int taskId, string userId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound("Task not found.");

            var userTask = await _context.UserTasks.FirstOrDefaultAsync(ut => ut.TaskID == taskId && ut.UserID == userId);
            if (userTask == null)
                return NotFound("Direct user assignment not found.");

            _context.UserTasks.Remove(userTask);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Direct user assignment removed successfully" });
        }

        [HttpPut("{taskId}")]
        public async Task<IActionResult> UpdateTask(int taskId, [FromBody] TaskRequestDTO request)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound("Task not found.");

            task.TaskName = request.TaskName;
            task.Description = request.Description;
            task.Deadline = request.Deadline;
            task.Priority = request.Priority ?? "Medium";
            task.Status = request.Status ?? "To Do";
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Task updated successfully" });
        }

        [HttpDelete("{taskId}")]
        public async Task<IActionResult> DeleteTask(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound("Task not found.");

            // Remove associated direct user assignments.
            var userTasks = _context.UserTasks.Where(ut => ut.TaskID == taskId);
            _context.UserTasks.RemoveRange(userTasks);

            // Remove associated group assignments.
            var taskGroups = _context.TaskGroups.Where(tg => tg.TaskID == taskId);
            _context.TaskGroups.RemoveRange(taskGroups);

            // Now remove the task itself.
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Task and all associated assignments deleted successfully" });
        }

        [Authorize(Policy = "CompleteTaskPolicy")]
        [HttpPut("{taskId}/complete")]
        public async Task<IActionResult> MarkAsCompleted(int taskId)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null)
                return NotFound("Task not found.");

            task.Status = "Completed";
            task.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Task marked as completed." });
        }

        [HttpGet("project/{projectId}/progress")]
        public async Task<IActionResult> GetProjectTaskProgress(int projectId)
        {
            var totalTasks = await _context.Tasks.CountAsync(t => t.ProjectID == projectId);
            var completedTasks = await _context.Tasks.CountAsync(t => t.ProjectID == projectId && t.Status == "Completed");

            return Ok(new { projectId, totalTasks, completedTasks });
        }

        [HttpGet("project/{projectId}/search")]
        public async Task<IActionResult> SearchTasksByProject(
        int projectId,
        [FromQuery] string searchTerm = null,
        [FromQuery] string status = null,
        [FromQuery] string priority = null,
        [FromQuery] string sortBy = null,
        [FromQuery] string sortOrder = "asc",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
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

            // Start with tasks for the project.
            var query = _context.Tasks.AsQueryable().Where(t => t.ProjectID == projectId);

            // Apply search filter for task name or description.
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(t => t.TaskName.Contains(searchTerm) || t.Description.Contains(searchTerm));
            }

            // Filter by status if provided.
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(t => t.Status == status);
            }

            // Filter by priority if provided.
            if (!string.IsNullOrWhiteSpace(priority))
            {
                query = query.Where(t => t.Priority == priority);
            }

            // Apply sorting based on query parameters.
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                // For simplicity, use a switch statement. In more advanced cases consider using Dynamic LINQ.
                if (sortOrder.ToLower() == "desc")
                {
                    query = sortBy.ToLower() switch
                    {
                        "deadline" => query.OrderByDescending(t => t.Deadline),
                        "priority" => query.OrderByDescending(t => t.Priority),
                        _ => query.OrderByDescending(t => t.TaskName)
                    };
                }
                else
                {
                    query = sortBy.ToLower() switch
                    {
                        "deadline" => query.OrderBy(t => t.Deadline),
                        "priority" => query.OrderBy(t => t.Priority),
                        _ => query.OrderBy(t => t.TaskName)
                    };
                }
            }
            else
            {
                // Default ordering.
                query = query.OrderBy(t => t.TaskName);
            }

            // Optionally add pagination.
            var totalTasks = await query.CountAsync();
            var tasks = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    t.Description,
                    t.Deadline,
                    t.Status,
                    t.Priority,
                    // Retrieve assigned user names via the join table if needed.
                    AssignedTo = _context.UserTasks
                                    .Where(ut => ut.TaskID == t.TaskID)
                                    .Join(_context.Users,
                                          ut => ut.UserID,
                                          u => u.Id,
                                          (ut, u) => u.FirstName + " " + u.LastName)
                                    .ToList()
                })
                .ToListAsync();

            return Ok(new
            {
                totalTasks,
                page,
                pageSize,
                tasks
            });
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
