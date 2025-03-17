namespace ProjectManagmentTool.Queries
{
    using MediatR;
    using System.Collections.Generic;

    public record GetAllGroupsQuery(int? CompanyID) : IRequest<List<Group>>;
}
