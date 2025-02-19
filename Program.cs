using Microsoft.EntityFrameworkCore;
using CanvasLMS.Services;
using CanvasLMS.Models;

var builder = WebApplication.CreateBuilder(args);
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
// Initialize the database
await InitializeDatabaseAsync(app);

// Method to handle database initialization
async Task InitializeDatabaseAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDBContext>();
        
        try
        {
            await context.Database.EnsureCreatedAsync();
            Console.WriteLine("Database created successfully.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to ensure database creation: {ex.Message}");
            // Continue execution despite database creation failure
        }

        try
        {
            // Check if admin user exists
            if (!await context.Users.AnyAsync(u => u.Email == "admin@gmail.com"))
            {
                // Create admin user
                var adminUser = new User
                {
                    Email = "admin@gmail.com",
                    Password = BCrypt.Net.BCrypt.HashPassword("1234"),
                    Name = "Administrator",
                    Role = Role.Admin
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
                Console.WriteLine("Admin user seeded successfully.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Warning: Failed to seed admin user: {ex.Message}");
            // Continue execution despite seeding failure
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Warning: Database context creation failed: {ex.Message}");
        // Continue execution despite context creation failure
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
