using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.Lecturers
{
    public class DetailsModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public DetailsModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public Lecturer Lecturer { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var lecturer = await _context.Lecturers.FirstOrDefaultAsync(m => m.Id == id);

            if (lecturer is not null)
            {
                Lecturer = lecturer;

                return Page();
            }

            return NotFound();
        }
    }
}
