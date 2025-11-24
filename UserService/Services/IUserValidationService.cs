

namespace Svea.UserService.Services
{
    public interface IUserValidationService
    {
        bool CheckUser(Guid userId, Guid companyId);
    }
}
