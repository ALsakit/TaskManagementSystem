
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.AspNetCore.Mvc.Rendering;
//using Newtonsoft.Json;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Http;
//using System.Text;
//using System.Threading.Tasks;
//using TaskManagementSystem.MVC.Helpers;
//using TaskManagementSystem.MVC.Models;

//namespace TaskManagementSystem.MVC.Controllers
//{
//    public class TaskController : Controller
//    {
//        private readonly ApiClientFactory _apiClientFactory;
//        private readonly ILogger<TaskController> _logger;
//        public TaskController(ApiClientFactory apiClientFactory, ILogger<TaskController> logger)
//        {
//            _apiClientFactory = apiClientFactory;
//            _logger = logger;
//        }
//        [RequireLogin]
//        public async Task<IActionResult> Index()
//        {
//            var client = _apiClientFactory.CreateClient();
//            var response = await client.GetAsync("Task");
//            if (!response.IsSuccessStatusCode)
//                return View(new List<TaskViewModel>());

//            var json = await response.Content.ReadAsStringAsync();
//            var tasks = JsonConvert.DeserializeObject<List<TaskViewModel>>(json);
//            return View(tasks);
//        }

//        [HttpGet]
//        public async Task<IActionResult> Create()
//        {
//            var vm = new TaskViewModel();
//            // جلب قائمة الموظفين
//            await RefillEmployees(vm);
//            // إعداد خيارات الأولوية والحالة إن أردت
//            vm.PriorityOptions = new List<SelectListItem> {
//        new SelectListItem("Low","Low"),
//        new SelectListItem("Medium","Medium"),
//        new SelectListItem("High","High")
//    };
//            return View(vm);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(TaskViewModel model)
//        {
//            //    if (!ModelState.IsValid)
//            //    {
//            //        await RefillEmployees(model);
//            //        model.PriorityOptions = new List<SelectListItem> {
//            //    new SelectListItem("Low","Low"),
//            //    new SelectListItem("Medium","Medium"),
//            //    new SelectListItem("High","High")
//            //};
//            //        return View(model);
//            //    }

//            var client = _apiClientFactory.CreateClient();
//            var toSend = new
//            {
//                Title = model.Title,
//                Description = model.Description,
//                Priority = model.Priority,
//                System = model.System,
//                DueDate = model.DueDate,
//                AssignedToUserId = model.AssignedToUserId
//            };
//            var json = JsonConvert.SerializeObject(toSend);
//            _logger.LogInformation("► HttpClient BaseAddress: {BaseAddress}", client.BaseAddress);

//            _logger?.LogInformation("Sending Task JSON: {json}", json);
//            var response = await client.PostAsync("Task", new StringContent(json, Encoding.UTF8, "application/json"));
//            _logger.LogInformation("API returned status code {StatusCode}", response.StatusCode);
//            var body = await response.Content.ReadAsStringAsync();
//            _logger.LogInformation("API response body: {Body}", body);

//            if (!response.IsSuccessStatusCode)
//            {
//                var err = body; // استخدم ما قرأته أعلاه
//                ModelState.AddModelError("", $"خطأ من API: {response.StatusCode}. {err}");
//                await RefillEmployees(model);
//                model.PriorityOptions = new List<SelectListItem> {
//            new SelectListItem("Low","Low"),
//            new SelectListItem("Medium","Medium"),
//            new SelectListItem("High","High")
//        };
//                return View(model);
//            }
//            return RedirectToAction("Index");
//        }

//        private async Task RefillEmployees(TaskViewModel vm)
//        {
//            var client = _apiClientFactory.CreateClient();
//            var resp = await client.GetAsync("User");
//            if (resp.IsSuccessStatusCode)
//            {
//                var usersJson = await resp.Content.ReadAsStringAsync();
//                var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
//                vm.Employees = users.Select(u => new SelectListItem
//                {
//                    Value = u.Id.ToString(),
//                    Text = u.Name
//                }).ToList();
//            }
//            else
//            {
//                vm.Employees = new List<SelectListItem>();
//            }
//        }


//        //[HttpGet]
//        //public async Task<IActionResult> Create()
//        //{
//        //    var vm = new TaskViewModel();
//        //    // جلب الموظفين
//        //    var client = _apiClientFactory.CreateClient();
//        //    var resp = await client.GetAsync("User");
//        //    if (resp.IsSuccessStatusCode)
//        //    {
//        //        var usersJson = await resp.Content.ReadAsStringAsync();
//        //        var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
//        //        vm.Employees = users.Select(u => new SelectListItem
//        //        {
//        //            Value = u.Id.ToString(),
//        //            Text = u.Name
//        //        }).ToList();
//        //    }
//        //    else
//        //    {
//        //        vm.Employees = new List<SelectListItem>();
//        //    }
//        //    return View(vm);
//        //}

//        //[HttpPost]
//        //public async Task<IActionResult> Create(TaskViewModel model)
//        //{
//        //    if (!ModelState.IsValid)
//        //    {
//        //        // أعد جلب الموظفين
//        //        var client2 = _apiClientFactory.CreateClient();
//        //        var resp2 = await client2.GetAsync("User");
//        //        if (resp2.IsSuccessStatusCode)
//        //        {
//        //            var usersJson = await resp2.Content.ReadAsStringAsync();
//        //            var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
//        //            model.Employees = users.Select(u => new SelectListItem
//        //            {
//        //                Value = u.Id.ToString(),
//        //                Text = u.Name
//        //            }).ToList();
//        //        }
//        //        return View(model);
//        //    }

//        //    var client = _apiClientFactory.CreateClient();
//        //    // جهّز بيانات الإرسال: كائن TaskItem JSON
//        //    var toSend = new TaskItemForPost
//        //    {
//        //        Title = model.Title,
//        //        Description = model.Description,
//        //        Priority = model.Priority,
//        //        System = model.System,
//        //        DueDate = model.DueDate,
//        //        AssignedToUserId = model.AssignedToUserId
//        //    };
//        //    var json = JsonConvert.SerializeObject(toSend);
//        //    var content = new StringContent(json, Encoding.UTF8, "application/json");
//        //    var response = await client.PostAsync("Task", content);
//        //    if (response.IsSuccessStatusCode)
//        //        return RedirectToAction("Index");

//        //    ModelState.AddModelError("", "حدث خطأ أثناء إنشاء المهمة.");
//        //    // أعد جلب الموظفين
//        //    var resp3 = await client.GetAsync("User");
//        //    if (resp3.IsSuccessStatusCode)
//        //    {
//        //        var usersJson = await resp3.Content.ReadAsStringAsync();
//        //        var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
//        //        model.Employees = users.Select(u => new SelectListItem
//        //        {
//        //            Value = u.Id.ToString(),
//        //            Text = u.Name
//        //        }).ToList();
//        //    }
//        //    return View(model);
//        //}

//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
//        {
//            var client = _apiClientFactory.CreateClient();
//            var resp = await client.GetAsync($"Task/{id}");
//            if (!resp.IsSuccessStatusCode)
//                return RedirectToAction("Index");

//            var json = await resp.Content.ReadAsStringAsync();
//            var model = JsonConvert.DeserializeObject<TaskViewModel>(json);

//            // جلب الموظفين
//            var resp2 = await client.GetAsync("User");
//            if (resp2.IsSuccessStatusCode)
//            {
//                var usersJson = await resp2.Content.ReadAsStringAsync();
//                var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
//                model.Employees = users.Select(u => new SelectListItem
//                {
//                    Value = u.Id.ToString(),
//                    Text = u.Name
//                }).ToList();
//            }
//            else
//            {
//                model.Employees = new List<SelectListItem>();
//            }
//            model.PriorityOptions = new List<SelectListItem>
//{
//    new SelectListItem { Value = "Low", Text = "Low" },
//    new SelectListItem { Value = "Medium", Text = "Medium" },
//    new SelectListItem { Value = "High", Text = "High" }
//};

//            model.StatusOptions = new List<SelectListItem>
//{
//    new SelectListItem { Value = "Pending", Text = "Pending" },
//    new SelectListItem { Value = "InProgress", Text = "In Progress" },
//    new SelectListItem { Value = "Completed", Text = "Completed" },
//    new SelectListItem { Value = "Overdue", Text = "Overdue" }
//};

//            return View(model);
//        }
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, TaskViewModel model)
//        {
//            if (!ModelState.IsValid && false)
//            {
//                await RefillEmployees(model); // إعادة تعبئة القائمة
//                model.PriorityOptions = new List<SelectListItem> {
//            new SelectListItem("Low", "Low"),
//            new SelectListItem("Medium", "Medium"),
//            new SelectListItem("High", "High")
//        };
//                model.StatusOptions = new List<SelectListItem> {
//            new SelectListItem("Pending", "Pending"),
//            new SelectListItem("InProgress", "InProgress"),
//            new SelectListItem("Completed", "Completed"),
//            new SelectListItem("Overdue", "Overdue")
//        };
//                return View(model);
//            }

//            var client = _apiClientFactory.CreateClient();
//            var toSend = new
//            {
//                Id = model.Id,
//                Title = model.Title,
//                Description = model.Description,
//                Priority = model.Priority,
//                System = model.System,
//                DueDate = model.DueDate,
//                AssignedToUserId = model.AssignedToUserId,
//                Status = model.Status
//            };

//            var json = JsonConvert.SerializeObject(toSend);
//            var resp = await client.PutAsync($"Task/{model.Id}", new StringContent(json, Encoding.UTF8, "application/json"));

//            if (!resp.IsSuccessStatusCode)
//            {
//                await RefillEmployees(model);
//                return View(model);
//            }

//            TempData["SuccessMessage"] = "تم تعديل المهمة بنجاح.";
//            return RedirectToAction("Index");
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit111(int id, TaskViewModel model)
//        {
//            if (!ModelState.IsValid && false)
//            {
//                await PopulateDropdowns(model);
//                return View(model);
//            }

//            var client = _apiClientFactory.CreateClient();

//            var toSend = new
//            {
//                Id = model.Id,
//                Title = model.Title,
//                Description = model.Description,
//                Priority = model.Priority,
//                System = model.System,
//                DueDate = model.DueDate,
//                AssignedToUserId = model.AssignedToUserId,
//                Status = model.Status
//            };

//            var json = JsonConvert.SerializeObject(toSend);

//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            // إرسال الطلب إلى API
//            var response = await client.PutAsync($"Task/{id}", content);

//            if (!response.IsSuccessStatusCode)
//            {
//                var errBody = await response.Content.ReadAsStringAsync();
//                ModelState.AddModelError("", $"خطأ في التعديل: {errBody}");
//                await PopulateDropdowns(model);
//                return View(model);
//            }

//            TempData["SuccessMessage"] = "تم تعديل المهمة بنجاح.";

//            // إعادة التوجيه بعد النجاح (أفضل من البقاء في نفس الصفحة)
//            return RedirectToAction("Index");
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit11(int id, TaskViewModel model)
//        {
//            if (!ModelState.IsValid && false)
//            {
//                await PopulateDropdowns(model); // تعبئة القوائم
//                return View(model);
//            }

//            var client = _apiClientFactory.CreateClient();

//            var toSend = new
//            {
//                Id = model.Id,
//                Title = model.Title,
//                Description = model.Description,
//                Priority = model.Priority,
//                System = model.System,
//                DueDate = model.DueDate,
//                AssignedToUserId = model.AssignedToUserId,
//                Status = model.Status
//            };

//            var json = JsonConvert.SerializeObject(toSend);
//            var response = await client.PutAsync($"Task/{id}", new StringContent(json, Encoding.UTF8, "application/json"));

//            if (!response.IsSuccessStatusCode)
//            {
//                var errBody = await response.Content.ReadAsStringAsync();
//                ModelState.AddModelError("", $"خطأ في التعديل: {errBody}");
//                await PopulateDropdowns(model);
//                return View(model);
//            }

//            TempData["SuccessMessage"] = "تم تعديل المهمة بنجاح.";

//            return View(model);

//            //return RedirectToAction("Index");
//        }

//        private async Task PopulateDropdowns(TaskViewModel model)
//        {
//            var client = _apiClientFactory.CreateClient();
//            var resp = await client.GetAsync("User");

//            if (resp.IsSuccessStatusCode)
//            {
//                var usersJson = await resp.Content.ReadAsStringAsync();
//                var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
//                model.Employees = users.Select(u => new SelectListItem
//                {
//                    Value = u.Id.ToString(),
//                    Text = u.Name
//                }).ToList();
//            }
//            else
//            {
//                model.Employees = new List<SelectListItem>();
//            }

//            model.PriorityOptions = new List<SelectListItem>
//            {
//                new SelectListItem("Low", "Low"),
//                new SelectListItem("Medium", "Medium"),
//                new SelectListItem("High", "High")
//            };

//            model.StatusOptions = new List<SelectListItem>
//            {
//                new SelectListItem("Pending", "Pending"),
//                new SelectListItem("InProgress", "In Progress"),
//                new SelectListItem("Completed", "Completed"),
//                new SelectListItem("Overdue", "Overdue")
//            };
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit11(TaskViewModel model)
//        {
//            if (!ModelState.IsValid)
//            {
//                var client0 = _apiClientFactory.CreateClient();
//                var resp0 = await client0.GetAsync("User");
//                if (resp0.IsSuccessStatusCode)
//                {
//                    var usersJson = await resp0.Content.ReadAsStringAsync();
//                    var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
//                    model.Employees = users.Select(u => new SelectListItem
//                    {
//                        Value = u.Id.ToString(),
//                        Text = u.Name
//                    }).ToList();
//                }
//                model.PriorityOptions = new List<SelectListItem>
//                {
//                    new SelectListItem("Low", "Low"),
//                    new SelectListItem("Medium", "Medium"),
//                    new SelectListItem("High", "High")
//                };
//                model.StatusOptions = new List<SelectListItem>
//                {
//                    new SelectListItem("Pending", "Pending"),
//                    new SelectListItem("InProgress", "In Progress"),
//                    new SelectListItem("Completed", "Completed"),
//                    new SelectListItem("Overdue", "Overdue")
//                };
//                return View(model);
//            }

//            var client = _apiClientFactory.CreateClient();

//            var updateModel = new UpdateTaskModel
//            {
//                Id = model.Id,
//                Title = model.Title,
//                Description = model.Description,
//                Priority = model.Priority,
//                System = model.System,
//                DueDate = model.DueDate,
//                AssignedToUserId = model.AssignedToUserId,
//                Status = model.Status
//            };

//            var json = JsonConvert.SerializeObject(updateModel);
//            var content = new StringContent(json, Encoding.UTF8, "application/json");

//            _logger?.LogInformation("Sending Edit JSON: {json}", json);

//            var response = await client.PutAsync($"Task/{model.Id}", content);
//            _logger?.LogInformation("API Edit returned: {StatusCode}", response.StatusCode);

//            if (response.IsSuccessStatusCode)
//                return RedirectToAction("Index");

//            var errBody = await response.Content.ReadAsStringAsync();
//            _logger?.LogWarning("API Edit error: {Body}", errBody);
//            ModelState.AddModelError("", $"حدث خطأ أثناء حفظ التعديلات: {response.StatusCode}. {errBody}");

//            var resp2 = await client.GetAsync("User");
//            if (resp2.IsSuccessStatusCode)
//            {
//                var usersJson = await resp2.Content.ReadAsStringAsync();
//                var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
//                model.Employees = users.Select(u => new SelectListItem
//                {
//                    Value = u.Id.ToString(),
//                    Text = u.Name
//                }).ToList();
//            }
//            model.PriorityOptions = new List<SelectListItem>
//    {
//        new SelectListItem("Low", "Low"),
//        new SelectListItem("Medium", "Medium"),
//        new SelectListItem("High", "High")
//    };
//            model.StatusOptions = new List<SelectListItem>
//    {
//        new SelectListItem("Pending", "Pending"),
//        new SelectListItem("InProgress", "In Progress"),
//        new SelectListItem("Completed", "Completed"),
//        new SelectListItem("Overdue", "Overdue")
//    };
//            return View(model);
//        }

//        // GET: /Task/Notifications
        
//        [HttpGet]
//        public async Task<IActionResult> Details(int id)
//        {
//            var client = _apiClientFactory.CreateClient();

//            // 1) جلب بيانات المهمة
//            var respTask = await client.GetAsync($"Task/{id}");
//            if (!respTask.IsSuccessStatusCode)
//                return RedirectToAction("Index");

//            var taskJson = await respTask.Content.ReadAsStringAsync();
//            var task = JsonConvert.DeserializeObject<TaskViewModel>(taskJson);

//            // 2) جلب التعليقات
//            var respComments = await client.GetAsync($"Task/{id}/comments");
//            List<TaskCommentDto> comments = new();
//            if (respComments.IsSuccessStatusCode)
//            {
//                var commJson = await respComments.Content.ReadAsStringAsync();
//                comments = JsonConvert.DeserializeObject<List<TaskCommentDto>>(commJson);
//            }

//            // 3) استخراج UserId من الجلسة
//            int currentUserId = 0;
//            if (HttpContext.Session.GetString("UserId") != null)
//                currentUserId = int.Parse(HttpContext.Session.GetString("UserId"));

//            var vm = new TaskDetailsViewModel
//            {
//                Task = task,
//                Comments = comments,
//                CurrentUserId = currentUserId // أضف هذا
//            };

//            return View(vm);
//        }

//        public class UpdateTaskModel
//        {
//            public int Id { get; set; }
//            public string Title { get; set; }
//            public string Description { get; set; }
//            public string Priority { get; set; }
//            public string System { get; set; }
//            public DateTime? DueDate { get; set; }
//            public string Status { get; set; }
//            public int AssignedToUserId { get; set; }
//        }

//        //public class UpdateTaskModel
//        //{
//        //    public int Id { get; set; }
//        //    public string Title { get; set; }
//        //    public string Description { get; set; }
//        //    public string Priority { get; set; }
//        //    public string System { get; set; }
//        //    public DateTime? DueDate { get; set; }
//        //    public string Status { get; set; }
//        //    public int AssignedToUserId { get; set; }
//        //}



//        [HttpPost]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var client = _apiClientFactory.CreateClient();
//            var response = await client.DeleteAsync($"Task/{id}");
//            // لا تعرض خطأ مفصل إن فشل، بل أعد التوجيه
//            return RedirectToAction("Index");
//        }
//    }

//    // DTO داخلي للإرسال إلى API، يطابق TaskItem في API جزئيًا:
//    public class TaskItemForPost
//    {
//        public int Id { get; set; } // في Create قد يكون 0 أو مهمل
//        public string Title { get; set; }
//        public string Description { get; set; }
//        public string Priority { get; set; }
//        public string System { get; set; }
//        public DateTime? DueDate { get; set; }
//        public int AssignedToUserId { get; set; }
//        public string Status { get; set; } = "Pending"; // في Edit يتم تغييره حسب الحاجة
//    }
//    //public class UpdateTaskModel
//    //{
//    //    public int Id { get; set; }
//    //    public string Title { get; set; }
//    //    public string Description { get; set; }
//    //    public string Priority { get; set; }
//    //    public string System { get; set; }
//    //    public DateTime? DueDate { get; set; }
//    //    public string Status { get; set; }
//    //    public int AssignedToUserId { get; set; }
//    //}

//}
