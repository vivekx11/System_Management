// seeddata
using CollegeEventPortal.Models;
using Microsoft.AspNetCore.Identity;

namespace CollegeEventPortal.Data
{
    public static class SeedData
    {
        public static async Task Initialize(IServiceProvider serviceProvider,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            string[] roleNames = { "Admin", "Student", "Judge", "TeamLeader" };

            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // Create admin user
            var adminEmail = "admin@college.edu";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "System Administrator",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Create sample judge
            var judgeEmail = "judge@college.edu";
            var judgeUser = await userManager.FindByEmailAsync(judgeEmail);

            if (judgeUser == null)
            {
                judgeUser = new ApplicationUser
                {
                    UserName = judgeEmail,
                    Email = judgeEmail,
                    FullName = "Dr. John Judge",
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(judgeUser, "Judge@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(judgeUser, "Judge");
                }
            }

            // Create sample student
            var studentEmail = "student@college.edu";
            var studentUser = await userManager.FindByEmailAsync(studentEmail);
            // student data 

            if (studentUser == null)
            {
                studentUser = new ApplicationUser
                {
                    UserName = studentEmail,
                    Email = studentEmail,
                    FullName = "John Student",
                    RollNumber = "CS2024001",
                    Department = "Computer Science",
                    Year = 3,
                    EmailConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(studentUser, "Student@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(studentUser, "Student");
                }
            }
        }
    }
}
