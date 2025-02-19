using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.Semesters
{
    public class CreateModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public CreateModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public SelectList FacultyList { get; set; } = default!;

        [BindProperty]
        public Semester Semester { get; set; } = default!;

        public IActionResult OnGet()
        {
            FacultyList = new SelectList(_context.Faculties, "Id", "Name");
            Semester = new Semester { Name = "" };
            return Page();
        }

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (Semester.FacultyId == Guid.Empty)
            {
                ModelState.AddModelError("Semester.FacultyId", "Faculty is required");
            }

            if (!ModelState.IsValid)
            {
                FacultyList = new SelectList(_context.Faculties, "Id", "Name");
                return Page();
            }

            var faculty = await _context.Faculties.FindAsync(Semester.FacultyId);
            if (faculty == null)
            {
                ModelState.AddModelError("Semester.FacultyId", "Selected faculty not found");
                FacultyList = new SelectList(_context.Faculties, "Id", "Name");
                return Page();
            }

            try
            {
                Semester.Faculty = faculty;
                _context.Semesters.Add(Semester);
                await _context.SaveChangesAsync();
                return RedirectToPage("./Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, "Error saving semester: " + ex.Message);
                FacultyList = new SelectList(_context.Faculties, "Id", "Name");
                return Page();
            }
        }
    }
}
