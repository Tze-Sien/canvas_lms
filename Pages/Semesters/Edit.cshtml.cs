using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.Semesters
{
    public class EditModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public EditModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Semester Semester { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var semester =  await _context.Semesters.FirstOrDefaultAsync(m => m.Id == id);
            if (semester == null)
            {
                return NotFound();
            }
            Semester = semester;
            return Page();
        }

        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more information, see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Semester).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SemesterExists(Semester.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("./Index");
        }

        private bool SemesterExists(Guid id)
        {
            return _context.Semesters.Any(e => e.Id == id);
        }
    }
}
