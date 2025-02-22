using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

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

        public IActionResult OnPost()
        {
            // Simulate PayPal payment processing
            System.Threading.Thread.Sleep(2000); // Simulate network delay

            // Redirect to success page
            return RedirectToPage("./PaymentSuccess");
        }
    }
}
