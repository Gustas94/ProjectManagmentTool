using ProjectManagmentTool.Commands;

namespace ProjectManagmentTool.Handlers
{
    using MediatR;
    using ProjectManagmentTool.Data;
    using ProjectManagmentTool.Repositories;
    using ProjectManagmentTool.Factories;  // ✅ PRIDĖTA GAMYKLA
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    public class CreateGroupHandler : IRequestHandler<CreateGroupCommand, int>
    {
        private readonly IGroupRepository _groupRepository;

        public CreateGroupHandler(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public async Task<int> Handle(CreateGroupCommand request, CancellationToken cancellationToken)
        {
            // NAUDOJAM GAMYKLOS METODĄ GRUPEI SUKURTI
            var group = GroupFactory.CreateGroup(
                request.GroupName,
                request.Description,
                request.GroupLeadID,
                request.CompanyID
            );

            await _groupRepository.AddGroup(group);
            await _groupRepository.SaveChangesAsync();

            // NAUDOJAM GAMYKLOS METODĄ GRUPĖS NARIAMS SUKURTI
            var groupMembers = GroupFactory.CreateGroupMembers(group.GroupID, request.GroupMemberIDs);
            await _groupRepository.AddGroupMembers(groupMembers);
            await _groupRepository.SaveChangesAsync();

            return group.GroupID;
        }
    }
}

