using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Text.Json;

namespace CanvasLMS.Pages.CourseEnrollments.TimeTableMatching
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public IndexModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public class CourseViewModel
        {
            public Guid Id { get; set; }
            public string CourseName { get; set; }
            public CanvasLMS.Models.DayOfWeek Day { get; set; }
            public TimeSpan StartTime { get; set; }
            public TimeSpan EndTime { get; set; }
            public int CreditHours { get; set; }
            public string LecturerName { get; set; }
        }

        [BindProperty(SupportsGet = true)]
        public CanvasLMS.Models.DayOfWeek? FilterDay { get; set; }

        public List<CourseViewModel> AvailableCourses { get; set; } = new();
        public List<CourseViewModel> PlannedCourses { get; set; } = new();
        public SelectList DayOptions { get; set; }
        public string ErrorMessage { get; set; }

        private void SavePlannedCoursesToSession()
        {
            try
            {
                var serialized = JsonSerializer.Serialize(PlannedCourses);
                Console.WriteLine($"Saving to session: {serialized}");
                HttpContext.Session.SetString("PlannedCourses", serialized);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving planned courses to session: {ex.Message}");
            }
        }

        private void LoadPlannedCoursesFromSession()
        {
            var serialized = HttpContext.Session.GetString("PlannedCourses");
            Console.WriteLine($"Loading from session: {serialized}");
            if (!string.IsNullOrEmpty(serialized))
            {
                try
                {
                    PlannedCourses = JsonSerializer.Deserialize<List<CourseViewModel>>(serialized) ?? new();
                    Console.WriteLine($"Loaded {PlannedCourses.Count} courses from session");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error deserializing planned courses: {ex.Message}");
                    PlannedCourses = new List<CourseViewModel>();
                }
            }
            else
            {
                Console.WriteLine("No planned courses in session");
                PlannedCourses = new List<CourseViewModel>();
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current semester that is open for enrollment
            var currentSemester = await _context.Semesters
                .FirstOrDefaultAsync(s => s.Status == SemesterStatus.OpenForEnrollment);

            // Load planned courses at the start
            LoadPlannedCoursesFromSession();

            if (currentSemester == null)
            {
                ErrorMessage = "No semester is currently open for enrollment.";
                return Page();
            }

            // Get all available courses for the semester
            var courses = await _context.SemesterCourses
                .Include(sc => sc.Course)
                    .ThenInclude(c => c.Lecturer)
                        .ThenInclude(l => l.User)
                .Where(sc => sc.SemesterId == currentSemester.Id)
                .Select(sc => new CourseViewModel
                {
                    Id = sc.Id,
                    CourseName = sc.Course.Name,
                    Day = sc.Day,
                    StartTime = sc.StartTime,
                    EndTime = sc.EndTime,
                    CreditHours = sc.Course.CreditHours,
                    LecturerName = sc.Course.Lecturer.User.Name
                })
                .ToListAsync();

            // Apply day filter if selected
            Console.WriteLine("FilterDay"); 
            Console.WriteLine(FilterDay);
            if (FilterDay.HasValue)
            {
                courses = courses.Where(c => c.Day == FilterDay.Value).ToList();
            }
            AvailableCourses = courses;
            DayOptions = new SelectList(Enum.GetValues(typeof(CanvasLMS.Models.DayOfWeek)));

            return Page();
        }

        private string FormatDay(CanvasLMS.Models.DayOfWeek day)
        {
            string[] days = { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
            return days[(int)day];
        }

        private (bool hasClash, string clashMessage, CourseViewModel clashingCourse) CheckForTimeClash(SemesterCourse newCourse)
        {
            LoadPlannedCoursesFromSession();

            foreach (var plannedCourse in PlannedCourses)
            {
                if (plannedCourse.Day == newCourse.Day &&
                    (
                        (newCourse.StartTime >= plannedCourse.StartTime && newCourse.StartTime < plannedCourse.EndTime) ||
                        (newCourse.EndTime > plannedCourse.StartTime && newCourse.EndTime <= plannedCourse.EndTime) ||
                        (newCourse.StartTime <= plannedCourse.StartTime && newCourse.EndTime >= plannedCourse.EndTime)
                    ))
                {
                    var message = $"Time clash with {plannedCourse.CourseName} ({FormatDay(plannedCourse.Day)} {plannedCourse.StartTime:hh\\:mm} - {plannedCourse.EndTime:hh\\:mm})";
                    return (true, message, plannedCourse);
                }
            }

            return (false, string.Empty, null);
        }

        [ValidateAntiForgeryToken]
        public JsonResult OnPostAddToPlanned(Guid courseId)
        {
            try 
            {
                LoadPlannedCoursesFromSession();

                var course = _context.SemesterCourses
                    .Include(sc => sc.Course)
                        .ThenInclude(c => c.Lecturer)
                            .ThenInclude(l => l.User)
                    .FirstOrDefault(sc => sc.Id == courseId);

                if (course == null)
                {
                    return new JsonResult(new { success = false, message = "Course not found." });
                }

                var (hasClash, clashMessage, clashingCourse) = CheckForTimeClash(course);
                if (hasClash)
                {
                    return new JsonResult(new { 
                        success = false, 
                        message = clashMessage,
                        clashingCourse = clashingCourse
                    });
                }

                var courseViewModel = new CourseViewModel
                {
                    Id = course.Id,
                    CourseName = course.Course.Name,
                    Day = course.Day,
                    StartTime = course.StartTime,
                    EndTime = course.EndTime,
                    CreditHours = course.Course.CreditHours,
                    LecturerName = course.Course.Lecturer.User.Name
                };

                PlannedCourses.Add(courseViewModel);
                SavePlannedCoursesToSession();

                // Log the operation to debug session state
                Console.WriteLine($"Added course to plan: {courseId}");
                var serialized = JsonSerializer.Serialize(courseViewModel);
                Console.WriteLine($"Course data: {serialized}");

                return new JsonResult(new { 
                    success = true,
                    course = courseViewModel
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding course: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return new JsonResult(new { success = false, message = "An error occurred while adding the course." });
            }
        }

        [ValidateAntiForgeryToken]
        public JsonResult OnPostRemoveFromPlanned(Guid courseId)
        {
            try
            {
                // Load current planned courses first
                LoadPlannedCoursesFromSession();

                var course = PlannedCourses.FirstOrDefault(c => c.Id == courseId);
                if (course == null)
                {
                    Console.WriteLine($"Course not found in planned courses: {courseId}");
                    return new JsonResult(new { success = false, message = "Course not found in planned courses." });
                }

                PlannedCourses.Remove(course);
                SavePlannedCoursesToSession();

                // Verify removal
                var serialized = HttpContext.Session.GetString("PlannedCourses");
                Console.WriteLine($"Session after remove: {serialized}");

                return new JsonResult(new { success = true });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error removing course: {ex.Message}");
                return new JsonResult(new { success = false, message = "An error occurred while removing the course." });
            }
        }
    }
}
