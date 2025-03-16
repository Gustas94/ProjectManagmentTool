using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.CompanyName) ||
                string.IsNullOrWhiteSpace(request.Industry) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "All required fields must be filled." });
            }

            // Check if the company name already exists
            if (await _context.Companies.AnyAsync(c => c.CompanyName == request.CompanyName))
            {
                return BadRequest(new { message = "A company with this name already exists." });
            }

            // Create CEO User
            var ceo = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(ceo, request.Password);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            // Assign CEO Role
            var ceoRole = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "CEO");
            if (ceoRole == null)
            {
                return BadRequest(new { message = "CEO role is missing from the database." });
            }

            ceo.RoleID = ceoRole.Id;
            await _context.SaveChangesAsync();

            // Create Company
            var company = new Company
            {
                CompanyName = request.CompanyName,
                Industry = request.Industry,
                CEOID = ceo.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Company created successfully", companyID = company.CompanyID });
        }
    }

    public class CreateCompanyRequest
    {
        public string Name { get; set; }
        public string Industry { get; set; }
        public string CEOId { get; set; }  // Keep CEOId, but pass UserId from frontend
    }
}

public class CreateCompanyDTO
{
    public string CompanyName { get; set; }
    public string Industry { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
}
