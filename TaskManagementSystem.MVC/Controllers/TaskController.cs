using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using TaskManagementSystem.MVC.Models;

namespace TaskManagementSystem.MVC.Controllers
{
    public class TaskController : Controller
    {
        private object _apiClientFactory;

        public IActionResult Index()
        {
            var emptyList = new List<TaskViewModel>(); // أو اجلبها من API لاحقًا
            return View(emptyList); // ✅ هذا يحل الخطأ
        }

        [HttpGet]
        //public async Task<IActionResult> Edit(int id)
        //{
        //    var client = _apiClientFactory.CreateClient();
        //    var resp = await client.GetAsync($"Task/{id}");
        //    if (!resp.IsSuccessStatusCode)
        //        return RedirectToAction("Index");

        //    var json = await resp.Content.ReadAsStringAsync();
        //    var model = JsonConvert.DeserializeObject<TaskViewModel>(json);

        //    PopulateDropdowns(model);

        //    // جلب الموظفين
        //    var resp2 = await client.GetAsync("User");
        //    if (resp2.IsSuccessStatusCode)
        //    {
        //        var usersJson = await resp2.Content.ReadAsStringAsync();
        //        var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
        //        model.Employees = users.Select(u => new SelectListItem
        //        {
        //            Value = u.Id.ToString(),
        //            Text = u.Name
        //        }).ToList();
        //    }
        //    else
        //    {
        //        model.Employees = new List<SelectListItem>();
        //    }

        //    return View(model);
        //}

        private void PopulateDropdowns(TaskViewModel model)
        {
            model.PriorityOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "Low", Text = "Low" },
        new SelectListItem { Value = "Medium", Text = "Medium" },
        new SelectListItem { Value = "High", Text = "High" }
    };

            model.StatusOptions = new List<SelectListItem>
    {
        new SelectListItem { Value = "Pending", Text = "Pending" },
        new SelectListItem { Value = "InProgress", Text = "In Progress" },
        new SelectListItem { Value = "Completed", Text = "Completed" },
        new SelectListItem { Value = "Overdue", Text = "Overdue" }
    };
        }
    }
    

}
