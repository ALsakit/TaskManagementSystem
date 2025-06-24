//using Microsoft.AspNetCore.Mvc;

//namespace TaskManagementSystem.MVC.Controllers
//{
//    public class AuthController : Controller
//    {
//        public IActionResult Index()
//        {
//            return View();
//        }
//    }
//}
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



        [HttpGet]
        public IActionResult Login()
        {
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


        [HttpPost]
        public async Task<IActionResult> Login1(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("Auth/login", content);
            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError("", "فشل تسجيل الدخول");
                return View(model);
            }

            var result = await response.Content.ReadAsStringAsync();
            var jobj = JObject.Parse(result);
            var token = jobj["token"]?.ToString();
            var role = jobj["role"]?.ToString();

            if (!string.IsNullOrEmpty(token))
            {
                _httpContextAccessor.HttpContext.Session.SetString("JWToken", token);
                _httpContextAccessor.HttpContext.Session.SetString("UserRole", role ?? "");
                // كذلك حفظ UserId وName إن رغبت:
                // يمكنك قراءة Claims من التوكن إذا أردت: لكن MVC لا تفكك JWT تلقائيًا، تحتاج مكتبة JWT للتحليل.

                return RedirectToAction("Index", "Task");
            }

            ModelState.AddModelError("", "فشل استرداد التوكن");
            return View(model);
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
