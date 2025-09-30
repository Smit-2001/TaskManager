using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerShared.Models;

namespace TaskManagerAPI.Repositories
{
    public class TaskRepository : ITaskRepository
    {
        private readonly AppDbContext _context;

        public TaskRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<TodoTask>> GetTasksAsync(
    string? filter,
    string? status,
    string? category,
    string? priority,
    string? title,
    string? sortBy,
    int pageNumber,
    int pageSize)
        {
            var query = _context.Tasks.AsQueryable();

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.IsCompleted == (status == "Completed"));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(t => t.Category == category);

            if (!string.IsNullOrEmpty(priority))
                query = query.Where(t => t.Priority == priority);

            if (!string.IsNullOrEmpty(title))
                query = query.Where(t => t.Title.Contains(title));

            if (!string.IsNullOrEmpty(filter) && DateTime.TryParse(filter, out var dueDate))
                query = query.Where(t => t.DueDate.Date == dueDate.Date);

            query = sortBy switch
            {
                "duedate_asc" => query.OrderBy(t => t.DueDate),
                "duedate_desc" => query.OrderByDescending(t => t.DueDate),
                "status" => query.OrderBy(t => t.IsCompleted),
                "title_asc" => query.OrderBy(t => t.Title),
                "title_desc" => query.OrderByDescending(t => t.Title),
                "category_asc" => query.OrderBy(t => t.Category),
                "category_desc" => query.OrderByDescending(t => t.Category),

                "priority_asc" => query.OrderBy(t =>
                    t.Priority == "Low" ? 1 :
                    t.Priority == "Medium" ? 2 : 3),
                "priority_desc" => query.OrderByDescending(t =>
                    t.Priority == "Low" ? 1 :
                    t.Priority == "Medium" ? 2 : 3),

                _ => query.OrderBy(t => t.Priority == "High" ? 1 :
                                        t.Priority == "Medium" ? 2 : 3)
                          .ThenBy(t => t.DueDate)
            };

            return await query.Skip((pageNumber - 1) * pageSize)
                              .Take(pageSize)
                              .ToListAsync();
        }


        public async Task<TodoTask?> GetTaskByIdAsync(int id)
        {
            return await _context.Tasks.FindAsync(id);
        }

        public async Task<TodoTask> AddTaskAsync(TodoTask task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return task;
        }

        public async Task UpdateTaskAsync(TodoTask task)
        {
            _context.Entry(task).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTaskAsync(int id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                _context.Tasks.Remove(task);
                await _context.SaveChangesAsync();
            }
        }
    }
}
