using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;

namespace CanvasLMS.Pages.CourseEnrollments
{
    public class EditModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public EditModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public CourseEnrollment CourseEnrollment { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var courseenrollment = await _context.CourseEnrollments
                .Include(c => c.SemesterCourse)
                    .ThenInclude(sc => sc!.Course)
                .Include(c => c.Student)
                    .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (courseenrollment == null)
            {
                return NotFound();
            }
            CourseEnrollment = courseenrollment;

            ViewData["SemesterCourseId"] = new SelectList(_context.SemesterCourses
                .Include(sc => sc.Course)
                .Select(sc => new { sc.Id, DisplayName = sc.Course!.Name }), 
                "Id", "DisplayName");

            ViewData["StudentId"] = new SelectList(_context.Students
                .Include(s => s.User)
                .Select(s => new { s.Id, DisplayName = s.User!.Name }), 
                "Id", "DisplayName");

            ViewData["ApprovalOptions"] = new SelectList(
                Enum.GetValues(typeof(AddDropApproval))
                    .Cast<AddDropApproval>()
                    .Select(e => new { Id = e, Name = e.ToString() }),
                "Id",
                "Name");

            ViewData["StatusOptions"] = new SelectList(
                Enum.GetValues(typeof(EnrollmentStatus))
                    .Cast<EnrollmentStatus>()
                    .Select(e => new { Id = e, Name = e.ToString() }),
                "Id",
                "Name");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {

                // Display the error message
                foreach (var modelState in ModelState.Values)
                {
                    foreach (var error in modelState.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.ErrorMessage);
                    }
                }
                

                return Page();
            }

            _context.Attach(CourseEnrollment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CourseEnrollmentExists(CourseEnrollment.Id))
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

        private bool CourseEnrollmentExists(Guid id)
        {
            return _context.CourseEnrollments.Any(e => e.Id == id);
        }
    }
}
