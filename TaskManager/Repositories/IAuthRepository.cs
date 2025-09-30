using TaskManagerShared.Models;

namespace TaskManagerAPI.Repositories
{
    public interface IAuthRepository
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string Token)> LoginAsync(LoginViewModel model);
    }
}
