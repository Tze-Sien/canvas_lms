using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace CanvasLMS.Pages.RegistrationSummary
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public IndexModel(ApplicationDBContext context)
        {
            _context = context;
        }

        private static System.DayOfWeek ConvertDayOfWeek(Models.DayOfWeek day)
        {
            return day switch
            {
                Models.DayOfWeek.Monday => System.DayOfWeek.Monday,
                Models.DayOfWeek.Tuesday => System.DayOfWeek.Tuesday,
                Models.DayOfWeek.Wednesday => System.DayOfWeek.Wednesday,
                Models.DayOfWeek.Thursday => System.DayOfWeek.Thursday,
                Models.DayOfWeek.Friday => System.DayOfWeek.Friday,
                Models.DayOfWeek.Saturday => System.DayOfWeek.Saturday,
                Models.DayOfWeek.Sunday => System.DayOfWeek.Sunday,
                _ => throw new ArgumentException("Invalid day of week", nameof(day))
            };
        }

        public class CourseSchedule
        {
            public string CourseName { get; set; } = string.Empty;
            public string LecturerName { get; set; } = string.Empty;
            public System.DayOfWeek Day { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int CreditHours { get; set; }
        }

        public List<CourseSchedule> EnrolledCourses { get; set; } = new();
        public Dictionary<System.DayOfWeek, List<CourseSchedule>> TimeTable { get; set; } = new();
        public TimeSpan EarliestTime { get; set; } = TimeSpan.FromHours(8); // Default 8 AM
        public TimeSpan LatestTime { get; set; } = TimeSpan.FromHours(18); // Default 6 PM

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current user
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToPage("/Login/Index");
            }

            // Get student info
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Id == Guid.Parse(userId));

            if (student == null)
            {
                return NotFound();
            }

            // Get active semester
            var activeSemester = await _context.Semesters
                .FirstOrDefaultAsync(s => s.Status == SemesterStatus.Ongoing);
            if (activeSemester == null)
            {
                return Page();
            }

            // Get enrolled courses
            var enrolledCourses = await _context.CourseEnrollments
                .Include(ce => ce.SemesterCourse!)
                    .ThenInclude(sc => sc.Course!)
                        .ThenInclude(c => c.Lecturer!)
                            .ThenInclude(l => l.User)
                .Where(ce => ce.StudentId == student.Id 
                    && ce.SemesterCourse!.SemesterId == activeSemester.Id
                    && ce.Status == EnrollmentStatus.Enrolled)
                .ToListAsync();

            // Process courses
            foreach (var enrollment in enrolledCourses)
            {
                if (enrollment.SemesterCourse?.Course == null) continue;

                var schedule = new CourseSchedule
                {
                    CourseName = enrollment.SemesterCourse.Course.Name,
                    LecturerName = enrollment.SemesterCourse.Course.Lecturer?.User?.Name ?? "TBA",
                    Day = ConvertDayOfWeek(enrollment.SemesterCourse.Day),
                    StartTime = enrollment.SemesterCourse.StartTime,
                    EndTime = enrollment.SemesterCourse.EndTime,
                    CreditHours = enrollment.SemesterCourse.Course.CreditHours
                };

                EnrolledCourses.Add(schedule);

                // Add to timetable
                if (!TimeTable.ContainsKey(schedule.Day))
                {
                    TimeTable[schedule.Day] = new List<CourseSchedule>();
                }
                TimeTable[schedule.Day].Add(schedule);

                // Update time range
                EarliestTime = TimeSpan.FromTicks(Math.Min(EarliestTime.Ticks, schedule.StartTime.Ticks));
                LatestTime = TimeSpan.FromTicks(Math.Max(LatestTime.Ticks, schedule.EndTime.Ticks));
            }

            return Page();
        }
    }
}
