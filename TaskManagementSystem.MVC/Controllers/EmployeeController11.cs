//using Microsoft.AspNetCore.Mvc;
//using Newtonsoft.Json;
//using System.Collections.Generic;
//using System.Text;
//using System.Threading.Tasks;
//using TaskManagementSystem.MVC.Helpers;
//using TaskManagementSystem.MVC.Models;

//namespace TaskManagementSystem.MVC.Controllers
//{
//    public class EmployeeController : Controller
//    {
//        private readonly ApiClientFactory _apiClientFactory;
//        private readonly ILogger<EmployeeController> _logger;

//        public EmployeeController(ApiClientFactory apiClientFactory, ILogger<EmployeeController> logger)
//        {
//            _apiClientFactory = apiClientFactory;
//            _logger = logger;
//            if (_apiClientFactory == null)
//                _logger.LogError("ApiClientFactory is null!");
//        }

//        // GET: /Employee/Index
//        [RequireLogin]
//        public async Task<IActionResult> Index()
//        {
//            var client = _apiClientFactory.CreateClient();
//            HttpResponseMessage response;
//            try
//            {
//                response = await client.GetAsync("User");
//            }
//            catch (Exception ex)
//            {
//                ViewData["Error"] = "فشل الاتصال بالـ API: " + ex.Message;
//                return View(new List<EmployeeViewModel>());
//            }

//            if (response == null)
//            {
//                ViewData["Error"] = "لم يستلم رد من الخادم.";
//                return View(new List<EmployeeViewModel>());
//            }
//            if (!response.IsSuccessStatusCode)
//            {
//                var err = await response.Content.ReadAsStringAsync();
//                ViewData["Error"] = $"فشل جلب الموظفين: {response.StatusCode}. {err}";
//                return View(new List<EmployeeViewModel>());
//            }

//            var json = await response.Content.ReadAsStringAsync();
//            List<EmployeeViewModel> employees;
//            try
//            {
//                employees = JsonConvert.DeserializeObject<List<EmployeeViewModel>>(json);
//            }
//            catch (Exception ex)
//            {
//                ViewData["Error"] = "خطأ في تحليل بيانات الموظفين: " + ex.Message;
//                return View(new List<EmployeeViewModel>());
//            }
//            return View(employees);
//        }

//        // GET: جلب بيانات موظف محدد للتعديل (AJAX)
//        [HttpGet]
//        public async Task<IActionResult> GetEmployee(int id)
//        {
//            try
//            {
//                _logger.LogInformation("استلام طلب جلب بيانات الموظف: {EmployeeId}", id);

//                var client = _apiClientFactory.CreateClient();
//                var response = await client.GetAsync($"User/{id}");

//                if (!response.IsSuccessStatusCode)
//                {
//                    var errorBody = await response.Content.ReadAsStringAsync();
//                    _logger.LogError("فشل في جلب بيانات الموظف. Status Code: {StatusCode}, Response: {Response}", response.StatusCode, errorBody);
//                    return Json(new { success = false, message = $"فشل في جلب بيانات الموظف: {errorBody}" });
//                }

//                var json = await response.Content.ReadAsStringAsync();
//                var employee = JsonConvert.DeserializeObject<EmployeeViewModel>(json);

//                _logger.LogInformation("تم جلب بيانات الموظف بنجاح: {EmployeeData}", JsonConvert.SerializeObject(employee));

//                return Json(new
//                {
//                    success = true,
//                    data = new
//                    {
//                        id = employee.Id,
//                        name = employee.Name,
//                        email = employee.Email,
//                        role = employee.Role
//                    }
//                });
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ في جلب بيانات الموظف");
//                return Json(new { success = false, message = "حدث خطأ أثناء جلب بيانات الموظف" });
//            }
//        }

//        // POST: إنشاء موظف جديد (AJAX)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> CreateAjax([FromForm] EmployeeCreateAjaxModel model)
//        {
//            try
//            {
//                _logger.LogInformation("استلام طلب إنشاء موظف جديد: {EmployeeData}", JsonConvert.SerializeObject(model));

//                if (!ModelState.IsValid)
//                {
//                    var errors = ModelState
//                        .Where(x => x.Value.Errors.Count > 0)
//                        .ToDictionary(
//                            kvp => kvp.Key,
//                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
//                        );

//                    _logger.LogWarning("بيانات غير صحيحة في إنشاء الموظف: {Errors}", JsonConvert.SerializeObject(errors));
//                    return Json(new { success = false, errors = errors });
//                }

//                var client = _apiClientFactory.CreateClient();
//                var toSend = new
//                {
//                    Name = model.Name,
//                    Email = model.Email,
//                    Password = model.Password,
//                    Role = model.Role
//                };

//                var json = JsonConvert.SerializeObject(toSend);
//                _logger.LogInformation("إرسال بيانات الموظف إلى API: {ApiData}", json);

//                var response = await client.PostAsync("User", new StringContent(json, Encoding.UTF8, "application/json"));

//                if (response.IsSuccessStatusCode)
//                {
//                    _logger.LogInformation("تم إنشاء الموظف بنجاح");
//                    return Json(new { success = true, message = "تم إضافة الموظف بنجاح" });
//                }
//                else
//                {
//                    var errorBody = await response.Content.ReadAsStringAsync();
//                    _logger.LogError("فشل في إنشاء الموظف. Status Code: {StatusCode}, Response: {Response}", response.StatusCode, errorBody);
//                    return Json(new { success = false, message = $"فشل في إضافة الموظف: {errorBody}" });
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ في إنشاء الموظف");
//                return Json(new { success = false, message = "حدث خطأ أثناء إضافة الموظف" });
//            }
//        }

//        // POST: تعديل موظف (AJAX)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> EditAjax(int id, [FromForm] EmployeeEditAjaxModel model)
//        {
//            try
//            {
//                _logger.LogInformation("استلام طلب تعديل الموظف: {EmployeeId}, البيانات: {EmployeeData}", id, JsonConvert.SerializeObject(model));

//                if (!ModelState.IsValid)
//                {
//                    var errors = ModelState
//                        .Where(x => x.Value.Errors.Count > 0)
//                        .ToDictionary(
//                            kvp => kvp.Key,
//                            kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
//                        );

//                    _logger.LogWarning("بيانات غير صحيحة في تعديل الموظف: {Errors}", JsonConvert.SerializeObject(errors));
//                    return Json(new { success = false, errors = errors });
//                }

//                var client = _apiClientFactory.CreateClient();
//                var toSend = new
//                {
//                    Name = model.Name,
//                    Email = model.Email,
//                    Role = model.Role
//                };

//                var json = JsonConvert.SerializeObject(toSend);
//                _logger.LogInformation("إرسال بيانات التعديل إلى API: {ApiData}", json);

//                var response = await client.PutAsync($"User/{id}", new StringContent(json, Encoding.UTF8, "application/json"));

//                if (response.IsSuccessStatusCode)
//                {
//                    _logger.LogInformation("تم تعديل الموظف بنجاح");
//                    return Json(new { success = true, message = "تم تعديل الموظف بنجاح" });
//                }
//                else
//                {
//                    var errorBody = await response.Content.ReadAsStringAsync();
//                    _logger.LogError("فشل في تعديل الموظف. Status Code: {StatusCode}, Response: {Response}", response.StatusCode, errorBody);
//                    return Json(new { success = false, message = $"فشل في تعديل الموظف: {errorBody}" });
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ في تعديل الموظف");
//                return Json(new { success = false, message = "حدث خطأ أثناء تعديل الموظف" });
//            }
//        }

//        // POST: حذف موظف (AJAX محسن)
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteConfirmed(int id)
//        {
//            try
//            {
//                _logger.LogInformation("استلام طلب حذف الموظف: {EmployeeId}", id);

//                var client = _apiClientFactory.CreateClient();
//                var response = await client.DeleteAsync($"User/{id}");

//                if (response.IsSuccessStatusCode)
//                {
//                    _logger.LogInformation("تم حذف الموظف بنجاح");
//                    return Json(new { success = true, message = "تم حذف الموظف بنجاح" });
//                }
//                else
//                {
//                    var errorBody = await response.Content.ReadAsStringAsync();
//                    _logger.LogError("فشل في حذف الموظف. Status Code: {StatusCode}, Response: {Response}", response.StatusCode, errorBody);
//                    return Json(new { success = false, message = $"فشل في حذف الموظف: {errorBody}" });
//                }
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "خطأ في حذف الموظف");
//                return Json(new { success = false, message = "حدث خطأ أثناء حذف الموظف" });
//            }
//        }

//        // الطرق التقليدية (للتوافق مع الواجهات القديمة)
//        [HttpGet]
//        public IActionResult Create()
//        {
//            var vm = new EmployeeCreateViewModel();
//            return View(vm);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Create(EmployeeCreateViewModel model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            var client = _apiClientFactory.CreateClient();
//            var toSend = new
//            {
//                Name = model.Name,
//                Email = model.Email,
//                Password = model.Password,
//                Role = model.Role
//            };
//            var json = JsonConvert.SerializeObject(toSend);
//            var resp = await client.PostAsync("User", new StringContent(json, Encoding.UTF8, "application/json"));
//            if (!resp.IsSuccessStatusCode)
//            {
//                var err = await resp.Content.ReadAsStringAsync();
//                ModelState.AddModelError("", $"خطأ من الخادم: {err}");
//                return View(model);
//            }
//            TempData["SuccessMessage"] = "تم إضافة الموظف.";
//            return RedirectToAction("Index");
//        }

//        [HttpGet]
//        public async Task<IActionResult> Edit(int id)
//        {
//            var client = _apiClientFactory.CreateClient();
//            var response = await client.GetAsync($"User/{id}");
//            if (!response.IsSuccessStatusCode)
//            {
//                ViewData["Error"] = "فشل جلب بيانات الموظف.";
//                return RedirectToAction("Index");
//            }

//            var json = await response.Content.ReadAsStringAsync();
//            var emp = JsonConvert.DeserializeObject<EmployeeViewModel>(json);

//            var vm = new EmployeeEditViewModel
//            {
//                Id = emp.Id,
//                Name = emp.Name,
//                Email = emp.Email,
//                Role = emp.Role
//            };

//            return View(vm);
//        }

//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Edit(int id, EmployeeEditViewModel model)
//        {
//            if (!ModelState.IsValid)
//                return View(model);

//            var client = _apiClientFactory.CreateClient();
//            var toSend = new
//            {
//                Name = model.Name,
//                Email = model.Email,
//                Role = model.Role
//            };

//            var json = JsonConvert.SerializeObject(toSend);
//            var resp = await client.PutAsync($"User/{id}", new StringContent(json, Encoding.UTF8, "application/json"));

//            if (!resp.IsSuccessStatusCode)
//            {
//                var err = await resp.Content.ReadAsStringAsync();
//                ModelState.AddModelError("", $"خطأ في التعديل: {err}");
//                return View(model);
//            }

//            TempData["SuccessMessage"] = "تم تعديل الموظف بنجاح.";
//            return RedirectToAction("Index");
//        }

//        [HttpGet]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var client = _apiClientFactory.CreateClient();
//            var response = await client.GetAsync($"User/{id}");
//            if (!response.IsSuccessStatusCode)
//            {
//                TempData["ErrorMessage"] = "لم يتم العثور على الموظف.";
//                return RedirectToAction("Index");
//            }

//            var json = await response.Content.ReadAsStringAsync();
//            var emp = JsonConvert.DeserializeObject<EmployeeViewModel>(json);
//            return View(emp);
//        }
//    }

//    // نماذج البيانات للـ AJAX
//    public class EmployeeCreateAjaxModel
//    {
//        public string Name { get; set; }
//        public string Email { get; set; }
//        public string Password { get; set; }
//        public string Role { get; set; }
//    }

//    public class EmployeeEditAjaxModel
//    {
//        public int Id { get; set; }
//        public string Name { get; set; }
//        public string Email { get; set; }
//        public string Role { get; set; }
//    }
//}

