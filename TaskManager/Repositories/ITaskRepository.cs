using TaskManagerShared.Models;

namespace TaskManagerAPI.Repositories
{
    public interface ITaskRepository
    {
        Task<IEnumerable<TodoTask>> GetTasksAsync(
            string? filter,
            string? status,
            string? category,
            string? priority,
            string? title,
            string? sortBy,
            int pageNumber,
            int pageSize);

        Task<TodoTask?> GetTaskByIdAsync(int id);
        Task<TodoTask> AddTaskAsync(TodoTask task);
        Task UpdateTaskAsync(TodoTask task);
        Task DeleteTaskAsync(int id);
    }
}
