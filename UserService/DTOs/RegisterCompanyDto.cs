namespace Svea.UserService.DTOs
{
    public class RegisterCompanyDto
    {
        public string CompanyName { get; set; } = null!;
        public string? CompanyDomain { get; set; }
    }
}
