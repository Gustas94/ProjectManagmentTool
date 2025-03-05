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

        // ✅ Get all tasks for a project
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetTasksByProject(int projectId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return Unauthorized("User is not authenticated.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("User not found.");

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null || project.CompanyID != user.CompanyID)
                return NotFound("Project not found or you don't have access.");

            var tasks = await _context.Tasks  // ✅ Changed from Task -> ProjectTask
                .Where(t => t.ProjectID == projectId)
                .Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    t.Description,
                    t.Deadline,
                    t.Status,
                    t.Priority,
                    AssignedTo = _context.Users
                        .Where(u => u.Id == t.AssignedTo)
                        .Select(u => u.FirstName + " " + u.LastName)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // Create a new task in a project
        [HttpPost("create")]
        public async Task<IActionResult> CreateTask([FromBody] TaskRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.TaskName))
                return BadRequest("Task Name is required.");

            if (request.AssignedTo == null || !request.AssignedTo.Any())
                return BadRequest("At least one assignee is required.");

            var user = await _context.Users.FindAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
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
                GroupID = null, // No group assignment for now
                Status = request.Status ?? "To Do",
                Priority = request.Priority ?? "Medium",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            foreach (var userId in request.AssignedTo)
            {
                _context.UserTasks.Add(new UserTask { UserID = userId, TaskID = task.TaskID });
            }
            await _context.SaveChangesAsync();

            return Ok(new { message = "Task created successfully", taskId = task.TaskID });
        }

        public class TaskRequestDTO
        {
            public string TaskName { get; set; }
            public string Description { get; set; }
            public DateTime Deadline { get; set; }
            public int ProjectID { get; set; }
            public List<string> AssignedTo { get; set; }
            public string Status { get; set; }
            public string Priority { get; set; }
        }
    }

        public class TaskRequestDTO
    {
        public string TaskName { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public int ProjectID { get; set; }
        public string AssignedTo { get; set; }
        public string Status { get; set; }
        public string Priority { get; set; }
    }
}
