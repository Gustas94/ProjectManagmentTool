namespace ProjectManagmentTool.Security
{
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using ProjectManagmentTool.Data;
    using System.Linq;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;

        public PermissionHandler(ApplicationDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return;

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || user.RoleID == null) return;

            var hasPermission = _context.RolePermissions
                .Where(rp => rp.RoleID == user.RoleID)
                .Join(_context.Permissions,
                    rp => rp.PermissionID,
                    p => p.PermissionID,
                    (rp, p) => p.PermissionName)
                .Any(name => name == requirement.PermissionName);

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
        }
    }
}
