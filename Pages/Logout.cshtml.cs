using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Authentication;

namespace CanvasLMS.Pages
{
    public class LogoutModel : PageModel
    {
        public async Task<IActionResult> OnPostAsync()
        {
            // Sign out the user and remove the authentication cookie
            await HttpContext.SignOutAsync("CookieAuth");

            // Redirect to the home page or login page
            return RedirectToPage("/Index");
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Sign out the user and remove the authentication cookie
            await HttpContext.SignOutAsync("CookieAuth");

            // Redirect to the home page or login page
            return RedirectToPage("/Login/Index");
        }
    }
}