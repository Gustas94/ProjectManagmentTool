namespace ProjectManagmentTool.Commands
{
    using MediatR;
    using System.Collections.Generic;

    public record CreateGroupCommand(
        string GroupName,
        string Description,
        string GroupLeadID,
        int? CompanyID,
        List<string> GroupMemberIDs
    ) : IRequest<int>;
}
