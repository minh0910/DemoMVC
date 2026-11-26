using DemoMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace DemoMVC.Services
{
    public sealed class DynamicAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly ApplicationDbContext _context;

        public DynamicAuthorizationFilter(ApplicationDbContext context) => _context = context;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor == null)
                return;

            if (context.HttpContext.GetEndpoint()?.Metadata.GetMetadata<IAllowAnonymous>() != null
                || descriptor.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).Length > 0
                || descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), inherit: true).Length > 0)
                return;

            var principal = context.HttpContext.User;
            if (principal.Identity?.IsAuthenticated != true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            var userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                context.Result = new ForbidResult();
                return;
            }

            var currentRoleIds = _context.UserRoles.AsNoTracking()
                .Where(userRole => userRole.UserId == userId)
                .Select(userRole => userRole.RoleId);

            var isAdmin = await _context.Roles.AsNoTracking()
                .AnyAsync(role => role.Name == "Admin" && currentRoleIds.Contains(role.Id));
            if (isAdmin)
                return;

            var permissionId = await _context.Permissions.AsNoTracking()
                .Where(permission => permission.Controller == descriptor.ControllerName
                    && permission.Action == descriptor.ActionName)
                .Select(permission => (int?)permission.Id)
                .SingleOrDefaultAsync();

            if (permissionId == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var allowed = await _context.RolePermissions.AsNoTracking()
                .AnyAsync(rolePermission => rolePermission.PermissionId == permissionId.Value
                    && currentRoleIds.Contains(rolePermission.RoleId));

            if (!allowed)
                context.Result = new ForbidResult();
        }
    }
}
