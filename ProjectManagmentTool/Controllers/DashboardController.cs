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
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Ensure only authenticated users can access this endpoint
    public class DashboardController : ControllerBase
    {
        private readonly UserManager<User> _userManager;

        public DashboardController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // Get logged-in user's ID

            if (userId == null)
                return Unauthorized("User is not authenticated.");

            var user = await _userManager.Users
                .Include(u => u.Role) // Ensure Role is loaded
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                return NotFound("User not found.");

            return Ok(new
            {
                firstName = user.FirstName,
                lastName = user.LastName,
                role = user.Role != null ? user.Role.Name : "Unknown",
                companyID = user.CompanyID
            });
        }
    }
}
