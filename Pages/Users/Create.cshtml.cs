using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.Users
{
    public class CreateModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public CreateModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public User User { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }


             User.Password = BCrypt.Net.BCrypt.HashPassword(User.Password);
            _context.Users.Add(User);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
