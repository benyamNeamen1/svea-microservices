
using Svea.UserService.Data;
using Svea.UserService.Models;
using System.Security.Cryptography;
using System.Text;
using Svea.UserService.Services;


public static class DataSeeder
{
    public static async Task SeedAsync(UserDbContext context)
    {
        
        await context.Database.EnsureCreatedAsync();

       
        Guid g=new Guid("ecfa2b93-8487-4713-b340-4878d9b8dc50");

        if (!context.Companies.Any())
        {
            Company comp = new Company();

            comp.CompanyName = "Svea Default Company";
            comp.CompanyDomain = "www.svea.se";
            comp.CompanyAddress = " ";
            comp.Id=g;
            context.Companies.Add(comp);
            await context.SaveChangesAsync();
        }

       
        if (!context.Users.Any())
        {

             
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes("Admin123"));

            var adminUser = new User
            {
                FirstName = "admin",
                Email = "admin@company.com",
                PasswordHash = hash,
                PasswordSalt =salt,
                CompanyId = context.Companies.First().Id
            };
            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }

    }
}
