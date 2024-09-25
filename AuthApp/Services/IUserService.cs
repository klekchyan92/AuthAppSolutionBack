using AuthApp.Models;

namespace AuthApp.Services
{
    public interface IUserService
    {
        Task<User> ValidateUserAsync(string username, string password);
        Task<IEnumerable<User>> GetUsers();
        Task UpdateUserActivity(string username, bool isActive);
        Task CreateUser(User newUser);
    }
}
