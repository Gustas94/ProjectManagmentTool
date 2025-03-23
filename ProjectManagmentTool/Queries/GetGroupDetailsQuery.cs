using MediatR;
using ProjectManagmentTool.DTOs;

namespace ProjectManagmentTool.Queries
{
    public class GetGroupDetailsQuery : IRequest<GroupDetailsDTO>
    {
        public int GroupId { get; set; }

        public GetGroupDetailsQuery(int groupId)
        {
            GroupId = groupId;
        }
    }
}