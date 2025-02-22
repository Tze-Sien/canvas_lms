using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using CanvasLMS.Models;
using CanvasLMS.Services;
using System.Security.Claims;

namespace CanvasLMS.Pages.Profile
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDBContext _context;

        public IndexModel(ApplicationDBContext context)
        {
            _context = context;
        }

        [BindProperty]
        public PasswordUpdateModel PasswordModel { get; set; } = new();

        [BindProperty]
        public ProfileUpdateModel ProfileModel { get; set; } = new();

        [BindProperty]
        public BankDetailsModel BankModel { get; set; } = new();

        private async Task LoadUserData()
        {
            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var student = await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);



            if (student != null)
            {


                ProfileModel = new ProfileUpdateModel
                {
                    Address = student.Address ?? "",
                    Postcode = student.Postcode ?? "",
                    City = student.City ?? "",
                    State = student.State ?? "",
                    Country = student.Country ?? ""
                };

                BankModel = new BankDetailsModel
                {
                    BankAcc = student.BankAcc ?? "",
                    BankName = student.BankName ?? "",
                    BankHolderName = student.BankHolderName ?? ""
                };
            }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadUserData();
            return Page();
        }

        public async Task<IActionResult> OnPostPasswordAsync()
        {
            if (!ModelState.IsValid)
            {
                await LoadUserData();
                return Page();
            }

            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToPage();
            }

            // Verify current password using BCrypt
            if (!BCrypt.Net.BCrypt.Verify(PasswordModel.CurrentPassword, user.Password))
            {
                ModelState.AddModelError("PasswordModel.CurrentPassword", "Current password is incorrect.");
                await LoadUserData();
                return Page();
            }

            try
            {
                // Hash the new password using BCrypt
                user.Password = BCrypt.Net.BCrypt.HashPassword(PasswordModel.NewPassword);
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Password updated successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Password update error: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to update password. Please try again.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostProfileAsync()
        {

            Console.WriteLine("qq");
            Console.WriteLine(ModelState);



            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);

            Console.WriteLine("Helo World");
            var student = await _context.Students
                .Include(s => s.User)
                .AsTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToPage();
            }

            try
            {
                // Log entity state before changes
                var entry = _context.Entry(student);
                Console.WriteLine($"Entity State before changes: {entry.State}");

                // Set properties
                student.Address = ProfileModel.Address;
                student.Postcode = ProfileModel.Postcode;
                student.City = ProfileModel.City;
                student.State = ProfileModel.State;
                student.Country = ProfileModel.Country;

                Console.WriteLine(student.Address);

                // Explicitly mark properties as modified
                entry.Property(s => s.Address).IsModified = true;
                entry.Property(s => s.Postcode).IsModified = true;
                entry.Property(s => s.City).IsModified = true;
                entry.Property(s => s.State).IsModified = true;
                entry.Property(s => s.Country).IsModified = true;

                // Log entity state after changes
                Console.WriteLine($"Entity State after changes: {entry.State}");

                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine("SaveChangesAsync completed successfully");
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"Database update error: {dbEx.Message}");
                    Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                    throw;
                }

                TempData["SuccessMessage"] = "Profile details updated successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating profile: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to update profile. Please try again.";
                return RedirectToPage();
            }
        }

        public async Task<IActionResult> OnPostBankAsync()
        {


            var userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty);
            var student = await _context.Students
                .Include(s => s.User)
                .AsTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId);

            if (student == null)
            {
                TempData["ErrorMessage"] = "Student profile not found.";
                return RedirectToPage();
            }

            try
            {
                // Log entity state before changes
                var entry = _context.Entry(student);
                Console.WriteLine($"Entity State before changes: {entry.State}");

                // Set properties
                student.BankAcc = BankModel.BankAcc;
                student.BankName = BankModel.BankName;
                student.BankHolderName = BankModel.BankHolderName;

                // Explicitly mark properties as modified
                entry.Property(s => s.BankAcc).IsModified = true;
                entry.Property(s => s.BankName).IsModified = true;
                entry.Property(s => s.BankHolderName).IsModified = true;

                // Log entity state after changes
                Console.WriteLine($"Entity State after changes: {entry.State}");

                try
                {
                    await _context.SaveChangesAsync();
                    Console.WriteLine("SaveChangesAsync completed successfully");
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"Database update error: {dbEx.Message}");
                    Console.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                    throw;
                }

                TempData["SuccessMessage"] = "Bank details updated successfully.";
                return RedirectToPage();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating bank details: {ex.Message}");
                TempData["ErrorMessage"] = "Failed to update bank details. Please try again.";
                return RedirectToPage();
            }
        }
    }

    public class PasswordUpdateModel
    {
        [Required(ErrorMessage = "Current password is required")]
        [Display(Name = "Current Password")]
        public string CurrentPassword { get; set; } = "";

        [Required(ErrorMessage = "New password is required")]
        [StringLength(100, ErrorMessage = "Password must be at least {2} characters long.", MinimumLength = 6)]
        [Display(Name = "New Password")]
        public string NewPassword { get; set; } = "";

        [Required(ErrorMessage = "Confirm password is required")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        [Display(Name = "Confirm Password")]
        public string ConfirmPassword { get; set; } = "";
    }

    public class ProfileUpdateModel
    {
        [Display(Name = "Address")]
        public string Address { get; set; } = "";

        [Display(Name = "Postcode")]
        public string Postcode { get; set; } = "";

        [Display(Name = "City")]
        public string City { get; set; } = "";

        [Display(Name = "State")]
        public string State { get; set; } = "";

        [Display(Name = "Country")]
        public string Country { get; set; } = "";
    }

    public class BankDetailsModel
    {
        [Display(Name = "Bank Account Number")]
        public string BankAcc { get; set; } = "";

        [Display(Name = "Bank Name")]
        public string BankName { get; set; } = "";

        [Display(Name = "Account Holder Name")]
        public string BankHolderName { get; set; } = "";
    }
}
