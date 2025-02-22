using Microsoft.EntityFrameworkCore;
using CanvasLMS.Services;
using CanvasLMS.Models;

var builder = WebApplication.CreateBuilder(args);


// Check if running in seed mode
if (args.Length > 0 && args[0] == "seed-data")
{
    await SeedData(builder.Configuration);
    return;
}

// Add authentication services
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.Cookie.Name = "AuthCookie"; // Name of the cookie
        options.LoginPath = "/Login"; // Path to the login page
        options.LogoutPath = "/Logout"; // Path to the logout page
        options.AccessDeniedPath = "/Account/AccessDenied"; // Path for access denied
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Cookie expiration time
        options.SlidingExpiration = true; // Renew the cookie on activity
        options.Cookie.HttpOnly = true; // Cookie is only accessible by the server
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Cookie.SameSite = SameSiteMode.Strict;
    });

builder.Services.AddAuthorization();


// Add services to the container.
builder.Services.AddRazorPages()
    .AddRazorPagesOptions(options =>
    {
        options.Conventions.AuthorizeFolder("/users");
        options.Conventions.AuthorizeFolder("/courses");
        options.Conventions.AuthorizeFolder("/courseenrollments");
        options.Conventions.AuthorizeFolder("/courseEnrollments/adddrop");
        options.Conventions.AuthorizeFolder("/courseenrollments/adddrop/histories");
        options.Conventions.AuthorizeFolder("/courseenrollments/adddrop/approvals");
        options.Conventions.AuthorizeFolder("/courseenrollments/enroll");
        options.Conventions.AuthorizeFolder("/registrationsummary");
        options.Conventions.AuthorizeFolder("/coursereviews");
        options.Conventions.AuthorizeFolder("/faculties");
        options.Conventions.AuthorizeFolder("/invoices");
        options.Conventions.AuthorizeFolder("/lecturers");
        options.Conventions.AuthorizeFolder("/payments");
        options.Conventions.AuthorizeFolder("/semesterstudents");
        options.Conventions.AuthorizeFolder("/students");
        options.Conventions.AuthorizeFolder("/studenttranscripts");
        options.Conventions.AuthorizeFolder("/studentstatement");
    });
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDBContext") ?? throw new InvalidOperationException("Connection string 'ApplicationDBContext' not found.")));


// Add session services
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Add database seeding
await InitializeDatabaseAsync(app);

// Method to handle database initialization
async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDBContext>();
        await context.Database.EnsureCreatedAsync();
        Console.WriteLine("Database created successfully.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error initializing database: {ex.Message}");
        throw;
    }
}

// Method to seed data
async Task SeedData(IConfiguration configuration)
{
    try
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDBContext>();
        optionsBuilder.UseSqlServer(configuration.GetConnectionString("ApplicationDBContext"));
        
        using var context = new ApplicationDBContext(optionsBuilder.Options);

        // Check and create Faculty
        var faculty = await context.Faculties.FirstOrDefaultAsync(f => f.Name == "Computer Science");
        if (faculty == null)
        {
            faculty = new Faculty { Name = "Computer Science" };
            context.Faculties.Add(faculty);
            await context.SaveChangesAsync();
            Console.WriteLine("Faculty created.");
        }

        // Check and create Users
        var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "admin@gmail.com");
        if (adminUser == null)
        {
            adminUser = new User
        {
            Email = "admin@gmail.com",
            Password = BCrypt.Net.BCrypt.HashPassword("admin"),
            Name = "Administrator",
            Role = Role.Admin
        };

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
            Console.WriteLine("Admin user created.");
        }

        var lecturerUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "lecturer@gmail.com");
        if (lecturerUser == null)
        {
            lecturerUser = new User
        {
            Email = "lecturer@gmail.com",
            Password = BCrypt.Net.BCrypt.HashPassword("lecturer"),
            Name = "John Doe",
            Role = Role.Lecturer
        };

            context.Users.Add(lecturerUser);
            await context.SaveChangesAsync();
            Console.WriteLine("Lecturer user created.");
        }

        var studentUser = await context.Users.FirstOrDefaultAsync(u => u.Email == "student@gmail.com");
        if (studentUser == null)
        {
            studentUser = new User
        {
            Email = "student@gmail.com",
            Password = BCrypt.Net.BCrypt.HashPassword("student"),
            Name = "Jane Smith",
            Role = Role.Student
        };

            context.Users.Add(studentUser);
            await context.SaveChangesAsync();
            Console.WriteLine("Student user created.");
        }

        // Check and create Lecturer
        var lecturer = await context.Lecturers.FirstOrDefaultAsync(l => l.UserId == lecturerUser.Id);
        if (lecturer == null)
        {
            lecturer = new Lecturer
        {
            User = lecturerUser,
            Faculty = faculty
        };
            context.Lecturers.Add(lecturer);
            await context.SaveChangesAsync();
            Console.WriteLine("Lecturer profile created.");
        }

        // Check and create Student
        var student = await context.Students.FirstOrDefaultAsync(s => s.UserId == studentUser.Id);
        if (student == null)
        {
            student = new Student
        {
            User = studentUser,
            Faculty = faculty,
            Status = StudentStatus.Active
        };
            context.Students.Add(student);
            await context.SaveChangesAsync();
            Console.WriteLine("Student profile created.");
        }

        // Check and create Courses
        var courseNames = new[] { "Web Development", "Data Structure and Analysis", "Database", "Networking" };
        var courses = new List<Course>();
        foreach (var courseName in courseNames)
        {
            var existingCourse = await context.Courses.FirstOrDefaultAsync(c => c.Name == courseName);
            if (existingCourse == null)
            {
                var newCourse = new Course { Name = courseName, CreditHours = 3, Faculty = faculty, Lecturer = lecturer };
                context.Courses.Add(newCourse);
                courses.Add(newCourse);
                Console.WriteLine($"Course {courseName} created.");
            }
            else
            {
                courses.Add(existingCourse);
            }
        }
        await context.SaveChangesAsync();

        // Check and create Semester
        var semesterName = "Y1S1 Jan 2025 - March 2025";
        var semester = await context.Semesters.FirstOrDefaultAsync(s => s.Name == semesterName);
        if (semester == null)
        {
            semester = new Semester
            {
                Name = semesterName,
                StartDate = new DateTime(2025, 1, 1),
                EndDate = new DateTime(2025, 3, 31),
                Faculty = faculty,
                Status = SemesterStatus.OpenForEnrollment
            };
            context.Semesters.Add(semester);
            await context.SaveChangesAsync();
            Console.WriteLine("Semester created.");
        }

        // Check and create Semester Courses with overlapping schedules
        var existingSemesterCourses = await context.SemesterCourses
            .Where(sc => sc.SemesterId == semester.Id)
            .ToListAsync();

        if (!existingSemesterCourses.Any())
        {
            var semesterCourses = new[]
        {
            new SemesterCourse 
            { 
                Semester = semester,
                Course = courses[0],
                Day = CanvasLMS.Models.DayOfWeek.Monday,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(11, 0, 0)
            },
            new SemesterCourse 
            { 
                Semester = semester,
                Course = courses[1],
                Day = CanvasLMS.Models.DayOfWeek.Monday,
                StartTime = new TimeSpan(10, 0, 0), // Overlaps with Web Development
                EndTime = new TimeSpan(12, 0, 0)
            },
            new SemesterCourse 
            { 
                Semester = semester,
                Course = courses[2],
                Day = CanvasLMS.Models.DayOfWeek.Tuesday,
                StartTime = new TimeSpan(14, 0, 0),
                EndTime = new TimeSpan(16, 0, 0)
            },
            new SemesterCourse 
            { 
                Semester = semester,
                Course = courses[3],
                Day = CanvasLMS.Models.DayOfWeek.Wednesday,
                StartTime = new TimeSpan(9, 0, 0),
                EndTime = new TimeSpan(11, 0, 0)
            }
        };
            context.SemesterCourses.AddRange(semesterCourses);
            await context.SaveChangesAsync();
            Console.WriteLine("Semester courses created.");
        }

        Console.WriteLine("Data seeding completed - existing data was preserved.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error seeding data: {ex.Message}");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.MapStaticAssets();

app.UseAuthentication();
app.UseAuthorization();

// Enable session middleware
app.UseSession();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
