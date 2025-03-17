namespace ProjectManagmentTool.Queries
{
    using MediatR;

    public record GetGroupDetailsQuery(int GroupId) : IRequest<Group>;

}
