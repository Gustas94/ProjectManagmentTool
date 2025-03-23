using MediatR;
using System.Collections.Generic;

namespace ProjectManagmentTool.Commands
{
    public record AssignGroupMembersCommand(int GroupId, List<string> MemberIDs) : IRequest<Unit>;
}
