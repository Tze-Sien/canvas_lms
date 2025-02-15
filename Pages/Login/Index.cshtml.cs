using CanvasLMS.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CanvasLMS.Pages.Login
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public required string Email { get; set; }

        [BindProperty]
        public required string Password { get; set; }

        [BindProperty(SupportsGet = true)] // Enables binding ReturnUrl from query params
        public string? ReturnUrl { get; set; }

        public required string ErrorMessage { get; set; }

        private readonly Services.ApplicationDBContext _context;

        public LoginModel(Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Find user by email
            User? user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);

            // Validate credentials
            if (user != null && BCrypt.Net.BCrypt.Verify(Password, user.Password))
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, Email),
                    new Claim(ClaimTypes.Name, user.Name) 
                };

                var identity = new ClaimsIdentity(claims, "CookieAuth");
                var principal = new ClaimsPrincipal(identity);

                await HttpContext.SignInAsync("CookieAuth", principal);

                // If ReturnUrl is provided and safe, redirect there; otherwise, go to Index
                if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                {
                    return LocalRedirect(ReturnUrl);
                }
                return RedirectToPage("/Index");
            }

            ErrorMessage = "Invalid username or password.";
            return Page();
        }
    }
}
