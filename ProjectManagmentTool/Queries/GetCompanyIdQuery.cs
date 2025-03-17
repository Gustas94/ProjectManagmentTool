namespace ProjectManagmentTool.Queries
{
    using MediatR;

    public record GetCompanyIdQuery(string UserId) : IRequest<int?>;

}
