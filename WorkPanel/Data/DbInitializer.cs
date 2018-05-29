using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WorkPanel.Models;

namespace WorkPanel.Data
{
    public class DbInitializer : IDbInitializer
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public DbInitializer(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //This example just creates an Administrator role and one Admin users
        public async Task Initialize()
        {
            //create database schema if none exists
            _context.Database.Migrate();

            //If there is already an Administrator role, abort
            if (!_context.Roles.Any(r => r.Name == "Administrator"))
            {
                //Create the Administartor Role
                var re = _roleManager.CreateAsync(new IdentityRole("Administrator")).Result;
            }

            if (!_context.Users.Any())
            {
                for (int i = 0; i < 5; i++)
                {
                    string userName = "testuser" + (i + 1);
                    string password = "Zaq123456789";
                    var result =
                        _userManager.CreateAsync(
                            new ApplicationUser {UserName = userName, Email = userName, EmailConfirmed = true},
                            password).Result;
                    if (result.Succeeded)
                    {
                        var user = _userManager.FindByEmailAsync(userName).Result;
                        var res = _userManager.AddToRoleAsync(user, "Administrator").Result;
                    }
                }
            }

            if (!_context.Assets.Any())
            {
                var usdAsset = new Asset
                {
                    Name = "USD",
                    ShortName = "USD",
                    Quantity = 0,                    
                    Price = 1
                };
                _context.Assets.Add(usdAsset);
                _context.SaveChanges();
            }
        }
    }
}
