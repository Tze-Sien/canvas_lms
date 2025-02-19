using Microsoft.AspNetCore.Mvc;

namespace CanvasLMS.Pages.Shared
{
    public class BreadcrumbViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var path = HttpContext.Request.Path.Value?.TrimStart('/');
            
            // Don't show breadcrumb on home page and login page
            if (string.IsNullOrEmpty(path) || path.StartsWith("Login", StringComparison.OrdinalIgnoreCase))
            {
                return Content(string.Empty);
            }

            var breadcrumbs = new List<(string Text, string Link)>();
            var text = path.Split('/')[0];
            // Convert to title case and add spaces
            text = System.Text.RegularExpressions.Regex.Replace(text, "([a-z])([A-Z])", "$1 $2");
            breadcrumbs.Add((text, null));

            return View(breadcrumbs);
        }
    }
}
