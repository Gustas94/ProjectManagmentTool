using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectManagmentTool.Data;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ProjectManagmentTool.Controllers
{
    [Route("api/invitations")]
    [ApiController]
    public class InvitationController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public InvitationController(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ✅ Generate an invitation link
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateInvitation()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.CompanyID == null)
                return Unauthorized("You must be part of a company to create invitations.");

            var invitation = new Invitation
            {
                InvitationLink = Guid.NewGuid().ToString(),
                CompanyID = user.CompanyID.Value,
                Email = null,  // ✅ No email required
                RoleID = null,  // ✅ Default role will be assigned
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            _context.Invitations.Add(invitation);
            await _context.SaveChangesAsync();

            return Ok(new { link = $"http://localhost:5173/register?invite={invitation.InvitationLink}" });
        }

        // ✅ Accept an invitation and register the user
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
        {
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.InvitationLink == request.InviteCode);

            if (invitation == null || invitation.ExpiresAt < DateTime.UtcNow)
                return BadRequest("Invalid or expired invitation.");

            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
                return BadRequest("Email is already registered.");

            // ✅ Default Role ID
            string defaultRoleId = "B134B12E-EFDA-48D4-B5F6-25CFFB4DD911";

            var newUser = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CompanyID = invitation.CompanyID, // ✅ Assign company from invitation
                RoleID = invitation.RoleID ?? defaultRoleId, // ✅ Default to USER role
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            _context.Invitations.Remove(invitation); // ✅ Delete the invitation after use
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful! You are now part of the company." });
        }

        // ✅ Get details of an invitation (before user registers)
        [HttpGet("{inviteCode}")]
        public async Task<IActionResult> GetInvitation(string inviteCode)
        {
            var invitation = await _context.Invitations
                .Where(i => i.InvitationLink == inviteCode && i.ExpiresAt > DateTime.UtcNow)
                .Select(i => new { i.CompanyID })
                .FirstOrDefaultAsync();

            if (invitation == null)
            {
                return NotFound("Invalid or expired invitation link.");
            }

            return Ok(invitation);
        }
    }

    public class AcceptInvitationRequest
    {
        public string InviteCode { get; set; }
        public string Email { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Password { get; set; }
    }
}
