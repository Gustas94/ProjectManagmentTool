namespace ProjectManagmentTool.Queries
{
    using MediatR;
    using ProjectManagmentTool.Repositories;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetAllGroupsHandler : IRequestHandler<GetAllGroupsQuery, List<Group>>
    {
        private readonly IGroupRepository _groupRepository;

        public GetAllGroupsHandler(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public async Task<List<Group>> Handle(GetAllGroupsQuery request, CancellationToken cancellationToken)
        {
            return await _groupRepository.GetAllGroupsByCompanyId(request.CompanyID);
        }
    }

}
