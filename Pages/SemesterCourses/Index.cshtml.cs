using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.SemesterCourses
{
    public class IndexModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public IndexModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public IList<SemesterCourse> SemesterCourse { get;set; } = default!;

        public async Task OnGetAsync()
        {
            SemesterCourse = await _context.SemesterCourses
                .Include(s => s.Course)
                .Include(s => s.Semester).ToListAsync();
        }
    }
}
