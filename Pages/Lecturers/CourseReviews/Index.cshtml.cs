using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using System.Security.Claims;

namespace CanvasLMS.Pages.Lecturers.CourseReviews
{
    public class IndexModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public IndexModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public class CourseStatistics
        {
            public Course Course { get; set; } = null!;
            public double AverageRating { get; set; }
            public int ReviewCount { get; set; }
            public List<CourseReview> Reviews { get; set; } = new();
        }

        public List<CourseStatistics> CourseStats { get; set; } = new();
        [BindProperty(SupportsGet = true)]
        public string? SortBy { get; set; }
        [BindProperty(SupportsGet = true)]
        public string? FilterRating { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get current user ID from claims
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return RedirectToPage("/Login/Index");
            }

            // Get lecturer record
            var lecturer = await _context.Lecturers
                .FirstOrDefaultAsync(l => l.UserId == Guid.Parse(userId));

            if (lecturer == null)
            {
                return RedirectToPage("/Index");
            }

            // Get all courses taught by the lecturer
            var courses = await _context.Courses
                .Where(c => c.LecturerId == lecturer.Id)
                .ToListAsync();

            // For each course, get reviews and calculate statistics
            foreach (var course in courses)
            {
                var reviews = await _context.CourseReviews
                    .Include(r => r.Student)
                        .ThenInclude(s => s!.User)
                    .Include(r => r.CourseEnrollment)
                        .ThenInclude(ce => ce!.SemesterCourse)
                    .Where(r => r.CourseEnrollment!.SemesterCourse!.CourseId == course.Id)
                    .ToListAsync();

                if (FilterRating != null && int.TryParse(FilterRating, out int minRating))
                {
                    reviews = reviews.Where(r => r.Rating >= minRating).ToList();
                }

                var stats = new CourseStatistics
                {
                    Course = course,
                    Reviews = reviews,
                    ReviewCount = reviews.Count,
                    AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0
                };

                CourseStats.Add(stats);
            }

            // Apply sorting
            CourseStats = SortBy?.ToLower() switch
            {
                "rating_high" => CourseStats.OrderByDescending(s => s.AverageRating).ToList(),
                "rating_low" => CourseStats.OrderBy(s => s.AverageRating).ToList(),
                "count_high" => CourseStats.OrderByDescending(s => s.ReviewCount).ToList(),
                "count_low" => CourseStats.OrderBy(s => s.ReviewCount).ToList(),
                _ => CourseStats.OrderByDescending(s => s.AverageRating).ToList() // Default sort
            };

            return Page();
        }
    }
}
