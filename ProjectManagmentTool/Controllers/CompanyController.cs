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

        public CompanyController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request)
        {
            var company = new Company
            {
                CompanyName = request.Name,
                Industry = request.Industry,
                CEOID = request.CEOId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            // Update user's CompanyID after company creation
            var user = await _context.Users.FindAsync(request.CEOId);
            if (user != null)
            {
                user.CompanyID = company.CompanyID;  // Assign the created company ID
                await _context.SaveChangesAsync();
            }

            return Ok(new { message = "Company created successfully." });
        }
    }

    public class CreateCompanyRequest
    {
        public string Name { get; set; }
        public string Industry { get; set; }
        public string CEOId { get; set; }  // Keep CEOId, but pass UserId from frontend
    }
}
