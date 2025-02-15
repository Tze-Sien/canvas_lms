using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.SemesterCourses
{
    public class DetailsModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public DetailsModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public SemesterCourse SemesterCourse { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var semestercourse = await _context.SemesterCourses.FirstOrDefaultAsync(m => m.Id == id);

            if (semestercourse is not null)
            {
                SemesterCourse = semestercourse;

                return Page();
            }

            return NotFound();
        }
    }
}
