using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.CourseReviews
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public DeleteModel(ApplicationDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CourseReview CourseReview { get; set; } = default!;
        public Course Course { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var currentUserId = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToPage("/Login/Index");
            }

            // Get active semesters
            var activeSemesters = await _context.Semesters
                .Where(s => s.Status == SemesterStatus.Ongoing)
                .ToListAsync();

            if (!activeSemesters.Any())
            {
                return RedirectToPage("./Index");
            }

            var semesterIds = activeSemesters.Select(s => s.Id).ToList();

            // Get the review with all required navigation properties
            var review = await _context.CourseReviews
                .Include(cr => cr.CourseEnrollment!)
                    .ThenInclude(ce => ce.SemesterCourse!)
                        .ThenInclude(sc => sc.Course)
                .Include(cr => cr.Student!)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            // Verify this review is for a course in an active semester
            if (!semesterIds.Contains(review!.CourseEnrollment!.SemesterCourse!.SemesterId))
            {
                return RedirectToPage("./Index");
            }

            CourseReview = review;
            Course = review.CourseEnrollment.SemesterCourse.Course!;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                // return NotFound();
            }

            var currentUserId = User?.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserId))
            {
                return RedirectToPage("/Login/Index");
            }

            // Get active semesters
            var activeSemesters = await _context.Semesters
                .Where(s => s.Status == SemesterStatus.Ongoing)
                .ToListAsync();

            if (!activeSemesters.Any())
            {
                return RedirectToPage("./Index");
            }

            var semesterIds = activeSemesters.Select(s => s.Id).ToList();

            // Get the review with required navigation properties
            var review = await _context.CourseReviews
                .Include(cr => cr.CourseEnrollment!)
                    .ThenInclude(ce => ce.SemesterCourse!)
                .Include(cr => cr.Student!)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(cr => cr.Id == id);

            // Verify active semester
            if (!semesterIds.Contains(review!.CourseEnrollment!.SemesterCourse!.SemesterId))
            {
                return RedirectToPage("./Index");
            }

            var courseId = review.CourseEnrollment.SemesterCourse.CourseId;
            _context.CourseReviews.Remove(review);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { selectedCourseId = courseId });
        }
    }
}
