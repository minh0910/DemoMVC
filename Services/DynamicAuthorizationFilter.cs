using DemoMVC.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace DemoMVC.Services
{
    public class DynamicAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly ApplicationDbContext _context;

        public DynamicAuthorizationFilter(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var endpoint = context.HttpContext.GetEndpoint();
            if (endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null)
            {
                return;
            }

            var mvcDescriptor = context.ActionDescriptor as Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor;
            if (mvcDescriptor == null)
            {
                return;
            }

            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new ChallengeResult();
                return;
            }

            // Tài khoản Admin luôn có toàn quyền.
            if (user.IsInRole("Admin"))
            {
                return;
            }

            var controller = mvcDescriptor.ControllerName;
            var action = mvcDescriptor.ActionName;

            var permission = await _context.Permissions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Controller == controller && p.Action == action);

            if (permission == null)
            {
                context.Result = new ForbidResult();
                return;
            }

            var roleNames = user.Claims
                .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                .Select(c => c.Value)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (roleNames.Count == 0)
            {
                context.Result = new ForbidResult();
                return;
            }

            var roleIds = await _context.Roles
                .AsNoTracking()
                .Where(r => roleNames.Contains(r.Name!))
                .Select(r => r.Id)
                .ToListAsync();

            var allowed = await _context.RolePermissions
                .AsNoTracking()
                .AnyAsync(rp => rp.PermissionId == permission.Id && roleIds.Contains(rp.RoleId));

            if (!allowed)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
