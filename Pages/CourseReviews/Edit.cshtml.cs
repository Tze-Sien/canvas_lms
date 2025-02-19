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
    public class EditModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public EditModel(ApplicationDBContext context)
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

            if (review == null)
            {
                return NotFound();
            }

            // Verify this review is for a course in an active semester
            if (!semesterIds.Contains(review.CourseEnrollment!.SemesterCourse!.SemesterId))
            {
                return RedirectToPage("./Index");
            }

            CourseReview = review;
            Course = review.CourseEnrollment.SemesterCourse.Course!;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
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

            // Get the existing review
            var existingReview = await _context.CourseReviews
                .Include(cr => cr.CourseEnrollment!)
                    .ThenInclude(ce => ce.SemesterCourse!)
                .Include(cr => cr.Student!)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(cr => cr.Id == CourseReview.Id);



            // Verify active semester
            if (!semesterIds.Contains(existingReview.CourseEnrollment!.SemesterCourse!.SemesterId))
            {
                return RedirectToPage("./Index");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Update only the rating and comment
            existingReview.Rating = CourseReview.Rating;
            existingReview.Comment = CourseReview.Comment;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseReviewExists(CourseReview.Id))
                {
                    return NotFound();
                }
                throw;
            }

            return RedirectToPage("./Index", new { selectedCourseId = existingReview.CourseEnrollment.SemesterCourse.CourseId });
        }

        private bool CourseReviewExists(Guid id)
        {
            return _context.CourseReviews.Any(e => e.Id == id);
        }
    }
}
