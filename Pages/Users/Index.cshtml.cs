using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using CanvasLMS.Models;
using CanvasLMS.Services;

namespace CanvasLMS.Pages.Users
{
    public class IndexModel : PageModel
    {
        private readonly CanvasLMS.Services.ApplicationDBContext _context;

        public IndexModel(CanvasLMS.Services.ApplicationDBContext context)
        {
            _context = context;
        }

        public IList<User> User { get;set; } = default!;
        public string SortOrder { get; set; }

        public async Task OnGetAsync(string sortOrder)
        {
            SortOrder = sortOrder;

            var users = from u in _context.Users
                        select u;

            switch (sortOrder)
            {
                case "name_desc":
                    users = users.OrderByDescending(u => u.Name);
                    break;
                case "email_asc":
                    users = users.OrderBy(u => u.Email);
                    break;
                case "email_desc":
                    users = users.OrderByDescending(u => u.Email);
                    break;
                case "role_asc":
                    users = users.OrderBy(u => u.Role);
                    break;
                case "role_desc":
                    users = users.OrderByDescending(u => u.Role);
                    break;
                default:
                    users = users.OrderBy(u => u.Name);
                    break;
            }

            User = await users.ToListAsync();
        }
    }
}
