using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;
using System.Security.Claims;

namespace CanvasLMS.Pages.Invoice
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public IndexModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public List<CourseEnrollment> Enrollments { get; set; } = new();
        public string InvoiceNumber => $"INV-{DateTime.Now:yyyyMMdd}-{User.FindFirst(ClaimTypes.NameIdentifier)?.Value?.Substring(0, 8)}";
        public DateTime InvoiceDate => DateTime.Now;
        public decimal TotalAmount => Enrollments.Sum(e => e.SemesterCourse?.Fee ?? 0);
        public Student? Student => Enrollments.FirstOrDefault()?.Student;

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Forbid();
            }

            // Get all enrollments for the user
            Enrollments = await _context.CourseEnrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s!.User)
                .Include(e => e.SemesterCourse)
                    .ThenInclude(sc => sc!.Course)
                .Where(e => e.Student!.UserId == Guid.Parse(userId))
                .ToListAsync();

            if (!Enrollments.Any())
            {
                return NotFound();
            }

            return Page();
        }
    }
}
