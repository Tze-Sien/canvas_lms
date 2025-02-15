using Microsoft.EntityFrameworkCore;
using CanvasLMS.Services;

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
        options.Conventions.AuthorizeFolder("/coursereviews");
        options.Conventions.AuthorizeFolder("/faculties");
        options.Conventions.AuthorizeFolder("/invoices");
        options.Conventions.AuthorizeFolder("/lecturers");
        options.Conventions.AuthorizeFolder("/payments");
        options.Conventions.AuthorizeFolder("/semesterstudents");
        options.Conventions.AuthorizeFolder("/students");
        options.Conventions.AuthorizeFolder("/studenttranscripts");
    });
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ApplicationDBContext") ?? throw new InvalidOperationException("Connection string 'ApplicationDBContext' not found.")));


var app = builder.Build();

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

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
