using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;

namespace CanvasLMS.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string Name { get; set; }
        [Required, EmailAddress]
        public required string Email { get; set; }
        [Required]
        public required string Password { get; set; }
        public Role? Role { get; set; } = null;
    }

    public class Faculty
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string Name { get; set; }
    }

    public class Semester
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string Name { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        [ForeignKey("Faculty")]
        public Guid FacultyId { get; set; }
        [DeleteBehavior(DeleteBehavior.NoAction)]
        public Faculty? Faculty { get; set; }
        public SemesterStatus Status { get; set; } = SemesterStatus.Draft;
    }

    public class Course
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public required string Name { get; set; }
        public int CreditHours { get; set; }
        [ForeignKey("Faculty")]
        public Guid FacultyId { get; set; }
        public Faculty? Faculty { get; set; }
        [ForeignKey("User")]
        public Guid? LecturerId { get; set; }
        public Lecturer? Lecturer { get; set; }

        public float Fee { get; set; }
    }

    public class Student
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [ForeignKey("Faculty")]
        public Guid FacultyId { get; set; }
        public Faculty? Faculty { get; set; }

        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User? User { get; set; }

        public string? Address { get; set; }
        public string? Postcode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? BankAcc { get; set; }
        public string? BankName { get; set; }
        public string? BankHolderName { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public StudentStatus Status { get; set; }
    }

    public class Lecturer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("Faculty")]
        public Guid FacultyId { get; set; }
        public Faculty? Faculty { get; set; }
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }

    public class SemesterCourse
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [DisplayName("Semester")]
        [ForeignKey("Semester")]
        public Guid SemesterId { get; set; }
        public Semester? Semester { get; set; }

        [DisplayName("Course")]
        [ForeignKey("Course")]
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }

        [Required]
        public DayOfWeek Day { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }


    }

    public class SemesterStudent
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("Semester")]
        public Guid SemesterId { get; set; }
        public required Semester Semester { get; set; }
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public required Student Student { get; set; }
    }

    public class CourseEnrollment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("SemesterCourse")]
        public Guid SemesterCourseId { get; set; }
        public SemesterCourse? SemesterCourse { get; set; }
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public Student? Student { get; set; }
        public AddDropApproval Approval { get; set; }
        public EnrollmentStatus Status { get; set; }
    }

    public class StudentTranscript
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public required Student Student { get; set; }
        [ForeignKey("CourseEnrollment")]
        public Guid CourseEnrollmentId { get; set; }
        public required CourseEnrollment CourseEnrollment { get; set; }
        public int CreditHours { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CourseReview
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("CourseEnrollment")]
        public Guid CourseEnrollmentId { get; set; }
        public CourseEnrollment? CourseEnrollment { get; set; }
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public Student? Student { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class Invoice
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("CourseEnrollment")]
        public Guid CourseEnrollmentId { get; set; }
        public required CourseEnrollment CourseEnrollment { get; set; }
        public string? PaymentId { get; set; }
        public int Amount { get; set; }
        public DateTime CreatedAt { get; set; }
        public InvoiceStatus Status { get; set; }
    }

    public class Payment
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public User? User { get; set; }
        public int Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddDropHistory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [ForeignKey("Semester")]
        public Guid SemesterId { get; set; }
        public Semester? Semester { get; set; }

        [Required]
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public Student? Student { get; set; }

        [Required]
        [ForeignKey("Course")]
        public Guid CourseId { get; set; }
        public Course? Course { get; set; }

        public EnrollmentStatus Action { get; set; }
        public AddDropApproval Status { get; set; } = AddDropApproval.Pending;

        public DateTime RequestedAt { get; set; }
        public DateTime? ActionedAt { get; set; }

        [ForeignKey("ActionedBy")]
        public Guid? ActionedById { get; set; }
        public Lecturer? ActionedBy { get; set; }

        public string? Comment { get; set; }
    }
}
