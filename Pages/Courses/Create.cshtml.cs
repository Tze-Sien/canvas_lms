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

namespace CanvasLMS.Pages.Courses
{
    public class CreateModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public CreateModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        private void LoadDropDowns()
        {
            ViewData["FacultyId"] = new SelectList(_context.Faculties.AsNoTracking(), "Id", "Name");
            ViewData["LecturerId"] = new SelectList(
                _context.Lecturers
                    .Include(l => l.User)
                    .Include(l => l.Faculty)
                    .AsNoTracking(),
                "Id", 
                "User.Name"
            );
        }

        public IActionResult OnGet()
        {
            LoadDropDowns();
            return Page();
        }

        [BindProperty]
        public Course Course { get; set; } = default!;

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                LoadDropDowns();
                return Page();
            }

            // Verify Faculty exists
            var faculty = await _context.Faculties.FindAsync(Course.FacultyId);
            if (faculty == null)
            {
                ModelState.AddModelError("Course.FacultyId", "Selected faculty not found");
                LoadDropDowns();
                return Page();
            }

            // Verify Lecturer exists if selected
            if (Course.LecturerId.HasValue)
            {
                var lecturer = await _context.Lecturers
                    .Include(l => l.Faculty)
                    .FirstOrDefaultAsync(l => l.Id == Course.LecturerId);
                
                if (lecturer == null)
                {
                    ModelState.AddModelError("Course.LecturerId", "Selected lecturer not found");
                    LoadDropDowns();
                    return Page();
                }

                // Verify lecturer belongs to the selected faculty
                if (lecturer.FacultyId != Course.FacultyId)
                {
                    ModelState.AddModelError("Course.LecturerId", "Lecturer must belong to the selected faculty");
                    LoadDropDowns();
                    return Page();
                }
            }

            _context.Courses.Add(Course);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
