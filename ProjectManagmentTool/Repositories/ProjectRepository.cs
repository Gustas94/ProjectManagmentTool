namespace ProjectManagmentTool.Repositories
{
    using Microsoft.EntityFrameworkCore;
    using ProjectManagmentTool.Data;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllProjectsByCompanyId(int companyId)
        {
            return await _context.Projects.Where(p => p.CompanyID == companyId).ToListAsync();
        }

        public async Task<Project?> GetProjectById(int projectId, int companyId)
        {
            return await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectID == projectId && p.CompanyID == companyId);
        }

        public async Task AddProject(Project project)
        {
            _context.Projects.Add(project);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
