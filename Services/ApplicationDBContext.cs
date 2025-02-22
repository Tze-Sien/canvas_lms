using CanvasLMS.Models;
using Microsoft.EntityFrameworkCore;

namespace CanvasLMS.Services
{
    public class ApplicationDBContext : DbContext
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
        public DbSet<AddDropHistory> AddDropHistories { get; set; }

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

            // Configure AddDropHistory relationships
            modelBuilder.Entity<AddDropHistory>()
            .HasOne(a => a.Semester)
            .WithMany()
            .HasForeignKey(a => a.SemesterId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AddDropHistory>()
            .HasOne(a => a.Student)
            .WithMany()
            .HasForeignKey(a => a.StudentId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AddDropHistory>()
            .HasOne(a => a.Course)
            .WithMany()
            .HasForeignKey(a => a.CourseId)
            .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<AddDropHistory>()
            .HasOne(a => a.ActionedBy)
            .WithMany()
            .HasForeignKey(a => a.ActionedById)
            .OnDelete(DeleteBehavior.NoAction);

            // Create composite index for efficient querying by semester and student
            modelBuilder.Entity<AddDropHistory>()
            .HasIndex(a => new { a.SemesterId, a.StudentId });

            // Configure SemesterCourse time-related properties
            modelBuilder.Entity<SemesterCourse>()
                .Property(sc => sc.StartTime)
                .IsRequired();

            modelBuilder.Entity<SemesterCourse>()
                .Property(sc => sc.EndTime)
                .IsRequired();

            modelBuilder.Entity<SemesterCourse>()
                .Property(sc => sc.Day)
                .IsRequired();

            // modelBuilder.Entity<SemesterCourse>()
            //     .Property(e => e.Fee)
            //     .HasColumnType("float"); // Ensure SQL Server uses FLOAT instead of DECIMAL
            modelBuilder.Entity<SemesterCourse>()
                .Property(e => e.Fee)
                .HasColumnType("int");
        }
    }
}
