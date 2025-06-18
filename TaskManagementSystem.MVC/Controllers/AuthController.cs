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
using Newtonsoft.Json.Linq;
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

        public AuthController(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpClient = new HttpClient { BaseAddress = new System.Uri(_apiBaseUrl) };
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
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
}
