using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Data;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IConfiguration _configuration;

        public CompanyController(ApplicationDbContext context, UserManager<User> userManager, IConfiguration configuration)
        {
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyDTO request)
        {
            if (string.IsNullOrWhiteSpace(request.CompanyName) ||
                request.IndustryId <= 0 || // Validate that a valid industry is selected
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.FirstName) ||
                string.IsNullOrWhiteSpace(request.LastName) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "All required fields must be filled, and a valid industry must be selected." });
            }

            // Check if the company name already exists
            if (await _context.Companies.AnyAsync(c => c.CompanyName == request.CompanyName))
            {
                return BadRequest(new { message = "A company with this name already exists." });
            }

            // Fetch the CEO Role before creating the user
            var ceoRole = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "CEO");
            if (ceoRole == null)
            {
                return BadRequest(new { message = "CEO role is missing from the database." });
            }

            // Create CEO User and assign RoleID before creation
            var ceo = new User
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                UserName = request.Email,
                RoleID = ceoRole.Id, // Assign role to avoid null value during insert
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            var result = await _userManager.CreateAsync(ceo, request.Password);
            if (!result.Succeeded)
            {
                var errorMessages = result.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new { errors = errorMessages });
            }

            var roleAssignResult = await _userManager.AddToRoleAsync(ceo, "CEO");
            if (!roleAssignResult.Succeeded)
            {
                var roleErrors = roleAssignResult.Errors.Select(e => e.Description).ToArray();
                return BadRequest(new { errors = roleErrors });
            }

            // Create Company using IndustryId instead of a free-text Industry
            var company = new Company
            {
                CompanyName = request.CompanyName,
                IndustryId = request.IndustryId,  // Updated to use IndustryId
                CEOID = ceo.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Companies.Add(company);
            await _context.SaveChangesAsync();

            ceo.CompanyID = company.CompanyID;
            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(ceo);

            return Ok(new { message = "Company created successfully", companyID = company.CompanyID, token });
        }


        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, $"{user.FirstName} {user.LastName}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
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
    public int IndustryId { get; set; }
    public string Email { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
}
