using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using System.Security.Claims;
using ProjectManagmentTool.Commands;
using ProjectManagmentTool.Queries;
using ProjectManagmentTool.DTOs;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Data;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/groups")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _context;

        public GroupsController(IMediator mediator, ApplicationDbContext context)
        {
            _mediator = mediator;
            _context = context;
        }

        // GET: api/groups/all
        [HttpGet("all")]
        public async Task<IActionResult> GetAllGroups()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User not found.");

            var companyId = await _mediator.Send(new GetCompanyIdQuery(userId));

            var groups = await _mediator.Send(new GetAllGroupsQuery(companyId));
            return Ok(groups);
        }

        // POST: api/groups/create
        [HttpPost("create")]
        public async Task<IActionResult> CreateGroup([FromBody] GroupRequestDTO request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User not found.");

            var companyId = await _mediator.Send(new GetCompanyIdQuery(userId));

            var groupId = await _mediator.Send(new CreateGroupCommand(
                request.GroupName, request.Description, request.GroupLeadID, companyId, request.GroupMemberIDs
            ));

            return Ok(new { message = "Group created successfully", groupID = groupId });
        }

        // GET: api/groups/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGroupById(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized("User not found.");

            var group = await _mediator.Send(new GetGroupByIdQuery(id));
            if (group == null) return NotFound();

            return Ok(group);
        }

        [HttpPost("{groupId}/assign-members")]
        public async Task<IActionResult> AssignMembersToGroup(int groupId, [FromBody] AssignMembersDTO request)
        {
            // Optional: You can add additional validation (e.g. check that the users belong to the same company as the group)

            await _mediator.Send(new AssignGroupMembersCommand(groupId, request.MemberIDs));
            return Ok(new { message = "Members assigned successfully." });
        }

        // GET: api/groups/{groupId}/members
        [HttpGet("{groupId}/members")]
        public async Task<IActionResult> GetGroupMembers(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.GroupMembers)
                    .ThenInclude(gm => gm.User)
                .FirstOrDefaultAsync(g => g.GroupID == groupId);

            if (group == null)
                return NotFound();

            var members = group.GroupMembers.Select(gm => new UserDTO
            {
                Id = gm.User.Id,
                FirstName = gm.User.FirstName,
                LastName = gm.User.LastName
            }).ToList();

            return Ok(members);
        }

        [HttpDelete("{groupId}/members/{userId}")]
        public async Task<IActionResult> RemoveMemberFromGroup(int groupId, string userId)
        {
            var groupMember = await _context.GroupMembers
                .FirstOrDefaultAsync(gm => gm.GroupID == groupId && gm.UserID == userId);

            if (groupMember == null)
                return NotFound("User is not a member of this group.");

            _context.GroupMembers.Remove(groupMember);

            // Get all project IDs that this group is assigned to
            var affectedProjectIds = await _context.ProjectGroups
                .Where(pg => pg.GroupID == groupId)
                .Select(pg => pg.ProjectID)
                .ToListAsync();

            foreach (var projectId in affectedProjectIds)
            {
                // Check if user is directly assigned to this project
                bool isIndividuallyAssigned = await _context.ProjectUsers
                    .AnyAsync(pu => pu.ProjectID == projectId && pu.UserID == userId &&
                        !_context.GroupMembers.Any(gm => gm.UserID == userId &&
                            _context.ProjectGroups.Any(pg => pg.ProjectID == projectId && pg.GroupID == gm.GroupID)
                        )
                    );

                // Check if user is part of another group that’s assigned to this project
                bool isInOtherAssignedGroups = await _context.ProjectGroups
                    .AnyAsync(pg =>
                        pg.ProjectID == projectId &&
                        pg.GroupID != groupId &&
                        _context.GroupMembers.Any(gm => gm.GroupID == pg.GroupID && gm.UserID == userId)
                    );

                // Remove user from project if no other group or direct assignment justifies their membership
                if (!isInOtherAssignedGroups && !isIndividuallyAssigned)
                {
                    var userProject = await _context.ProjectUsers
                        .FirstOrDefaultAsync(pu => pu.ProjectID == projectId && pu.UserID == userId);

                    if (userProject != null)
                        _context.ProjectUsers.Remove(userProject);
                }
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "User removed from group and cleaned up from related projects." });
        }
    }
}
