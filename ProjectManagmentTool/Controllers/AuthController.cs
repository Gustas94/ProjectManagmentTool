using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ProjectManagmentTool.Data;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context; // ✅ Injected

        public AuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            RoleManager<Role> roleManager,
            IConfiguration configuration,
            ApplicationDbContext context) // ✅ Injected ApplicationDbContext
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _configuration = configuration;
            _context = context;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Email and password are required." });

            if (await _userManager.FindByEmailAsync(request.Email) != null)
                return BadRequest(new { message = "Email is already taken." });

            // If InviteCode exists, handle invited user registration
            if (!string.IsNullOrWhiteSpace(request.InviteCode))
            {
                var invitation = await _context.Invitations.FirstOrDefaultAsync(i => i.InviteCode == request.InviteCode);
                if (invitation == null)
                    return BadRequest(new { message = "Invalid or expired invite link." });

                // Get the role assigned in the invitation
                var userRole = await _roleManager.FindByIdAsync(invitation.RoleID.ToString());
                if (userRole == null)
                    return StatusCode(500, "Invalid role assigned in invitation.");

                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    RoleID = userRole.Id,
                    CompanyID = invitation.CompanyID, // ✅ Assign to invited company
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

                await _userManager.AddToRoleAsync(user, userRole.Name);
                return Ok(new { message = "User registered successfully via invite." });
            }
            else
            {
                // ✅ Default registration (No invite, assign CEO role)
                var userRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "CEO");
                if (userRole == null)
                    return StatusCode(500, "CEO role does not exist. Please create it first.");

                var user = new User
                {
                    UserName = request.Email,
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    RoleID = userRole.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

                await _userManager.AddToRoleAsync(user, "CEO");
                return Ok(new { message = "User registered successfully and assigned as CEO." });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return Unauthorized("Invalid credentials.");

            var result = await _signInManager.PasswordSignInAsync(user, request.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized("Invalid credentials.");

            // Generate JWT Token
            var token = GenerateJwtToken(user);
            return Ok(new { token });
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

    public class RegisterRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? InviteCode { get; set; } // ✅ Fixed missing InviteCode
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
