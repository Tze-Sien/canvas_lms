using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.SemesterStudents
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
        ViewData["SemesterId"] = new SelectList(_context.Semesters, "Id", "Name");
        ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Id");
            return Page();
        }

        [BindProperty]
        public SemesterStudent SemesterStudent { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.SemesterStudents.Add(SemesterStudent);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
