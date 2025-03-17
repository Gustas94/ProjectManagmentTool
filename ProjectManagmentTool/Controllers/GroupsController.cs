using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using ProjectManagmentTool.Design_Patterns;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/groups")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GroupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/groups/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllGroups()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User not found.");

            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { u.CompanyID })
                .FirstOrDefaultAsync();

            if (user == null || user.CompanyID == null)
                return BadRequest("User's company could not be determined.");

            var groups = await _context.Groups
                .Where(g => g.CompanyID == user.CompanyID)
                .Select(g => new
                {
                    g.GroupID,
                    g.GroupName,
                    g.Description,
                    GroupLeadName = g.GroupLead != null ? g.GroupLead.FirstName + " " + g.GroupLead.LastName : "N/A",
                    g.CreatedAt,
                    g.UpdatedAt,
                    g.CompanyID
                })
                .ToListAsync();

            return Ok(groups);
        }



        // POST: api/groups/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromBody] GroupRequestDTO request)
        {
            if (string.IsNullOrEmpty(request.GroupName))
                return BadRequest("Group Name is required.");

            if (request.GroupMemberIDs == null || !request.GroupMemberIDs.Any())
                return BadRequest("At least one group member must be selected.");

            if (string.IsNullOrEmpty(request.GroupLeadID))
                return BadRequest("Group Lead must be selected.");

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
                return Unauthorized("User not found.");

            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.CompanyID == null)
                return BadRequest("User's company could not be determined.");

            int? companyId = user.CompanyID;

            // Use Factory Pattern to create a new Group
            var group = GroupFactory.CreateGroup(request.GroupName, request.Description, request.GroupLeadID, companyId);

            _context.Groups.Add(group);
            await _context.SaveChangesAsync(); // Save group first so we can reference it in members

            // Check which members are already in this group to avoid duplicates
            var existingMemberIds = await _context.GroupMembers
                .Where(gm => gm.GroupID == group.GroupID)
                .Select(gm => gm.UserID)
                .ToListAsync();

            var newMembers = request.GroupMemberIDs
                .Where(memberId => !existingMemberIds.Contains(memberId)) // Prevent duplicates in this group
                .Select(memberId => new GroupMember { GroupID = group.GroupID, UserID = memberId })
                .ToList();

            if (newMembers.Any())
            {
                _context.GroupMembers.AddRange(newMembers);
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Group created successfully", groupID = group.GroupID });
        }


        // GET: api/groups/{groupId}
        [HttpGet("{groupId}")]
        public async Task<IActionResult> GetGroupDetails(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.GroupLead)
                .Include(g => g.ProjectGroups)
                    .ThenInclude(pg => pg.Project) // Load associated projects
                .Include(g => g.Tasks) // Load associated tasks
                .FirstOrDefaultAsync(g => g.GroupID == groupId);

            if (group == null)
            {
                return NotFound(new { message = "Group not found" });
            }

            return Ok(new
            {
                group.GroupID,
                group.GroupName,
                group.Description,
                ProjectAffiliation = group.ProjectGroups.Any()
                    ? group.ProjectGroups.Select(pg => pg.Project.ProjectName).ToList()
                    : new List<string> { "N/A" },
                GroupLeadName = group.GroupLead != null
                    ? $"{group.GroupLead.FirstName} {group.GroupLead.LastName}"
                    : "N/A",
                CurrentProgress = "0%", // If you add progress tracking, update this
                Status = "Active",
                CreatedAt = group.CreatedAt,
                UpdatedAt = group.UpdatedAt,
                Tasks = group.Tasks.Select(t => new
                {
                    t.TaskID,
                    t.TaskName,
                    t.Description,
                    t.Deadline,
                    t.Priority,
                    t.Status
                }).ToList()
            });
        }
    }

        public class GroupRequestDTO
    {
        public string GroupName { get; set; }
        public string Description { get; set; }
        // The selected team lead (must be one of the selected group members)
        public string GroupLeadID { get; set; }
        // List of selected company user IDs to add as group members
        public List<string> GroupMemberIDs { get; set; }
    }
}
