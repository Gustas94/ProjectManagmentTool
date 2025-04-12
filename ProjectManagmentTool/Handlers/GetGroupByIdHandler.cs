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
                .Include(g => g.Tasks) // Direct one-to-many
                .Include(g => g.TaskGroups) // Join table
                    .ThenInclude(tg => tg.Task) // Tasks from many-to-many
                .Include(g => g.GroupLead)
                .Include(g => g.ProjectGroups)
                    .ThenInclude(pg => pg.Project)
                .FirstOrDefaultAsync(g => g.GroupID == request.GroupId);

            if (group == null) return null;

            // Merge tasks from both sources, avoiding duplicates
            var allTasks = group.Tasks.ToList();

            foreach (var tg in group.TaskGroups)
            {
                if (tg.Task != null && !allTasks.Any(t => t.TaskID == tg.Task.TaskID))
                {
                    allTasks.Add(tg.Task);
                }
            }

            return new GroupDetailsDTO
            {
                GroupID = group.GroupID,
                GroupName = group.GroupName,
                Description = group.Description,
                GroupLeadName = $"{group.GroupLead.FirstName} {group.GroupLead.LastName}",
                ProjectAffiliation = group.ProjectGroups.Select(pg => pg.Project.ProjectName).ToList(),
                CurrentProgress = group.CurrentProgress ?? "N/A",
                Status = group.Status ?? "Active",
                Tasks = allTasks.Select(t => new TaskDTO
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
