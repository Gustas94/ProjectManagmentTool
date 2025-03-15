using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Data;
using System.Threading.Tasks;

namespace ProjectManagmentTool.Services
{
    public class TaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task DeleteTaskAsync(int taskId)
        {
            var task = await _context.Tasks
                .Include(t => t.TaskGroups) // Include TaskGroups to ensure they are loaded
                .FirstOrDefaultAsync(t => t.TaskID == taskId);

            if (task != null)
            {
                // Remove all TaskGroup associations
                _context.TaskGroups.RemoveRange(task.TaskGroups);

                // Remove the task itself
                _context.Tasks.Remove(task);

                await _context.SaveChangesAsync();
            }
        }
    }
}