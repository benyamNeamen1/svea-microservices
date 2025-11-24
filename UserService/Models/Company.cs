

namespace Svea.UserService.Models
{
    public class Company
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string? CompanyName { get; set; }
        public string? CompanyDomain { get; set; }
        public string? CompanyAddress { get; set; } 
        public string? CompanyCity { get; set; } 
        public string? CompanyCountry { get; set; } 
        public string? CompanyPostalCode { get; set; } 
        public string? CompanyMobileNumber { get; set; }
        public string? CompanyEmail { get; set; } 
        public string? CompanyLoggo { get; set; }
        public string? ManagerFirstName { get; set; }
        public string? ManagerLastName { get; set; } 
        public DateTime ManagerBirthDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
