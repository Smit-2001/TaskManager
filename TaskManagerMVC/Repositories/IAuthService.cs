using TaskManagerShared.Models;

namespace TaskManagerMVC.Repositories
{
    public interface IAuthService
    {
        Task<(bool Success, string ErrorMessage)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string Token, string ErrorMessage)> LoginAsync(LoginViewModel model);
    }
}
