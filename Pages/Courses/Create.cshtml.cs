using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CanvasLMS.Models;
using CanvasLMS.Services;
using Microsoft.EntityFrameworkCore;

namespace CanvasLMS.Pages.Courses
{
    public class CreateModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public CreateModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        private void LoadDropDowns()
        {
            ViewData["FacultyId"] = new SelectList(_context.Faculties.AsNoTracking(), "Id", "Name");


            ViewData["LecturerId"] = new SelectList(_context.Users.Where(u => u.Role == Role.Lecturer).AsNoTracking(), "Id", "Name");
        }

        public IActionResult OnGet()
        {
            LoadDropDowns();
            return Page();
        }

        [BindProperty]
        public Course Course { get; set; } = default!;

        // static void PrintProperties(object obj)
        // {
        //     foreach (var prop in obj.GetType().GetProperties())
        //         Console.WriteLine($"{prop.Name}: {prop.GetValue(obj)}");
        //     Console.Out.Flush();
        // }

        public async Task<IActionResult> OnPostAsync()
        {

            // Verify Faculty exists
            var faculty = _context.Faculties.Find(Course.FacultyId);
            if (faculty == null)
            {
                ModelState.AddModelError("Course.FacultyId", "Selected faculty not found");
                LoadDropDowns();
                return Page();
            }


            this.Course.LecturerId = null;

            _context.Courses.Add(Course);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
