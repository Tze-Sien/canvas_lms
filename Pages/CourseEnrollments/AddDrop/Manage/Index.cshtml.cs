using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;
using System.Security.Claims;

namespace CanvasLMS.Pages.CourseEnrollments.AddDrop.Manage
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public IndexModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public User? CurrentUser { get; set; }
        public List<AddDropHistory> PendingRequests { get; set; } = new();
        [BindProperty]
        public string? Comment { get; set; }

        [TempData]
        public string? ErrorMessage { get; set; }
        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current user ID from session
            Guid userId;
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
            {
                ErrorMessage = "User not authenticated.";
                return RedirectToPage("/Index");
            }

            // Get user and validate role
            CurrentUser = await _context.Users.FindAsync(userId);
            if (CurrentUser == null || (CurrentUser.Role != Role.Admin && CurrentUser.Role != Role.FacultyAdmin))
            {
                ErrorMessage = "Access denied. Only administrators can access this page.";
                return RedirectToPage("/Index");
            }

            // Get pending requests with related data
            PendingRequests = await _context.AddDropHistories
                .Include(h => h.Student)
                    .ThenInclude(s => s!.User)
                .Include(h => h.Course)
                .Include(h => h.Semester)
                .Where(h => h.Status == AddDropApproval.Pending)
                .OrderByDescending(h => h.RequestedAt)
                .ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostApproveAsync(Guid historyId)
        {
            var history = await ValidateAndGetHistory(historyId);
            if (history == null) return RedirectToPage();

            // Update history record
            history.Status = AddDropApproval.Approved;
            history.ActionedAt = DateTime.UtcNow;
            history.Comment = Comment;

            // Get current user as lecturer (admin is also stored as lecturer for action tracking)
            var lecturer = await GetCurrentLecturer();
            if (lecturer != null)
            {
                history.ActionedById = lecturer.Id;
            }

            // Update enrollment status
            var enrollment = await _context.CourseEnrollments
                .Where(ce => ce.StudentId == history.StudentId)
                .Where(ce => ce.SemesterCourse.CourseId == history.CourseId)
                .Where(ce => ce.SemesterCourse.SemesterId == history.SemesterId)
                .OrderByDescending(ce => ce.Id)
                .FirstOrDefaultAsync();

            if (enrollment != null)
            {
                enrollment.Approval = AddDropApproval.Approved;
            }

            await _context.SaveChangesAsync();
            SuccessMessage = $"Request approved successfully.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostRejectAsync(Guid historyId)
        {
            var history = await ValidateAndGetHistory(historyId);
            if (history == null) return RedirectToPage();

            // Update history record
            history.Status = AddDropApproval.Rejected;
            history.ActionedAt = DateTime.UtcNow;
            history.Comment = Comment;

            // Get current user as lecturer
            var lecturer = await GetCurrentLecturer();
            if (lecturer != null)
            {
                history.ActionedById = lecturer.Id;
            }

            // Update enrollment status
            var enrollment = await _context.CourseEnrollments
                .Where(ce => ce.StudentId == history.StudentId)
                .Where(ce => ce.SemesterCourse.CourseId == history.CourseId)
                .Where(ce => ce.SemesterCourse.SemesterId == history.SemesterId)
                .OrderByDescending(ce => ce.Id)
                .FirstOrDefaultAsync();

            if (enrollment != null)
            {
                enrollment.Approval = AddDropApproval.Rejected;
                // Revert the status change since it was rejected
                enrollment.Status = enrollment.Status == EnrollmentStatus.Dropped ? 
                    EnrollmentStatus.Enrolled : EnrollmentStatus.Dropped;
            }

            await _context.SaveChangesAsync();
            SuccessMessage = $"Request rejected successfully.";
            return RedirectToPage();
        }

        private async Task<AddDropHistory?> ValidateAndGetHistory(Guid historyId)
        {
            // Validate user is still logged in and is an admin
            Guid userId;
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
            {
                ErrorMessage = "User not authenticated.";
                return null;
            }

            var user = await _context.Users.FindAsync(userId);
            if (user == null || (user.Role != Role.Admin && user.Role != Role.FacultyAdmin))
            {
                ErrorMessage = "Access denied. Only administrators can perform this action.";
                return null;
            }

            // Get history record with related data
            var history = await _context.AddDropHistories
                .Include(h => h.Student)
                    .ThenInclude(s => s!.User)
                .Include(h => h.Course)
                .Include(h => h.Semester)
                .FirstOrDefaultAsync(h => h.Id == historyId);

            if (history == null)
            {
                ErrorMessage = "Request not found.";
                return null;
            }

            if (history.Status != AddDropApproval.Pending)
            {
                ErrorMessage = "This request has already been processed.";
                return null;
            }

            return history;
        }

        private async Task<Lecturer?> GetCurrentLecturer()
        {
            Guid userId;
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
            {
                return null;
            }

            return await _context.Lecturers
                .FirstOrDefaultAsync(l => l.UserId == userId);
        }
    }
}
