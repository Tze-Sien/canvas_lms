using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.CourseEnrollments
{
    public class EditModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public EditModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CourseEnrollment CourseEnrollment { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseenrollment =  await _context.CourseEnrollments.FirstOrDefaultAsync(m => m.Id == id);
            if (courseenrollment == null)
            {
                return NotFound();
            }
            CourseEnrollment = courseenrollment;
           ViewData["SemesterCourseId"] = new SelectList(_context.SemesterCourses, "Id", "Id");
           ViewData["StudentId"] = new SelectList(_context.Students, "Id", "Id");
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(CourseEnrollment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseEnrollmentExists(CourseEnrollment.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool CourseEnrollmentExists(Guid id)
        {
            return _context.CourseEnrollments.Any(e => e.Id == id);
        }
    }
}
