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

namespace CanvasLMS.Pages.CourseReviews
{
    public class IndexModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public IndexModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public IList<CourseReview> CourseReview { get;set; } = default!;
        public Student? CurrentStudent { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current user ID from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToPage("/Login/Index");
            }

            // Get student record
            CurrentStudent = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == Guid.Parse(userId));

            if (CurrentStudent == null)
            {
                return RedirectToPage("/Index");
            }

            // Get reviews for current student
            CourseReview = await _context.CourseReviews
                .Include(c => c.CourseEnrollment)
                    .ThenInclude(ce => ce!.SemesterCourse)
                    .ThenInclude(sc => sc!.Course)
                .Include(c => c.Student)
                    .ThenInclude(s => s!.User)
                .Where(c => c.StudentId == CurrentStudent.Id)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return Page();
        }
    }
}
