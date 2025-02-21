using CanvasLMS.Models;
using CanvasLMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace CanvasLMS.Pages;

[Authorize]
public class IndexModel : PageModel
{
    public required List<(string title, string url, string ImageUrl)> NavigationLinks { get; set; }
    public bool IsStudent { get; private set; }
    public required string UserName { get; set; }

    private readonly ApplicationDBContext _context;

    public IndexModel(ApplicationDBContext context)
    {
        _context = context;
    }

    public void OnGet()
    {
        var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.Empty.ToString());
        var user = _context.Users.Find(userId);

        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        UserName = user.Name;
        IsStudent = user.Role == Role.Student;

        if (IsStudent)
        {
            NavigationLinks = new List<(string title, string url, string ImageUrl)>
            {
                ("Course Enrollment", "/Courseenrollments/Enroll/Index", "https://via.placeholder.com/100"),
                ("Timetable Matching", "/CourseEnrollments/TimeTableMatching/Index", "https://via.placeholder.com/100"),
                ("Course Add/Drop", "/CourseEnrollments/AddDrop/Index", "https://via.placeholder.com/100"),
                ("Course Evaluation", "/CourseReviews/Index", "https://via.placeholder.com/100"),
                ("Student Statement", "/StudentStatement/Index", "https://via.placeholder.com/100"),
                ("Class Timetable", "/RegistrationSummary/Index", "https://via.placeholder.com/100"),
                ("Manage Payments", "/Payments/Index", "https://via.placeholder.com/100")
            };
        }
        else
        {
            NavigationLinks = new List<(string title, string url, string ImageUrl)>
            {
                ("Manage Users", "/Users/Index", "https://via.placeholder.com/100"),
                ("Manage Faculties", "/Faculties/Create", "https://via.placeholder.com/100"),
                ("Manage Semesters", "/Semesters/Create", "https://via.placeholder.com/100"),
                ("Manage Payments", "/Payments/Create", "https://via.placeholder.com/100"),
                ("Manage Courses", "/SemesterCourses/Create", "https://via.placeholder.com/100")
            };
        }
    }
}
