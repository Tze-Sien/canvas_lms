using CanvasLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace CanvasLMS.Services 
{
    public class ApplicationDBContext: DbContext
    {
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> contextOptions) : base(contextOptions)
        { }

        public DbSet<User> Users { get; set; }
        public DbSet<Faculty> Faculties { get; set; }
        public DbSet<Semester> Semesters { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<SemesterCourse> SemesterCourses { get; set; }
        public DbSet<SemesterStudent> SemesterStudents { get; set; }
        public DbSet<CourseEnrollment> CourseEnrollments { get; set; }
        public DbSet<StudentTranscript> StudentTranscripts { get; set; }
        public DbSet<CourseReview> CourseReviews { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<Payment> Payments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CourseEnrollment>()
            .HasOne(ce => ce.SemesterCourse)
            .WithMany()
            .HasForeignKey(ce => ce.SemesterCourseId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<CourseEnrollment>()
            .HasOne(ce => ce.Student)
            .WithMany()
            .HasForeignKey(ce => ce.StudentId)
            .OnDelete(DeleteBehavior.NoAction);
        }
    }
    
}