using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;
using System.Security.Claims;

namespace CanvasLMS.Pages.StudentStatement
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public IndexModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public Student? Student { get; set; }
        public List<SemesterEnrollmentGroup> SemesterGroups { get; set; } = new();
        public int TotalCreditHours { get; set; }

        public class SemesterEnrollmentGroup
        {
            public Semester Semester { get; set; } = null!;
            public List<CourseEnrollmentInfo> Enrollments { get; set; } = new();
            public int SemesterCreditHours { get; set; }
        }

        public class CourseEnrollmentInfo
        {
            public CourseEnrollment Enrollment { get; set; } = null!;
            public Course Course { get; set; } = null!;
            public SemesterCourse SemesterCourse { get; set; } = null!;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToPage("/Login/Index");
            }

            Student = await _context.Students
                .Include(s => s.User)
                .Include(s => s.Faculty)
                .FirstOrDefaultAsync(s => s.User.Id == Guid.Parse(userId));

            if (Student == null)
            {
                return NotFound();
            }

            var enrollments = await _context.CourseEnrollments
                .Include(ce => ce.SemesterCourse)
                    .ThenInclude(sc => sc!.Course)
                .Include(ce => ce.SemesterCourse)
                    .ThenInclude(sc => sc!.Semester)
                .Where(ce => ce.StudentId == Student.Id)
                .OrderBy(ce => ce.SemesterCourse!.Semester!.StartDate)
                .ToListAsync();


            var groupedEnrollments = enrollments
                .GroupBy(e => e.SemesterCourse!.Semester)
                .OrderByDescending(g => g.Key!.StartDate);

            foreach (var group in groupedEnrollments)
            {
                var semesterGroup = new SemesterEnrollmentGroup
                {
                    Semester = group.Key!
                };

                foreach (var enrollment in group)
                {
                    semesterGroup.Enrollments.Add(new CourseEnrollmentInfo
                    {
                        Enrollment = enrollment,
                        Course = enrollment.SemesterCourse!.Course!,
                        SemesterCourse = enrollment.SemesterCourse
                    });
                    semesterGroup.SemesterCreditHours += enrollment.SemesterCourse.Course.CreditHours;
                }

                SemesterGroups.Add(semesterGroup);
                TotalCreditHours += semesterGroup.SemesterCreditHours;
            }

            return Page();
        }
    }
}
