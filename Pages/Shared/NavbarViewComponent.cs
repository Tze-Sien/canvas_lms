using Microsoft.AspNetCore.Mvc;
using CanvasLMS.Models;
using CanvasLMS.Services;
using System.Security.Claims;

namespace CanvasLMS.Pages.Shared
{
    public class NavbarViewComponent : ViewComponent
    {
        private readonly ApplicationDBContext _context;

        public NavbarViewComponent(ApplicationDBContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            var isAuthenticated = User?.Identity?.IsAuthenticated == true;
            string? userRole = null;

            if (isAuthenticated && User is ClaimsPrincipal claimsPrincipal)
            {
                var userId = Guid.Parse(claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
                if (userId != Guid.Empty)
                {
                    userRole = _context.Users.Find(userId)?.Role.ToString();
                }
            }

            return View(new NavbarViewModel 
            { 
                IsAuthenticated = isAuthenticated,
                UserRole = userRole,
                UserName = User?.Identity?.Name
            });
        }
    }

    public class NavbarViewModel
    {
        public bool IsAuthenticated { get; set; }
        public string? UserRole { get; set; }
        public string? UserName { get; set; }
    }
}