using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.CourseReviews
{
    public class CreateModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public CreateModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToPage("/Login/Index");
            }

            // Get the student info
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.User.Id == Guid.Parse(userId));

            if (student == null)
            {
                return Forbid();
            }

            // Get ongoing semester
            var currentSemester = await _context.Semesters
                .FirstOrDefaultAsync(s => s.Status == SemesterStatus.Ongoing);
            
            if (currentSemester == null)
            {
                ModelState.AddModelError(string.Empty, "No active semester found.");
                return Page();
            }



            // Get enrolled courses
            var enrolledCourses = await _context.CourseEnrollments
                .Include(ce => ce.SemesterCourse)
                    .ThenInclude(sc => sc.Course)
                .Where(ce => ce.StudentId == student.Id &&
                           ce.Approval == AddDropApproval.Approved &&
                           ce.Status == EnrollmentStatus.Enrolled &&
                           ce.SemesterCourse.SemesterId == currentSemester.Id)
                .Select(ce => new SelectListItem
                {
                    Value = ce.Id.ToString(),
                    Text = ce.SemesterCourse.Course.Name
                })
                .ToListAsync();

            if (!enrolledCourses.Any())
            {
                ModelState.AddModelError(string.Empty, "No enrolled courses found for review.");
                return Page();
            }

            ViewData["CourseEnrollmentId"] = new SelectList(enrolledCourses, "Value", "Text");
            
            // Don't initialize CourseReview here since we need the CourseEnrollment
            // and Student which will be set during POST
            
            return Page();
        }

        [BindProperty]
        public CourseReview CourseReview { get; set; } = default!;

        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return RedirectToPage("/Login/Index");
            }

            // Get the student info
            var student = await _context.Students
                .FirstOrDefaultAsync(s => s.UserId == Guid.Parse(userId));
            
            if (student == null)
            {
                return Forbid();
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Get the CourseEnrollment
            var courseEnrollment = await _context.CourseEnrollments
                .Include(ce => ce.Student)
                .FirstOrDefaultAsync(ce => ce.Id == CourseReview.CourseEnrollmentId);

            if (courseEnrollment == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid course enrollment.");
                return Page();
            }

            // Set the required properties
            CourseReview.Student = student;
            CourseReview.CourseEnrollment = courseEnrollment;
            CourseReview.StudentId = student.Id;
            CourseReview.CreatedAt = DateTime.Now;

            _context.CourseReviews.Add(CourseReview);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
