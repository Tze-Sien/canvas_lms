using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;
using System.Security.Claims;

namespace CanvasLMS.Pages.StudentTranscripts
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public DetailsModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public StudentTranscript StudentTranscript { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studenttranscript = await _context.StudentTranscripts
                .Include(s => s.Student)
                    .ThenInclude(s => s.User)
                .Include(s => s.CourseEnrollment)
                    .ThenInclude(ce => ce.SemesterCourse)
                        .ThenInclude(sc => sc.Course)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (studenttranscript == null)
            {
                return NotFound();
            }

            StudentTranscript = studenttranscript;
            return Page();
        }
    }
}
