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

        // ✅ Generate an invitation
        [HttpPost("create")]
        [Authorize]
        public async Task<IActionResult> CreateInvitation([FromBody] CreateInvitationRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.CompanyID == null)
                return Unauthorized("You must be part of a company to create invitations.");

            var invite = new Invitation
            {
                InviteCode = Guid.NewGuid().ToString(), // ✅ Unique code
                CompanyID = user.CompanyID.Value,
                Email = request.Email,
                RoleID = request.RoleID,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7) // ✅ Expiry in 7 days
            };

            _context.Invitations.Add(invite);
            await _context.SaveChangesAsync();

            return Ok(new { invite.InviteCode, invite.InvitationLink });
        }

        // ✅ Accept an invitation and register the user
        [HttpPost("accept")]
        public async Task<IActionResult> AcceptInvitation([FromBody] AcceptInvitationRequest request)
        {
            var invitation = await _context.Invitations
                .FirstOrDefaultAsync(i => i.InviteCode == request.InviteCode);

            if (invitation == null || invitation.ExpiresAt < DateTime.UtcNow)
                return BadRequest(new { message = "Invalid or expired invitation." });

            var userExists = await _userManager.FindByEmailAsync(request.Email);
            if (userExists != null)
                return BadRequest(new { message = "Email is already registered." });

            string defaultRoleId = "B134B12E-EFDA-48D4-B5F6-25CFFB4DD911"; // ✅ Default role

            var newUser = new User
            {
                UserName = request.Email,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                CompanyID = invitation.CompanyID, // ✅ Assign company from invite
                RoleID = invitation.RoleID ?? defaultRoleId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(newUser, request.Password);
            if (!result.Succeeded)
                return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

            _context.Invitations.Remove(invitation); // ✅ Delete invitation after use
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful! You are now part of the company." });
        }

        // ✅ Validate an invitation before user registers
        [HttpGet("validate")]
        public async Task<IActionResult> ValidateInvitation([FromQuery] string inviteCode)
        {
            var invitation = await _context.Invitations
                .Where(i => i.InviteCode == inviteCode && i.ExpiresAt > DateTime.UtcNow)
                .Select(i => new { i.CompanyID })
                .FirstOrDefaultAsync();

            if (invitation == null)
                return BadRequest(new { message = "Invalid or expired invitation." });

            return Ok(invitation);
        }
    }

    public class CreateInvitationRequest
    {
        public string? Email { get; set; }
        public string? RoleID { get; set; }
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
