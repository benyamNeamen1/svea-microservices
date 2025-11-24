using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Svea.UserService.Data;
using Svea.UserService.DTOs;
using Svea.UserService.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Svea.UserService.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserDbContext _db;
        private readonly IConfiguration _cfg;

        public AuthService(UserDbContext db, IConfiguration cfg)
        {
            _db = db;
            _cfg = cfg;
        }

        public async Task<Company> CreateCompanyAsync(RegisterCompanyDto dto)
        {
            var company = new Company { CompanyName = dto.CompanyName, CompanyDomain = dto.CompanyDomain };
            _db.Companies.Add(company);
            await _db.SaveChangesAsync();
            return company;
        }

        public async Task<User> CreateUserAsync(RegisterUserDto dto)
        {
            var exists = await _db.Users.AnyAsync(u => u.Email == dto.Email && u.CompanyId == dto.CompanyId);
            if (exists) throw new InvalidOperationException("User already exists");

            CreatePasswordHash(dto.Password, out var hash, out var salt);

            var user = new User
            {
                CompanyId = dto.CompanyId,
                Email = dto.Email,
                PasswordHash = hash,
                PasswordSalt = salt,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
            return user;
        }

        public async Task<(string token, DateTime expires)> AuthenticateAsync(LoginDto dto)
        {

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email && u.CompanyId == dto.CompanyId);
            if (user == null) throw new InvalidOperationException("Invalid credentials");

            if (!VerifyPassword(dto.Password, user.PasswordHash, user.PasswordSalt))
                throw new InvalidOperationException("Invalid credentials");

            var token = GenerateToken(user);
            var expires = DateTime.UtcNow.AddHours(8);
            return (token, expires);
        }

        private void CreatePasswordHash(string password, out byte[] hash, out byte[] salt)
        {
            using var hmac = new HMACSHA512();
            salt = hmac.Key;
            hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
        }

        private bool VerifyPassword(string password, byte[] hash, byte[] salt)
        {
            using var hmac = new HMACSHA512(salt);
            var computed = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            if (computed.Length != hash.Length) return false;
            for (int i = 0; i < hash.Length; i++)
                if (computed[i] != hash[i]) return false;
            return true;
        }

        private string GenerateToken(User user)
        {
            var key = Encoding.UTF8.GetBytes(_cfg["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim("company_id", user.CompanyId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(8),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
