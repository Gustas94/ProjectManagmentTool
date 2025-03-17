using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using System.Threading.Tasks;
using System.Security.Claims;
using ProjectManagmentTool.Commands;
using ProjectManagmentTool.Queries;
using ProjectManagmentTool.DTOs;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/groups")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public GroupsController(IMediator mediator)
        {
            _mediator = mediator;
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
    }
}
