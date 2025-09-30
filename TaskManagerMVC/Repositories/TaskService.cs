using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using TaskManagerShared.Models;

namespace TaskManagerMVC.Repositories
{
    public class TaskService : ITaskService
    {
        private readonly IHttpClientFactory _factory;
        private readonly IConfiguration _config;

        public TaskService(IHttpClientFactory factory, IConfiguration config)
        {
            _factory = factory;
            _config = config;
        }

        private HttpClient CreateClient(string token)
        {
            var client = _factory.CreateClient();
            client.BaseAddress = new Uri(_config["Api:BaseUrl"]!);
            if (!string.IsNullOrEmpty(token))
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return client;
        }

        public async Task<List<TodoTask>> GetTasksAsync(string? filter, string? status, string? category, string? priority,
                                                       string? title, string? sortBy, int pageNumber, int pageSize, string token)
        {
            var client = CreateClient(token);

            var qs = new List<string>();
            if (!string.IsNullOrWhiteSpace(filter)) qs.Add($"filter={Uri.EscapeDataString(filter)}");
            if (!string.IsNullOrWhiteSpace(status)) qs.Add($"status={Uri.EscapeDataString(status)}");
            if (!string.IsNullOrWhiteSpace(category)) qs.Add($"category={Uri.EscapeDataString(category)}");
            if (!string.IsNullOrWhiteSpace(priority)) qs.Add($"priority={Uri.EscapeDataString(priority)}");
            if (!string.IsNullOrWhiteSpace(title)) qs.Add($"title={Uri.EscapeDataString(title)}");
            if (!string.IsNullOrWhiteSpace(sortBy)) qs.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
            qs.Add($"pageNumber={pageNumber}");
            qs.Add($"pageSize={pageSize}");

            var q = string.Join("&", qs);
            var response = await client.GetAsync($"tasks?{q}");

            if (!response.IsSuccessStatusCode) return new List<TodoTask>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<TodoTask>>(json) ?? new List<TodoTask>();
        }

        public async Task<TodoTask?> GetTaskByIdAsync(int id, string token)
        {
            var client = CreateClient(token);
            var response = await client.GetAsync($"tasks/{id}");
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TodoTask>(json);
        }

        public async Task<bool> CreateTaskAsync(TodoTask task, string token)
        {
            var client = CreateClient(token);
            var json = JsonConvert.SerializeObject(task);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("tasks", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateTaskAsync(TodoTask task, string token)
        {
            var client = CreateClient(token);
            var json = JsonConvert.SerializeObject(task);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PutAsync($"tasks/{task.Id}", content);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> MarkCompleteAsync(int id, string token)
        {
            var client = CreateClient(token);

            var task = await GetTaskByIdAsync(id, token);
            if (task == null) return false;

            task.IsCompleted = true;
            return await UpdateTaskAsync(task, token);
        }

        public async Task<bool> DeleteTaskAsync(int id, string token)
        {
            var client = CreateClient(token);
            var response = await client.DeleteAsync($"tasks/{id}");
            return response.IsSuccessStatusCode;
        }
    }
}
