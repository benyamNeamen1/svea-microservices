using Svea.UserService.Data;

namespace Svea.UserService.Services
{
    public class UserValidationService : IUserValidationService
    {
        private readonly UserDbContext _db;

        public UserValidationService(UserDbContext context)
        {
            _db = context;
        }

        public bool CheckUser(Guid userId, Guid companyId)
        {
            return _db.Users.Any(u =>
                u.Id == userId &&
                u.CompanyId == companyId 
            );
        }
    }
}
