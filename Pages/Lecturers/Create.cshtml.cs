using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CanvasLMS.Models;
using CanvasLMS.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace CanvasLMS.Pages.Lecturers
{
    public class CreateModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public CreateModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        private void PopulateDropDowns()
        {
            ViewData["FacultyId"] = new SelectList(_context.Faculties, "Id", "Name");
            var availableUsers = _context.Users.Where(u => u.Role == null).Select(u => new { u.Id, u.Email });
            ViewData["UserId"] = new SelectList(availableUsers, "Id", "Email");
        }

        public IActionResult OnGet()
        {
            PopulateDropDowns();
            return Page();
        }

        [BindProperty]
        public Lecturer Lecturer { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns();
                return Page();
            }
            // Find and update the selected user's role
            var user = await _context.Users.FindAsync(Lecturer.UserId);
            if (user != null)
            {
                user.Role = Role.Lecturer;
                _context.Lecturers.Add(Lecturer);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }

            // If user not found, return to page with error
            ModelState.AddModelError("", "Selected user not found");
            PopulateDropDowns();
            return Page();
        }
    }
}
