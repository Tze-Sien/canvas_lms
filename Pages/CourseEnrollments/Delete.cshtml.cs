using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.CourseEnrollments
{
    public class DeleteModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public DeleteModel(CanvasLMS.Services.ApplicationDBContext context)
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

            var courseenrollment = await _context.CourseEnrollments.FirstOrDefaultAsync(m => m.Id == id);

            if (courseenrollment is not null)
            {
                CourseEnrollment = courseenrollment;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseenrollment = await _context.CourseEnrollments.FindAsync(id);
            if (courseenrollment != null)
            {
                CourseEnrollment = courseenrollment;
                _context.CourseEnrollments.Remove(CourseEnrollment);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
