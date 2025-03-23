using MediatR;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Data;
using ProjectManagmentTool.DTOs;
using ProjectManagmentTool.Queries;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ProjectManagmentTool.Handlers
{
    public class GetGroupDetailsHandler : IRequestHandler<GetGroupDetailsQuery, GroupDetailsDTO>
    {
        private readonly ApplicationDbContext _context;

        public GetGroupDetailsHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GroupDetailsDTO> Handle(GetGroupDetailsQuery request, CancellationToken cancellationToken)
        {
            var group = await _context.Groups
                .Include(g => g.GroupLead)
                .Include(g => g.Tasks)
                .Include(g => g.ProjectGroups)
                    .ThenInclude(pg => pg.Project)
                .FirstOrDefaultAsync(g => g.GroupID == request.GroupId, cancellationToken);

            if (group == null)
                return null;

            var members = await _context.GroupMembers
                .Where(gm => gm.GroupID == request.GroupId)
                .Include(gm => gm.User)
                .Select(gm => new UserDTO
                {
                    Id = gm.User.Id,
                    FirstName = gm.User.FirstName,
                    LastName = gm.User.LastName
                })
                .ToListAsync(cancellationToken);

            return new GroupDetailsDTO
            {
                GroupID = group.GroupID,
                GroupName = group.GroupName,
                Description = group.Description,
                GroupLeadName = group.GroupLead != null ? $"{group.GroupLead.FirstName} {group.GroupLead.LastName}" : "N/A",
                ProjectAffiliation = group.ProjectGroups.Select(pg => pg.Project.ProjectName).ToList(),
                CurrentProgress = group.CurrentProgress,
                Status = group.Status,
                Tasks = group.Tasks.Select(t => new TaskDTO
                {
                    TaskID = t.TaskID,
                    TaskName = t.TaskName,
                    Description = t.Description,
                    Deadline = t.Deadline.ToString("yyyy-MM-dd"), // fix DateTime error
                    Priority = t.Priority,
                    Status = t.Status
                }).ToList(),
                Members = members
            };
        }
    }
}
