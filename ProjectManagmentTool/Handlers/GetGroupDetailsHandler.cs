namespace ProjectManagmentTool.Handlers
{
    using MediatR;
    using ProjectManagmentTool.Queries;
    using ProjectManagmentTool.Repositories;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetGroupDetailsHandler : IRequestHandler<GetGroupDetailsQuery, Group>
    {
        private readonly IGroupRepository _groupRepository;

        public GetGroupDetailsHandler(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public async Task<Group> Handle(GetGroupDetailsQuery request, CancellationToken cancellationToken)
        {
            return await _groupRepository.GetGroupById(request.GroupId);
        }
    }

}
