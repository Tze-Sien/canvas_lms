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
    public class DetailsModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public DetailsModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

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
    }
}
