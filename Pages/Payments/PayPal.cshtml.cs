using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.Payments
{
    public class PayPalModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public decimal Amount { get; set; }

        public IActionResult OnGet()
        {
            if (Amount <= 0)
            {
                return RedirectToPage("./Index");
            }
            return Page();
        }

        private readonly ApplicationDBContext _context;

        public PayPalModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Simulate PayPal payment processing
            System.Threading.Thread.Sleep(2000); // Simulate network delay

            // Update payment status for all pending enrollments
            var pendingPayments = await _context.CourseEnrollments
                .Where(ce => ce.Status == EnrollmentStatus.Enrolled &&
                            ce.Approval == AddDropApproval.Approved &&
                            ce.PaymentStatus == PaymentStatus.Pending)
                .ToListAsync();

            foreach (var enrollment in pendingPayments)
            {
                enrollment.PaymentStatus = PaymentStatus.Paid;
                enrollment.PaidAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            // Redirect to success page
            return RedirectToPage("./PaymentSuccess");
        }
    }
}
