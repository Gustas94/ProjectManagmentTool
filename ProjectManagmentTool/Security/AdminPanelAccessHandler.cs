namespace ProjectManagmentTool.Security
{
    using Microsoft.AspNetCore.Authorization;
    using System.Security.Claims;
    using System.Threading.Tasks;

    public class AdminPanelAccessHandler : AuthorizationHandler<AdminPanelAccessRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, AdminPanelAccessRequirement requirement)
        {
            var user = context.User;

            // Check if user has permission claim
            if (user.HasClaim("Permission", "ADMIN_PANEL_ACCESS"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            // Check if user is in CEO role
            if (user.IsInRole("CEO"))
            {
                context.Succeed(requirement);
                return Task.CompletedTask;
            }

            return Task.CompletedTask;
        }
    }
}
