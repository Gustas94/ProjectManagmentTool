namespace ProjectManagmentTool.Queries
{
    using MediatR;
    using ProjectManagmentTool.DTOs;

    public record GetGroupByIdQuery(int GroupId) : IRequest<GroupDetailsDTO>;
}
