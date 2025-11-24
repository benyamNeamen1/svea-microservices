
namespace Svea.UserService.DTOs
{
    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public Guid CompanyId { get; set; }
    }
}

