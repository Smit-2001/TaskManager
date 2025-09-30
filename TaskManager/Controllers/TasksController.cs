using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.Repositories;
using TaskManagerShared.Models;

namespace TaskManagerAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskRepository _repo;

        public TasksController(ITaskRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<IEnumerable<TodoTask>>> GetTasks(
            string? filter,
            string? status,
            string? category,
            string? priority,
            string? title,
            string? sortBy,
            int pageNumber = 1,
            int pageSize = 10)
        {
            var tasks = await _repo.GetTasksAsync(filter, status, category, priority, title, sortBy, pageNumber, pageSize);
            return Ok(tasks);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<TodoTask>> GetTask(int id)
        {
            var task = await _repo.GetTaskByIdAsync(id);
            if (task == null) return NotFound();
            return Ok(task);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,User")]
        public async Task<ActionResult<TodoTask>> CreateTask(TodoTask task)
        {
            var created = await _repo.AddTaskAsync(task);
            return CreatedAtAction(nameof(GetTask), new { id = created.Id }, created);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> UpdateTask(int id, TodoTask task)
        {
            if (id != task.Id) return BadRequest();
            await _repo.UpdateTaskAsync(task);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            await _repo.DeleteTaskAsync(id);
            return NoContent();
        }
    }
}
