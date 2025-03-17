namespace ProjectManagmentTool.Repositories
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IProjectRepository
    {
        Task<List<Project>> GetAllProjectsByCompanyId(int companyId);
        Task<Project?> GetProjectById(int projectId, int companyId);
        Task AddProject(Project project);
        Task SaveChangesAsync();
    }
}
