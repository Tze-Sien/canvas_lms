using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;
using System.Security.Claims;

namespace CanvasLMS.Pages.CourseEnrollments.AddDrop
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public IndexModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public User? CurrentUser { get; set; }
        public Student? Student { get; set; }
        public List<SemesterCourse> AvailableCourses { get; set; } = new();
        public List<CourseEnrollment> CurrentEnrollments { get; set; } = new();
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
                return Page();
            }

            // Get user and validate role
            CurrentUser = await _context.Users.FindAsync(userId);
            if (CurrentUser == null || CurrentUser.Role != Role.Student)
            {
                ErrorMessage = "Access denied. Only students can access this page.";
                return RedirectToPage("/Index");
            }

            // Get student record
            Student = await _context.Students
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (Student == null)
            {
                ErrorMessage = "Student record not found.";
                return RedirectToPage("/Index");
            }

            // Get current semester that's open for enrollment
            var currentSemester = await _context.Semesters
                .FirstOrDefaultAsync(s => s.Status == SemesterStatus.OpenForEnrollment);

            if (currentSemester == null)
            {
                ErrorMessage = "No semester is currently open for enrollment.";
                return Page();
            }

            // Get available courses for the semester
            AvailableCourses = await _context.SemesterCourses
                .Include(sc => sc.Course)
                    .ThenInclude(c => c!.Lecturer)
                        .ThenInclude(l => l!.User)
                .Where(sc => sc.SemesterId == currentSemester.Id)
                .ToListAsync();

            // Get current enrollments
            CurrentEnrollments = await _context.CourseEnrollments
                .Include(ce => ce.SemesterCourse)
                    .ThenInclude(sc => sc.Course)
                .Where(ce => ce.StudentId == Student.Id)
                .Where(ce => ce.SemesterCourse.SemesterId == currentSemester.Id)
                .ToListAsync();

            // Filter out enrolled courses from available courses
            var enrolledCourseIds = CurrentEnrollments
                .Where(ce => ce.Status == EnrollmentStatus.Enrolled && ce.Approval == AddDropApproval.Approved)
                .Select(ce => ce.SemesterCourse.CourseId)
                .ToHashSet();

            AvailableCourses = AvailableCourses
                .Where(sc => !enrolledCourseIds.Contains(sc.CourseId))
                .ToList();

            return Page();
        }

        public async Task<IActionResult> OnPostEnrollAsync(Guid semesterCourseId)
        {
            // Validate user is still logged in and is a student
            Guid userId;
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
            {
                ErrorMessage = "User not authenticated.";
                return Page();
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
            {
                ErrorMessage = "Student record not found.";
                return RedirectToPage("/Index");
            }

            // Get all existing enrollments and sort by UUID to find the last one
            var existingEnrollments = await _context.CourseEnrollments
                .Where(ce => ce.StudentId == student.Id && ce.SemesterCourseId == semesterCourseId)
                .OrderByDescending(ce => ce.Id)
                .ToListAsync();

            // Check if the last one is added and approved
            var existingEnrollment = existingEnrollments.FirstOrDefault();
            if (existingEnrollment != null && existingEnrollment.Status == EnrollmentStatus.Enrolled && existingEnrollment.Approval == AddDropApproval.Approved)
            {
                ErrorMessage = "You are already enrolled in this course.";
                return RedirectToPage();
            }

            // Pending approval 
            if (existingEnrollment != null && existingEnrollment.Approval == AddDropApproval.Pending)
            {
                ErrorMessage = "A request is already pending for this course.";
                return RedirectToPage();
            }

            // Get required related entities
            var semesterCourse = await _context.SemesterCourses.FindAsync(semesterCourseId);
            if (semesterCourse == null)
            {
                ErrorMessage = "Course not found.";
                return RedirectToPage();
            }

            // Create new enrollment
            // Create new enrollment
            var enrollment = new CourseEnrollment
            {
                SemesterCourseId = semesterCourseId,
                SemesterCourse = semesterCourse,
                StudentId = student.Id,
                Student = student,
                Status = EnrollmentStatus.Enrolled,
                Approval = AddDropApproval.Pending,
            };

            // Create add/drop history record
            var history = new AddDropHistory
            {
                SemesterId = semesterCourse.SemesterId,
                StudentId = student.Id,
                CourseId = semesterCourse.CourseId,
                Action = EnrollmentStatus.Enrolled,
                Status = AddDropApproval.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.CourseEnrollments.Add(enrollment);
            _context.AddDropHistories.Add(history);
            await _context.SaveChangesAsync();


            SuccessMessage = "Course enrollment pending approval.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDropAsync(Guid enrollmentId)
        {
            // Validate user is still logged in and is a student
            Guid userId;
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
            {
                ErrorMessage = "User not authenticated.";
                return Page();
            }

            var student = await _context.Students.FirstOrDefaultAsync(s => s.UserId == userId);
            if (student == null)
            {
                ErrorMessage = "Student record not found.";
                return RedirectToPage("/Index");
            }

            // Get existing enrollment
            var enrollment = await _context.CourseEnrollments
                .Include(ce => ce.SemesterCourse)
                .FirstOrDefaultAsync(ce => ce.Id == enrollmentId && ce.StudentId == student.Id);

            if (enrollment == null)
            {
                ErrorMessage = "Enrollment not found.";
                return RedirectToPage();
            }

            if (enrollment.Approval == AddDropApproval.Pending)
            {
                ErrorMessage = "A request is already pending for this course.";
                return RedirectToPage();
            }

            // Update enrollment status
            enrollment.Status = EnrollmentStatus.Dropped;
            enrollment.Approval = AddDropApproval.Pending;

            // Create add/drop history record
            var history = new AddDropHistory
            {
                SemesterId = enrollment.SemesterCourse.SemesterId,
                StudentId = student.Id,
                CourseId = enrollment.SemesterCourse.CourseId,
                Action = EnrollmentStatus.Dropped,
                Status = AddDropApproval.Pending,
                RequestedAt = DateTime.UtcNow
            };

            _context.AddDropHistories.Add(history);
            await _context.SaveChangesAsync();

            SuccessMessage = "Course drop request pending approval.";
            return RedirectToPage();
        }
    }
}
