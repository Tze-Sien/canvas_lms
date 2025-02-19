using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using CanvasLMS.Models;
using CanvasLMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace CanvasLMS.Pages.Students
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public CreateModel(ApplicationDBContext context)
        {
            _context = context;
        }

        public IActionResult OnGet()
        {
            PopulateDropDowns();
            return Page();
        }

        [BindProperty]
        public Student Student { get; set; } = default!;

        private void PopulateDropDowns()
        {
            ViewData["FacultyId"] = new SelectList(_context.Faculties.AsNoTracking(), "Id", "Name");
            ViewData["UserId"] = new SelectList(_context.Users.AsNoTracking().Where(u => u.Role == Role.Student), "Id", "Email");
            ViewData["Status"] = new SelectList(Enum.GetValues<StudentStatus>());
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                PopulateDropDowns();
                return Page();
            }

            // Verify Faculty exists
            var faculty = await _context.Faculties.FindAsync(Student.FacultyId);
            if (faculty == null)
            {
                ModelState.AddModelError("Student.FacultyId", "Selected faculty does not exist");
                PopulateDropDowns();
                return Page();
            }

            // Verify User exists and is a student
            var user = await _context.Users.FindAsync(Student.UserId);
            if (user == null)
            {
                ModelState.AddModelError("Student.UserId", "Selected user does not exist");
                PopulateDropDowns();
                return Page();
            }
            if (user.Role != Role.Student)
            {
                ModelState.AddModelError("Student.UserId", "Selected user must be a student");
                PopulateDropDowns();
                return Page();
            }

            // Check if user is already assigned to a student
            var existingStudent = await _context.Students.AnyAsync(s => s.UserId == Student.UserId);
            if (existingStudent)
            {
                ModelState.AddModelError("Student.UserId", "Selected user is already assigned to a student");
                PopulateDropDowns();
                return Page();
            }

            // Update user role to Student
            user.Role = Role.Student;
            _context.Users.Update(user);

            // Add the new student
            _context.Students.Add(Student);
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index");
        }
    }
}
