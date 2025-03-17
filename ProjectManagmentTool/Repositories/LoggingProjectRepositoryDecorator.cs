namespace ProjectManagmentTool.Repositories
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class LoggingProjectRepositoryDecorator : IProjectRepository
    {
        private readonly IProjectRepository _innerRepository;

        public LoggingProjectRepositoryDecorator(IProjectRepository innerRepository)
        {
            _innerRepository = innerRepository;
        }

        public async Task<List<Project>> GetAllProjectsByCompanyId(int companyId)
        {
            Console.WriteLine($"Fetching projects for Company ID: {companyId}");
            return await _innerRepository.GetAllProjectsByCompanyId(companyId);
        }

        public async Task<Project?> GetProjectById(int projectId, int companyId)
        {
            Console.WriteLine($"Fetching project with ID: {projectId} for Company ID: {companyId}");
            return await _innerRepository.GetProjectById(projectId, companyId);
        }

        public async Task AddProject(Project project)
        {
            Console.WriteLine($"Adding new project: {project.ProjectName}");
            await _innerRepository.AddProject(project);
        }

        public async Task SaveChangesAsync()
        {
            Console.WriteLine("Saving changes to database...");
            await _innerRepository.SaveChangesAsync();
        }
    }
}
