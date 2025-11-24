using System;

namespace Svea.UserService.Models
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CompanyId { get; set; }
        public Company? Company { get; set; } 
        public byte[]? PasswordHash { get; set; } 
        public byte[]? PasswordSalt { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; } 
        public string? Address { get; set; } 
        public string? City { get; set; } 
        public string? Country { get; set; } 
        public string? PostalCode { get; set; }
        public string? MobileNumber { get; set; } 
        public string? Email { get; set; }
        public string? ProfilPic { get; set; } 
        public DateTime BirthDate { get; set; }
        public bool IsActive { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
