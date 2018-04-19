using System.Linq;
using Microsoft.AspNetCore.Identity;
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
        public async void Initialize()
        {
            //create database schema if none exists
            _context.Database.EnsureCreated();

            //If there is already an Administrator role, abort
            if (!_context.Roles.Any(r => r.Name == "Administrator"))
            {
                //Create the Administartor Role
                await _roleManager.CreateAsync(new IdentityRole("Administrator"));
            }

            if (!_context.Users.Any())
            {
                //Create the default Admin account and apply the Administrator role
                string userName = "videogamedonkey";
                string password = "thankyoudarksouls";
                var result = await _userManager.CreateAsync(new ApplicationUser { UserName = userName, Email = userName, EmailConfirmed = true }, password);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(userName);
                    await _userManager.AddToRoleAsync(user, "Administrator");
                }
            }
        }
    }
}
