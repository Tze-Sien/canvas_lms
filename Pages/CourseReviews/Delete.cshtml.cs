using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.CourseReviews
{
    public class DeleteModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public DeleteModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CourseReview CourseReview { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coursereview = await _context.CourseReviews.FirstOrDefaultAsync(m => m.Id == id);

            if (coursereview is not null)
            {
                CourseReview = coursereview;

                return Page();
            }

            return NotFound();
        }

        public async Task<IActionResult> OnPostAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var coursereview = await _context.CourseReviews.FindAsync(id);
            if (coursereview != null)
            {
                CourseReview = coursereview;
                _context.CourseReviews.Remove(CourseReview);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
