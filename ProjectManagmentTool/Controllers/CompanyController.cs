using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectManagmentTool.Data;
using System;
using System.Threading.Tasks;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public CompanyController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateProject([FromBody] Project request)
        {
            if (string.IsNullOrEmpty(request.ProjectManagerID))
                return BadRequest("Project Manager ID is required.");

            if (request.CompanyID <= 0)
                return BadRequest("Company ID is required.");

            // Fetch Company
            var company = await _context.Companies.FindAsync(request.CompanyID);
            if (company == null)
                return BadRequest("Company not found.");

            // Fetch Project Manager
            var projectManager = await _context.Users.FindAsync(request.ProjectManagerID);
            if (projectManager == null)
                return BadRequest("Project Manager not found.");

            var project = new Project
            {
                ProjectName = request.ProjectName,
                Description = request.Description,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                CompanyID = request.CompanyID,
                ProjectManagerID = request.ProjectManagerID,
                Company = company,  // ✅ Assign Company object
                ProjectManager = projectManager,  // ✅ Assign ProjectManager object
                Visibility = request.Visibility,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Project created successfully", projectId = project.ProjectID });
        }
    }

    public class CreateCompanyRequest
    {
        public string Name { get; set; }
        public string Industry { get; set; }
        public string CEOId { get; set; }  // Keep CEOId, but pass UserId from frontend
    }
}
