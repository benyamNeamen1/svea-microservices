using System.ComponentModel;

namespace Svea.UserService.DTOs
{
    public class LoginDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
         [DefaultValue("ecfa2b93-8487-4713-b340-4878d9b8dc50")]
        public Guid CompanyId { get; set; }
    }
}
