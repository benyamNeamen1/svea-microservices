using Microsoft.AspNetCore.Mvc;
using Svea.UserService.DTOs;
using Svea.UserService.Services;

namespace Svea.UserService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _svc;
        public AuthController(IAuthService svc) => _svc = svc;

        [HttpPost("companies")]
        public async Task<IActionResult> RegisterCompany(RegisterCompanyDto dto)
        {
            var c = await _svc.CreateCompanyAsync(dto);
            return CreatedAtAction(null, new { id = c.Id }, c);
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser(RegisterUserDto dto)
        {
            try
            {
                var u = await _svc.CreateUserAsync(dto);

                var loginDto = new LoginDto
                {
                    Email = dto.Email,
                    Password = dto.Password,
                    CompanyId= dto.CompanyId
                };

                var (token, expires) = await _svc.AuthenticateAsync(loginDto);

                return Ok(new LoginResponseDto { Token = token, ExpiresAt = expires });

                //return CreatedAtAction(null, new { id = u.Id }, u);
            }
            catch (InvalidOperationException ex) { return BadRequest(new { error = ex.Message }); }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            try
            {
                var (token, expires) = await _svc.AuthenticateAsync(dto);
                return Ok(new LoginResponseDto { Token = token, ExpiresAt = expires });
            }
            catch (InvalidOperationException ex) { return Unauthorized(new { error = ex.Message }); }
        }
    }
}
