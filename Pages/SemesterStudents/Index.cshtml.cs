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
    public class IndexModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public IndexModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public IList<SemesterStudent> SemesterStudent { get;set; } = default!;

        public async Task OnGetAsync()
        {
            SemesterStudent = await _context.SemesterStudents
                .Include(s => s.Semester)
                .Include(s => s.Student).ToListAsync();
        }
    }
}
