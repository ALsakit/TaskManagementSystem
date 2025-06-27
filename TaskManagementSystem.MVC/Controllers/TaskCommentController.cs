using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using TaskManagementSystem.MVC.Helpers;
using TaskManagementSystem.MVC.Models;

namespace TaskManagementSystem.MVC.Controllers
{
    public class TaskCommentController : Controller
    {
        private readonly ApiClientFactory _apiClientFactory;
        private readonly ILogger<TaskController> _logger;

        public TaskCommentController(ApiClientFactory apiClientFactory, ILogger<TaskController> logger)
        {
            _apiClientFactory = apiClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var client = _apiClientFactory.CreateClient();
            var response = await client.GetAsync("Task");
            if (!response.IsSuccessStatusCode)
                return View(new List<TaskViewModel>());

            var json = await response.Content.ReadAsStringAsync();
            var tasks = JsonConvert.DeserializeObject<List<TaskViewModel>>(json);
            return View(tasks);
        }
    }
}
