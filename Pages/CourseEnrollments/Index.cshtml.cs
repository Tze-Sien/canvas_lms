using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.CourseEnrollments
{
    public class IndexModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public IndexModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public IList<CourseEnrollment> CourseEnrollment { get;set; } = default!;

        public async Task OnGetAsync()
        {
            CourseEnrollment = await _context.CourseEnrollments
                .Include(c => c.SemesterCourse)
                .Include(c => c.Student).ToListAsync();
        }
    }
}
