using TaskManagerShared.Models;

namespace TaskManagerMVC.Repositories
{
    public interface ITaskService
    {
        Task<List<TodoTask>> GetTasksAsync(string? filter, string? status, string? category, string? priority,
                                           string? title, string? sortBy, int pageNumber, int pageSize, string token);

        Task<TodoTask?> GetTaskByIdAsync(int id, string token);
        Task<bool> CreateTaskAsync(TodoTask task, string token);
        Task<bool> UpdateTaskAsync(TodoTask task, string token);
        Task<bool> MarkCompleteAsync(int id, string token);
        Task<bool> DeleteTaskAsync(int id, string token);
    }
}
