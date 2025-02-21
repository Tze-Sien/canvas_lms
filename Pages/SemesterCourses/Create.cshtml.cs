using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.SemesterCourses
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
            PopulateDropDowns();
            Semesters = _context.Semesters.ToList();
            Courses = _context.Courses.ToList();
            return Page();
        }

        public List<Semester> Semesters { get; set; }
        public List<Course> Courses { get; set; }

        [BindProperty]
        public SemesterCourse SemesterCourse { get; set; } = default!;

        private void PopulateDropDowns()
        {
            ViewData["CourseId"] = new SelectList(_context.Courses, "Id", "Name");
            ViewData["SemesterId"] = new SelectList(_context.Semesters, "Id", "Name");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns();
                return Page();
            }


            // Additional validation
            if (SemesterCourse.SemesterId == Guid.Empty)
            {
                ModelState.AddModelError("SemesterCourse.SemesterId", "Please select a semester");
                PopulateDropDowns();
                return Page();
            }

            if (SemesterCourse.CourseId == Guid.Empty)
            {
                ModelState.AddModelError("SemesterCourse.CourseId", "Please select a course");
                PopulateDropDowns();
                return Page();
            }

            _context.SemesterCourses.Add(SemesterCourse);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
