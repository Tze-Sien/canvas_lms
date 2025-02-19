using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;
using System.Security.Claims;

namespace CanvasLMS.Pages.CourseEnrollments.AddDrop
{
    public class HistoryModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public HistoryModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public IList<AddDropHistory> History { get; set; } = default!;

        [BindProperty(SupportsGet = true)]
        public Guid? SemesterId { get; set; }

        public IList<Semester> Semesters { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Forbid();
            }

            

            // Get current user's role and ID
            var currentUserId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? throw new InvalidOperationException("User not authenticated."));
            var isAdmin = User.IsInRole(Role.Admin.ToString());
            var isLecturer = User.IsInRole(Role.Lecturer.ToString());

            // Get all semesters for the filter dropdown
            Semesters = await _context.Semesters
                .OrderByDescending(s => s.StartDate)
                .ToListAsync();

            // Base query with includes
            var query = _context.AddDropHistories
                .Include(a => a.Student)
                    .ThenInclude(s => s.User)
                .Include(a => a.Course)
                .Include(a => a.Semester)
                .Include(a => a.ActionedBy)
                    .ThenInclude(l => l!.User)
                .AsQueryable();

            // Apply role-based filtering
            if (!isAdmin && !isLecturer)
            {
                // Students can only see their own history
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.UserId == currentUserId);

                if (student == null)
                {
                    return Forbid();
                }

                query = query.Where(a => a.StudentId == student.Id);
            }
            else if (isLecturer)
            {
                // Lecturers can see requests for their courses
                var lecturer = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.UserId == currentUserId);

                if (lecturer == null)
                {
                    return Forbid();
                }

                query = query.Where(a => a.ActionedById == lecturer.Id);
            }

            // Apply semester filter if selected
            if (SemesterId.HasValue)
            {
                query = query.Where(a => a.SemesterId == SemesterId.Value);
            }

            // Get final results ordered by most recent first
            History = await query
                .OrderByDescending(a => a.RequestedAt)
                .ToListAsync();

            return Page();
        }
    }
}
