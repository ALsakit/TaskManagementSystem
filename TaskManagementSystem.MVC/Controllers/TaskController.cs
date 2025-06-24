using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using TaskManagementSystem.MVC.Helpers;
using TaskManagementSystem.MVC.Models;

namespace TaskManagementSystem.MVC.Controllers
{
    public class TaskController : Controller
    {
        private readonly ApiClientFactory _apiClientFactory;
        private readonly ILogger<TaskController> _logger;

        public TaskController(ApiClientFactory apiClientFactory, ILogger<TaskController> logger)
        {
            _apiClientFactory = apiClientFactory;
            _logger = logger;
        }

        [RequireLogin]
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

        /// <summary>
        /// جلب قائمة الموظفين للنموذج المنبثق
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            try
            {
                var client = _apiClientFactory.CreateClient();
                var response = await client.GetAsync("User");

                if (response.IsSuccessStatusCode)
                {
                    var usersJson = await response.Content.ReadAsStringAsync();
                    var users = JsonConvert.DeserializeObject<List<User>>(usersJson);

                    var employees = users.Select(u => new
                    {
                        value = u.Id.ToString(),
                        text = u.Name,
                        role = u.Role ?? "Employee" // إضافة الدور إذا كان متوفراً
                    }).ToList();

                    return Json(new { success = true, data = employees });
                }
                else
                {
                    _logger.LogError("فشل في جلب قائمة الموظفين من API. Status Code: {StatusCode}", response.StatusCode);
                    return Json(new { success = false, message = "فشل في جلب قائمة الموظفين" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في جلب قائمة الموظفين");
                return Json(new { success = false, message = "حدث خطأ أثناء جلب قائمة الموظفين" });
            }
        }

        /// <summary>
        /// جلب بيانات مهمة محددة للتعديل
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetTask(int id)
        {
            try
            {
                var client = _apiClientFactory.CreateClient();
                var response = await client.GetAsync($"Task/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var task = JsonConvert.DeserializeObject<TaskViewModel>(json);

                    return Json(new { success = true, data = task });
                }
                else
                {
                    _logger.LogError("فشل في جلب بيانات المهمة {TaskId} من API. Status Code: {StatusCode}", id, response.StatusCode);
                    return Json(new { success = false, message = "فشل في جلب بيانات المهمة" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في جلب بيانات المهمة {TaskId}", id);
                return Json(new { success = false, message = "حدث خطأ أثناء جلب بيانات المهمة" });
            }
        }

        /// <summary>
        /// إنشاء مهمة جديدة عبر AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjax([FromBody] TaskCreateModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return Json(new { success = false, errors = errors });
                }

                var client = _apiClientFactory.CreateClient();
                var toSend = new
                {
                    Title = model.Title,
                    Description = model.Description,
                    Priority = model.Priority,
                    System = model.System,
                    DueDate = model.DueDate,
                    AssignedToUserId = model.AssignedToUserId
                };

                var json = JsonConvert.SerializeObject(toSend);
                _logger.LogInformation("إنشاء مهمة جديدة: {TaskData}", json);

                var response = await client.PostAsync("Task", new StringContent(json, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "تم إنشاء المهمة بنجاح" });
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("فشل في إنشاء المهمة. Status Code: {StatusCode}, Response: {Response}", response.StatusCode, errorBody);
                    return Json(new { success = false, message = $"فشل في إنشاء المهمة: {errorBody}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في إنشاء المهمة");
                return Json(new { success = false, message = "حدث خطأ أثناء إنشاء المهمة" });
            }
        }

        /// <summary>
        /// تعديل مهمة عبر AJAX
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAjax(int id, [FromBody] TaskEditModel model)
        {
            try
            {
                if (id != model.Id)
                {
                    return Json(new { success = false, message = "معرف المهمة غير صحيح" });
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .ToDictionary(
                            kvp => kvp.Key,
                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                        );

                    return Json(new { success = false, errors = errors });
                }

                var client = _apiClientFactory.CreateClient();
                var toSend = new
                {
                    Id = model.Id,
                    Title = model.Title,
                    Description = model.Description,
                    Priority = model.Priority,
                    System = model.System,
                    DueDate = model.DueDate,
                    AssignedToUserId = model.AssignedToUserId,
                    Status = model.Status
                };

                var json = JsonConvert.SerializeObject(toSend);
                _logger.LogInformation("تعديل المهمة {TaskId}: {TaskData}", id, json);

                var response = await client.PutAsync($"Task/{id}", new StringContent(json, Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "تم تعديل المهمة بنجاح" });
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("فشل في تعديل المهمة {TaskId}. Status Code: {StatusCode}, Response: {Response}", id, response.StatusCode, errorBody);
                    return Json(new { success = false, message = $"فشل في تعديل المهمة: {errorBody}" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في تعديل المهمة {TaskId}", id);
                return Json(new { success = false, message = "حدث خطأ أثناء تعديل المهمة" });
            }
        }

        /// <summary>
        /// حذف مهمة
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var client = _apiClientFactory.CreateClient();
                var response = await client.DeleteAsync($"Task/{id}");

                if (response.IsSuccessStatusCode)
                {
                    return Json(new { success = true, message = "تم حذف المهمة بنجاح" });
                }
                else
                {
                    var errorBody = await response.Content.ReadAsStringAsync();
                    _logger.LogError("فشل في حذف المهمة {TaskId}. Status Code: {StatusCode}, Response: {Response}", id, response.StatusCode, errorBody);
                    return Json(new { success = false, message = "فشل في حذف المهمة" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطأ في حذف المهمة {TaskId}", id);
                return Json(new { success = false, message = "حدث خطأ أثناء حذف المهمة" });
            }
        }

        // الطرق الأصلية للواجهات المنفصلة (للتوافق مع النظام الحالي)
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var vm = new TaskViewModel();
            await RefillEmployees(vm);
            vm.PriorityOptions = new List<SelectListItem> {
                new SelectListItem("Low","Low"),
                new SelectListItem("Medium","Medium"),
                new SelectListItem("High","High")
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TaskViewModel model)
        {
            var client = _apiClientFactory.CreateClient();
            var toSend = new
            {
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                System = model.System,
                DueDate = model.DueDate,
                AssignedToUserId = model.AssignedToUserId
            };
            var json = JsonConvert.SerializeObject(toSend);
            _logger.LogInformation("► HttpClient BaseAddress: {BaseAddress}", client.BaseAddress);
            _logger?.LogInformation("Sending Task JSON: {json}", json);

            var response = await client.PostAsync("Task", new StringContent(json, Encoding.UTF8, "application/json"));
            _logger.LogInformation("API returned status code {StatusCode}", response.StatusCode);
            var body = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("API response body: {Body}", body);

            if (!response.IsSuccessStatusCode)
            {
                var err = body;
                ModelState.AddModelError("", $"خطأ من API: {response.StatusCode}. {err}");
                await RefillEmployees(model);
                model.PriorityOptions = new List<SelectListItem> {
                    new SelectListItem("Low","Low"),
                    new SelectListItem("Medium","Medium"),
                    new SelectListItem("High","High")
                };
                return View(model);
            }
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var client = _apiClientFactory.CreateClient();
            var resp = await client.GetAsync($"Task/{id}");
            if (!resp.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var json = await resp.Content.ReadAsStringAsync();
            var model = JsonConvert.DeserializeObject<TaskViewModel>(json);

            await RefillEmployees(model);
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

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, TaskViewModel model)
        {
            if (!ModelState.IsValid && false)
            {
                await RefillEmployees(model);
                model.PriorityOptions = new List<SelectListItem> {
                    new SelectListItem("Low", "Low"),
                    new SelectListItem("Medium", "Medium"),
                    new SelectListItem("High", "High")
                };
                model.StatusOptions = new List<SelectListItem> {
                    new SelectListItem("Pending", "Pending"),
                    new SelectListItem("InProgress", "InProgress"),
                    new SelectListItem("Completed", "Completed"),
                    new SelectListItem("Overdue", "Overdue")
                };
                return View(model);
            }

            var client = _apiClientFactory.CreateClient();
            var toSend = new
            {
                Id = model.Id,
                Title = model.Title,
                Description = model.Description,
                Priority = model.Priority,
                System = model.System,
                DueDate = model.DueDate,
                AssignedToUserId = model.AssignedToUserId,
                Status = model.Status
            };

            var json = JsonConvert.SerializeObject(toSend);
            var resp = await client.PutAsync($"Task/{model.Id}", new StringContent(json, Encoding.UTF8, "application/json"));

            if (!resp.IsSuccessStatusCode)
            {
                await RefillEmployees(model);
                return View(model);
            }

            TempData["SuccessMessage"] = "تم تعديل المهمة بنجاح.";
            return RedirectToAction("Index");
        }

        private async Task RefillEmployees(TaskViewModel vm)
        {
            var client = _apiClientFactory.CreateClient();
            var resp = await client.GetAsync("User");
            if (resp.IsSuccessStatusCode)
            {
                var usersJson = await resp.Content.ReadAsStringAsync();
                var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
                vm.Employees = users.Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Name
                }).ToList();
            }
            else
            {
                vm.Employees = new List<SelectListItem>();
            }
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var client = _apiClientFactory.CreateClient();

            // 1) جلب بيانات المهمة
            var respTask = await client.GetAsync($"Task/{id}");
            if (!respTask.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var taskJson = await respTask.Content.ReadAsStringAsync();
            var task = JsonConvert.DeserializeObject<TaskViewModel>(taskJson);

            // 2) جلب التعليقات
            var respComments = await client.GetAsync($"Task/{id}/comments");
            List<TaskCommentDto> comments = new();
            if (respComments.IsSuccessStatusCode)
            {
                var commJson = await respComments.Content.ReadAsStringAsync();
                comments = JsonConvert.DeserializeObject<List<TaskCommentDto>>(commJson);
            }

            // 3) استخراج UserId من الجلسة
            int currentUserId = 0;
            if (HttpContext.Session.GetString("UserId") != null)
                currentUserId = int.Parse(HttpContext.Session.GetString("UserId"));

            var vm = new TaskDetailsViewModel
            {
                Task = task,
                Comments = comments,
                CurrentUserId = currentUserId // أضف هذا
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([FromBody] CommentViewModel model)
        {
            var token = HttpContext.Session.GetString("JWToken");
            var userIdString = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userIdString))
                return Unauthorized();

            model.UserId = int.Parse(userIdString); // تأكد أنك خزّنت UserId في الجلسة عند تسجيل الدخول

            var client = _apiClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("Task/AddComment", content);

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, "تعذر إرسال التعليق");

            var responseBody = await response.Content.ReadAsStringAsync();
            return Content(responseBody, "application/json");
        }
        public class CommentViewModel
        {
            public int TaskId { get; set; }
            public string CommentText { get; set; }
            public int UserId { get; set; }
        }

    }



    // نماذج البيانات للـ AJAX
    public class TaskCreateModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string System { get; set; }
        public DateTime? DueDate { get; set; }
        public int AssignedToUserId { get; set; }
    }

    public class TaskEditModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Priority { get; set; }
        public string System { get; set; }
        public DateTime? DueDate { get; set; }
        public int AssignedToUserId { get; set; }
        public string Status { get; set; }
    }
}

