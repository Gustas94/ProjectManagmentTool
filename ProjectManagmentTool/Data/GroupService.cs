using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Data;
using System.Threading.Tasks;

namespace ProjectManagmentTool.Services
{
    public class GroupService
    {
        private readonly ApplicationDbContext _context;

        public GroupService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DeleteGroupAsync(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.TaskGroups) // Include TaskGroups to ensure they are loaded
                .FirstOrDefaultAsync(g => g.GroupID == groupId);

            if (group != null)
            {
                // Remove all TaskGroup associations
                _context.TaskGroups.RemoveRange(group.TaskGroups);

                // Remove the group itself
                _context.Groups.Remove(group);

                await _context.SaveChangesAsync();
            }
        }
    }
}