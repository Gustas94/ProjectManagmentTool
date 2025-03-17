namespace ProjectManagmentTool.Handlers
{
    using MediatR;
    using ProjectManagmentTool.Queries;
    using ProjectManagmentTool.Repositories;
    using System.Threading;
    using System.Threading.Tasks;

    public class GetCompanyIdHandler : IRequestHandler<GetCompanyIdQuery, int?>
    {
        private readonly IGroupRepository _groupRepository;

        public GetCompanyIdHandler(IGroupRepository groupRepository)
        {
            _groupRepository = groupRepository;
        }

        public async Task<int?> Handle(GetCompanyIdQuery request, CancellationToken cancellationToken)
        {
            return await _groupRepository.GetCompanyIdByUserId(request.UserId);
        }
    }
}
