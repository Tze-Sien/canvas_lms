using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.SemesterStudents
{
    public class DetailsModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public DetailsModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public SemesterStudent SemesterStudent { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var semesterstudent = await _context.SemesterStudents.FirstOrDefaultAsync(m => m.Id == id);

            if (semesterstudent is not null)
            {
                SemesterStudent = semesterstudent;

                return Page();
            }

            return NotFound();
        }
    }
}
