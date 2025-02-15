using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public Role Role { get; set; }
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
        public required Faculty Faculty { get; set; }
        [ForeignKey("Lecturer")]
        public Guid? LecturerId { get; set; }
        public Lecturer? Lecturer { get; set; }
    }

    public class Student
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("Faculty")]
        public Guid FacultyId { get; set; }
        public required Faculty Faculty { get; set; }
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public required User User { get; set; }
        public string? Address { get; set; }
        public string? Postcode { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? BankAcc { get; set; }
        public string? BankName { get; set; }
        public string? BankHolderName { get; set; }
        public StudentStatus Status { get; set; }
    }

    public class Lecturer
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("Faculty")]
        public Guid FacultyId { get; set; }
        public required Faculty Faculty { get; set; }
        [ForeignKey("User")]
        public Guid UserId { get; set; }
        public required User User { get; set; }
    }

    public class SemesterCourse
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [ForeignKey("Semester")]
        public Guid SemesterId { get; set; }
        public required Semester Semester { get; set; }
        [ForeignKey("Course")]
        public Guid CourseId { get; set; }
        public required Course Course { get; set; }
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
        public required SemesterCourse SemesterCourse { get; set; }
        [ForeignKey("Student")] 
        public Guid StudentId { get; set; }
        public required Student Student { get; set; }
        public bool Approved { get; set; }
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
        public required CourseEnrollment CourseEnrollment { get; set; }
        [ForeignKey("Student")]
        public Guid StudentId { get; set; }
        public required Student Student { get; set; }
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
        public required User User { get; set; }
        public int Amount { get; set; }
        public PaymentStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
