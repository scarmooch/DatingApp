using System.Collections.Generic;
using System.Linq;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;

namespace DatingApp.API.Data
{
    public class Seed
    {
        // S20.202 now using proper identity authentication and roles
        // public static void SeedUsers(DataContext context)
        // {
        //     // only add seed data if db empty
        //     if (!context.Users.Any())
        //     {
        //         var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
        //         var users = JsonConvert.DeserializeObject<List<User>>(userData);

        //         foreach (var user in users)
        //         {
        //             byte[] passwordHash, passwordSalt;
        //             // !!!!!! hardcoded password for seed data
        //             CreatePasswordHash("password", out passwordHash, out passwordSalt);

        //             // user.PasswordHash = passwordHash;
        //             // user.PasswordSalt = passwordSalt;
        //             user.UserName = user.UserName.ToLower();
        //             context.Users.Add(user);
        //         }

        //         context.SaveChanges();
        //     }
        // }

        // private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        // {
        //     using(var hmac = new System.Security.Cryptography.HMACSHA512())
        //     {
        //         passwordSalt = hmac.Key;
        //         passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
        //     }
        // }

        public static void SeedUsers(UserManager<User> userManager, RoleManager<Role> roleManager)
        {
             // only add seed data if db empty
            if (!userManager.Users.Any())
            {
                var userData = System.IO.File.ReadAllText("Data/UserSeedData.json");
                var users = JsonConvert.DeserializeObject<List<User>>(userData);

                // create some roles
                var roles = new List<Role>
                {
                    new Role{Name="Member"},
                    new Role{Name="Admin"},
                    new Role{Name="Moderator"},
                    new Role{Name="VIP"}
                };
                foreach (var role in roles)
                {
                    roleManager.CreateAsync(role).Wait();
                }
                foreach (var user in users)
                {
                    userManager.CreateAsync(user, "password").Wait();
                    userManager.AddToRoleAsync(user, "Member");
                }

                // create admin user
                var adminUser = new User
                {
                    UserName = "Admin"
                };
                var result = userManager.CreateAsync(adminUser, "password").Result;
                if(result.Succeeded)
                {
                    var admin = userManager.FindByNameAsync("Admin").Result;
                    userManager.AddToRolesAsync(admin, new[] {"Admin", "Moderator"});
                }
                
            }
       }

    }
}