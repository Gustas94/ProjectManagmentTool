namespace ProjectManagmentTool.Handlers
{
    using MediatR;
    using Microsoft.EntityFrameworkCore;
    using ProjectManagmentTool.Data;
    using ProjectManagmentTool.DTOs;
    using ProjectManagmentTool.Queries;
    using System;

    public class GetGroupByIdHandler : IRequestHandler<GetGroupByIdQuery, GroupDetailsDTO>
    {
        private readonly ApplicationDbContext _context;

        public GetGroupByIdHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<GroupDetailsDTO> Handle(GetGroupByIdQuery request, CancellationToken cancellationToken)
        {
            var group = await _context.Groups
                .Include(g => g.Tasks)
                .Include(g => g.GroupLead)
                .Include(g => g.ProjectGroups)
                    .ThenInclude(pg => pg.Project)
                .FirstOrDefaultAsync(g => g.GroupID == request.GroupId);

            if (group == null) return null;

            return new GroupDetailsDTO
            {
                GroupID = group.GroupID,
                GroupName = group.GroupName,
                Description = group.Description,
                GroupLeadName = $"{group.GroupLead.FirstName} {group.GroupLead.LastName}",
                ProjectAffiliation = group.ProjectGroups.Select(pg => pg.Project.ProjectName).ToList(),
                CurrentProgress = "0%", // Placeholder or real logic if you have it
                Status = "Active", // Placeholder
                Tasks = group.Tasks.Select(t => new TaskDTO
                {
                    TaskID = t.TaskID,
                    TaskName = t.TaskName,
                    Description = t.Description,
                    Deadline = t.Deadline.ToString("o"),
                    Priority = t.Priority,
                    Status = t.Status
                }).ToList()
            };
        }
    }
}
