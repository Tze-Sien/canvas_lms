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

        public IList<CourseEnrollment> Enrollments { get; set; } = default!;
        public decimal TotalPendingAmount { get; set; }
        public bool HasPendingPayments => Enrollments.Any(e => e.PaymentStatus == PaymentStatus.Pending);

        public async Task OnGetAsync()
        {
            // Get all approved enrollments
            Enrollments = await _context.CourseEnrollments
                .Include(ce => ce.SemesterCourse)
                    .ThenInclude(sc => sc.Course)
                .Include(ce => ce.Student)
                    .ThenInclude(s => s.User)
                .Where(ce =>
                    ce.Status == EnrollmentStatus.Enrolled &&
                    ce.Approval == AddDropApproval.Approved)
                .OrderByDescending(ce => ce.PaidAt)
                .ThenBy(ce => ce.SemesterCourse.Course.Name)
                .ToListAsync();

            // Calculate total pending amount
            TotalPendingAmount = Enrollments
                .Where(e => e.PaymentStatus == PaymentStatus.Pending)
                .Sum(p => p.SemesterCourse.Fee);
        }
    }
}
