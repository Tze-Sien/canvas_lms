using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;
using System.Security.Claims;

namespace CanvasLMS.Pages.CourseEnrollments.AddDrop
{
    public class ApprovalModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public ApprovalModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public IList<AddDropHistory> PendingRequests { get; set; } = default!;

        [BindProperty]
        public Guid RequestId { get; set; }

        [BindProperty]
        public AddDropApproval Action { get; set; }

        [BindProperty]
        public string? Comment { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get all pending requests with related data
            PendingRequests = await _context.AddDropHistories
                .Include(a => a.Student!)
                    .ThenInclude(s => s!.User!)
                .Include(a => a.Course)
                .Include(a => a.Semester)
                .Where(a => a.Status == AddDropApproval.Pending)
                .OrderByDescending(a => a.RequestedAt)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);

            if (userId == Guid.Empty)
            {
                return RedirectToPage("/Logout");
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.Role == Role.Student)
            {
                return RedirectToPage("/Logout");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var request = await _context.AddDropHistories
                .Include(a => a.Student)
                .Include(a => a.Course)
                .FirstOrDefaultAsync(a => a.Id == RequestId);

            if (request == null)
            {
                return NotFound();
            }

            // Get current user ID from claims
            var nameIdentifier = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(nameIdentifier) || !Guid.TryParse(nameIdentifier, out var currentUserId))
            {
                return Forbid();
            }

            // Get lecturer record
            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(l => l.UserId == currentUserId);
            
            if (lecturer == null && !User.IsInRole(Role.Admin.ToString()))
            {
                return Forbid();
            }

            // Update request status
            request.Status = Action;
            request.ActionedAt = DateTime.UtcNow;
            request.ActionedById = lecturer?.Id;
            request.Comment = Comment;

            // Handle CourseEnrollment updates based on approval
            if (Action == AddDropApproval.Approved)
            {
                if (request.Action == EnrollmentStatus.Enrolled) // Add Course
                {
                    // Get the current semester course
                    var semesterCourse = await _context.SemesterCourses
                        .FirstOrDefaultAsync(sc => 
                            sc.CourseId == request.CourseId && 
                            sc.SemesterId == request.SemesterId);

                    if (semesterCourse != null)
                    {
                        // Check if enrollment already exists
                        var existingEnrollment = await _context.CourseEnrollments
                            .FirstOrDefaultAsync(ce => 
                                ce.StudentId == request.StudentId && 
                                ce.SemesterCourseId == semesterCourse.Id);

                        if (existingEnrollment == null)
                        {
                            // Create new enrollment
                            var newEnrollment = new CourseEnrollment
                            {
                                StudentId = request.StudentId,
                                SemesterCourseId = semesterCourse.Id,
                                Status = EnrollmentStatus.Enrolled,
                                Approval = AddDropApproval.Approved
                            };
                            _context.CourseEnrollments.Add(newEnrollment);
                        }
                        else
                        {
                            // Update existing enrollment
                            existingEnrollment.Status = EnrollmentStatus.Enrolled;
                            existingEnrollment.Approval = AddDropApproval.Approved;
                        }
                    }
                }
                else if (request.Action == EnrollmentStatus.Dropped) // Drop Course
                {
                    // Find and remove the enrollment
                    var enrollment = await _context.CourseEnrollments
                        .Include(ce => ce.SemesterCourse)
                        .FirstOrDefaultAsync(ce => 
                            ce.StudentId == request.StudentId && 
                            ce.SemesterCourse != null && 
                            ce.SemesterCourse.CourseId == request.CourseId &&
                            ce.SemesterCourse.SemesterId == request.SemesterId);

                    if (enrollment != null)
                    {
                        _context.CourseEnrollments.Remove(enrollment);
                    }
                }
            }

            await _context.SaveChangesAsync();

            return RedirectToPage("/CourseEnrollments/AddDrop/Approvals/Index");
        }
    }
}
