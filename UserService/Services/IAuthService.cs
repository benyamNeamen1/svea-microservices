using Svea.UserService.DTOs;
using Svea.UserService.Models;

namespace Svea.UserService.Services
{
    public interface IAuthService
    {
        Task<Company> CreateCompanyAsync(RegisterCompanyDto dto);
        Task<User> CreateUserAsync(RegisterUserDto dto);
        Task<(string token, DateTime expires)> AuthenticateAsync(LoginDto dto);
    }
}
