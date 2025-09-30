using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using TaskManagerMVC.Repositories;
using TaskManagerShared.Models;

namespace TaskManagerMVC.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        private string GetToken() =>
            User.Claims.FirstOrDefault(c => c.Type == "JwtToken")?.Value ?? "";

        // ------------------ INDEX ------------------
        public async Task<IActionResult> Index(string? filter, string? status, string? category, string? priority,
                                              string? title, string? sortBy, int pageNumber = 1, int pageSize = 10)
        {
            var token = GetToken();
            var tasks = await _taskService.GetTasksAsync(filter, status, category, priority, title, sortBy, pageNumber, pageSize, token);

            ViewBag.Total = tasks.Count;
            ViewBag.Completed = tasks.Count(t => t.IsCompleted);
            ViewBag.SortBy = sortBy;
            ViewBag.PageNumber = pageNumber;
            ViewBag.PageSize = pageSize;

            return View(tasks);
        }

        // ------------------ CREATE ------------------
        public IActionResult Create() => View();

        [HttpPost]
        public async Task<IActionResult> Create(TodoTask task)
        {
            if (!ModelState.IsValid) return View(task);

            var token = GetToken();
            var success = await _taskService.CreateTaskAsync(task, token);
            if (!success) return StatusCode(500);

            return RedirectToAction(nameof(Index));
        }

        // ------------------ EDIT ------------------
        public async Task<IActionResult> Edit(int id)
        {
            var token = GetToken();
            var task = await _taskService.GetTaskByIdAsync(id, token);
            if (task == null) return NotFound();

            return View(task);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(TodoTask task)
        {
            if (!ModelState.IsValid) return View(task);

            var token = GetToken();
            var success = await _taskService.UpdateTaskAsync(task, token);
            if (!success) return StatusCode(500);

            return RedirectToAction(nameof(Index));
        }

        // ------------------ MARK COMPLETE ------------------
        [HttpPost]
        public async Task<IActionResult> MarkComplete(int id)
        {
            var token = GetToken();
            var success = await _taskService.MarkCompleteAsync(id, token);
            if (!success) return NotFound();

            return RedirectToAction(nameof(Index));
        }

        // ------------------ DELETE ------------------
        public async Task<IActionResult> Delete(int id)
        {
            var token = GetToken();
            var task = await _taskService.GetTaskByIdAsync(id, token);
            if (task == null) return NotFound();

            return View(task);
        }

        [HttpPost, ActionName("DeleteConfirmed")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var token = GetToken();
            var success = await _taskService.DeleteTaskAsync(id, token);

            if (!success)
                return RedirectToAction("AccessDenied", "Account");

            return RedirectToAction(nameof(Index));
        }
    }
}
