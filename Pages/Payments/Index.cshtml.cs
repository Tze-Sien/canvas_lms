using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.Payments
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public IndexModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public IList<CourseEnrollment> PendingPayments { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // Get approved enrollments that need payment
            PendingPayments = await _context.CourseEnrollments
                .Include(ce => ce.SemesterCourse)
                    .ThenInclude(sc => sc.Course)
                .Include(ce => ce.Student)
                    .ThenInclude(s => s.User)
                .Where(ce =>
                    ce.Status == EnrollmentStatus.Enrolled &&
                    ce.Approval == AddDropApproval.Approved)
                .ToListAsync();
        }
    }
}
