using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Commands;
using ProjectManagmentTool.Data;
using ProjectManagmentTool.Repositories;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectManagmentTool.Handlers
{
    public class AssignGroupMembersHandler : IRequestHandler<AssignGroupMembersCommand, Unit>
    {
        private readonly IGroupRepository _groupRepository;
        private readonly ApplicationDbContext _context;

        public AssignGroupMembersHandler(IGroupRepository groupRepository, ApplicationDbContext context)
        {
            _groupRepository = groupRepository;
            _context = context;
        }

        public async Task<Unit> Handle(AssignGroupMembersCommand request, CancellationToken cancellationToken)
        {
            // Ensure the group exists.
            var group = await _groupRepository.GetGroupById(request.GroupId);
            if (group == null)
            {
                throw new System.Exception("Group not found.");
            }

            // Get already-assigned member IDs for the group.
            var existingMemberIDs = await _context.GroupMembers
                .Where(gm => gm.GroupID == request.GroupId)
                .Select(gm => gm.UserID)
                .ToListAsync(cancellationToken);

            // Filter out any duplicate assignments.
            var newMembers = request.MemberIDs
                .Where(id => !existingMemberIDs.Contains(id))
                .Select(id => new GroupMember { GroupID = request.GroupId, UserID = id })
                .ToList();

            if (newMembers.Any())
            {
                await _groupRepository.AddGroupMembers(newMembers);
                await _groupRepository.SaveChangesAsync();
            }

            return Unit.Value;
        }
    }
}
