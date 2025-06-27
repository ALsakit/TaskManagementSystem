
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using TaskManagementSystem.MVC.Models;

namespace TaskManagementSystem.MVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7172/api/";


        private readonly IHttpClientFactory _clientFactory;

        public AuthController(IHttpContextAccessor httpContextAccessor, IHttpClientFactory clientFactory)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = new HttpClient { BaseAddress = new System.Uri(_apiBaseUrl) };

            _clientFactory = clientFactory;

        }

        // GET: /Auth/Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        // POST: /Auth/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            // التحويل إلى النموذج الذي تتوقعه API
            var createUserObj = new
            {
                Name = model.Name,
                Email = model.Email,
                Password = model.Password,
                Role = "Admin" // أو model.Role إذا عرضت حقل الدور للمستخدم
            };

            var client = _clientFactory.CreateClient();
            client.BaseAddress = new System.Uri(_apiBaseUrl);
           

            var json = JsonConvert.SerializeObject(createUserObj);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;
            try
            {
                response = await client.PostAsync("User", content); // endpoint: POST api/User
            }
            catch (HttpRequestException ex)
            {
                // خطأ في الاتصال بالـ API
                ModelState.AddModelError("", "تعذّر الاتصال بالخادم. حاول لاحقاً.");
                return View(model);
            }

            if (!response.IsSuccessStatusCode)
            {
                // قراءة رسالة الخطأ من الـ API (مثلاً Email موجود سابقاً)
                var errorContent = await response.Content.ReadAsStringAsync();
                // اعتماداً على تنسيق الخطأ: إذا كانت رسالة نصية:
                ModelState.AddModelError("", $"فشل التسجيل: {errorContent}");
                // أو يمكنك تحليل JSON إذا كان الـ API يعيد رسائل مفصّلة
                return View(model);
            }

            // عند النجاح:
            TempData["RegisterSuccess"] = "تم إنشاء الحساب بنجاح. يمكنك تسجيل الدخول الآن.";
            return RedirectToAction("Login");
        }

        // يجب تعديل GET Login ليعرض رسالة نجاح التسجيل إن وجدت:
        [HttpGet]
        public IActionResult Login()
        {
            if (TempData["RegisterSuccess"] != null)
            {
                ViewBag.RegisterSuccess = TempData["RegisterSuccess"];
            }
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid && false)
                return View(model);

            var client = _clientFactory.CreateClient("AuthApiClient");

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("Auth/login", content); // ← تأكد أن هذا URL صحيح

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "فشل تسجيل الدخول. تحقق من البريد الإلكتروني وكلمة المرور.";
                return View();
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<LoginResponse>(responseContent);

            // حفظ التوكن في Session أو Cookie أو LocalStorage - حسب الحاجة
            HttpContext.Session.SetString("JWToken", result.token);
            HttpContext.Session.SetString("UserRole", result.user.Role);
            HttpContext.Session.SetString("UserName", result.user.Name);
            HttpContext.Session.SetString("UserId", result.user.Id.ToString());

            //int x = 4;
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("JWToken")))
            {
                //return RedirectToAction("Login", "Auth");
                return View();
            }
            //_httpContextAccessor.HttpContext.Session.SetString("JWToken", result.token);
            //_httpContextAccessor.HttpContext.Session.SetString("UserRole", result.role ?? "");
            // حفظ معلومات المستخدم لو أردت
            //TempData["UserName"] = result.user.Name;

            return RedirectToAction("Index", "Home");
        }


      

        public IActionResult Logout()
        {
            _httpContextAccessor.HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
    public class LoginViewModel
    {
        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
    public class LoginResponse
    {
        public string token { get; set; }
        public User user { get; set; }

    }

}
