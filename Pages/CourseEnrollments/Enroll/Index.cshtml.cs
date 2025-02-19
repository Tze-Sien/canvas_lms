using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;
using System.Security.Claims;
using NuGet.Protocol;

namespace CanvasLMS.Pages.CourseEnrollments.Enroll
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public IndexModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public Student? CurrentStudent { get; set; }
        public Semester? OpenSemester { get; set; }
        public bool IsAlreadyEnrolled { get; set; }
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {

            // 1. Check if user is authenticated and get email
            Guid userId;
            if (!Guid.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out userId))
            {
                ErrorMessage = "User not authenticated.";
                return Page();
            }

            userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);

            if (userId == Guid.Empty)
            {
                ErrorMessage = "User not authenticated.";
                return Page();
            }

            // 2. Find user by email and verify role
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            // Role actually is string
            if (user == null || user.Role.ToString() != "Student")
            {
                ErrorMessage = "Access denied. Only students can access this page.";
                return Page();
            }

            // 3. Get student details and check status
            CurrentStudent = await _context.Students
                .Include(s => s.Faculty)
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (CurrentStudent == null)
            {
                ErrorMessage = "Student record not found.";
                return Page();
            }

            if (CurrentStudent.Status != StudentStatus.Active)
            {
                ErrorMessage = "You must be an active student to enroll in courses.";
                return Page();
            }

            // 4. Find open semester for student's faculty
            OpenSemester = await _context.Semesters
                .FirstOrDefaultAsync(s => 
                    s.FacultyId == CurrentStudent.FacultyId && 
                    s.Status == SemesterStatus.OpenForEnrollment);

            if (OpenSemester == null)
            {
                ErrorMessage = "No open enrollment period found for your faculty.";
                return Page();
            }

            // 5. Check if already enrolled
            IsAlreadyEnrolled = await _context.SemesterStudents
                .AnyAsync(ss => ss.StudentId == CurrentStudent.Id && ss.SemesterId == OpenSemester.Id);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            await OnGetAsync(); // Revalidate current state

            if (CurrentStudent == null || OpenSemester == null)
            {
                ErrorMessage = "Invalid state. Please try again.";
                return Page();
            }

            if (IsAlreadyEnrolled)
            {
                ErrorMessage = "You are already enrolled in this semester.";
                return Page();
            }

            // Create semester enrollment with required properties
            var semesterStudent = new SemesterStudent
            {
                StudentId = CurrentStudent.Id,
                SemesterId = OpenSemester.Id,
                Student = CurrentStudent,
                Semester = OpenSemester
            };

            _context.SemesterStudents.Add(semesterStudent);
            await _context.SaveChangesAsync();

            SuccessMessage = "Successfully enrolled in the semester! You can now add or drop courses.";
            return RedirectToPage("/CourseEnrollments/AddDrop/Index");
        }
    }
}