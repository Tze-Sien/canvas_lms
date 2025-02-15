using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.StudentTranscripts
{
    public class DeleteModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public DeleteModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public StudentTranscript StudentTranscript { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var studenttranscript = await _context.StudentTranscripts.FirstOrDefaultAsync(m => m.Id == id);

            if (studenttranscript is not null)
            {
                StudentTranscript = studenttranscript;

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

            var studenttranscript = await _context.StudentTranscripts.FindAsync(id);
            if (studenttranscript != null)
            {
                StudentTranscript = studenttranscript;
                _context.StudentTranscripts.Remove(StudentTranscript);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index");
        }
    }
}
