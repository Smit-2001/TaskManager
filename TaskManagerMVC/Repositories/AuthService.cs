using System.Text.Json;
using System.Text;
using TaskManagerShared.Models;

namespace TaskManagerMVC.Repositories
{
    public class AuthService : IAuthService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public AuthService(IHttpClientFactory factory, IConfiguration config)
        {
            _factory = factory;
            _config = config;
        }

        public async Task<(bool Success, string ErrorMessage)> RegisterAsync(RegisterViewModel model)
        {
            var client = _factory.CreateClient();
            client.BaseAddress = new Uri(_config["Api:BaseUrl"]!);

            var payload = JsonSerializer.Serialize(model);
            var resp = await client.PostAsync("auth/register", new StringContent(payload, Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode)
            {
                return (false, await resp.Content.ReadAsStringAsync());
            }

            return (true, "");
        }

        public async Task<(bool Success, string Token, string ErrorMessage)> LoginAsync(LoginViewModel model)
        {
            var client = _factory.CreateClient();
            client.BaseAddress = new Uri(_config["Api:BaseUrl"]!);

            var payload = JsonSerializer.Serialize(model);
            var resp = await client.PostAsync("auth/login", new StringContent(payload, Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode)
            {
                return (false, "", "Invalid login attempt");
            }

            var json = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(json);
            var token = doc.RootElement.GetProperty("token").GetString();

            return (true, token ?? "", "");
        }
    }
}
