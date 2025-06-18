using Microsoft.AspNetCore.Http;
using System;
using System.Net.Http;
using System.Net.Http.Headers;

namespace TaskManagementSystem.MVC.Helpers
{
    public class ApiClientFactory
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;

        public ApiClientFactory(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
            
            _apiBaseUrl = "https://localhost:7172/api/";

        }

        public HttpClient CreateClient()
        {
            var client = new HttpClient { BaseAddress = new Uri(_apiBaseUrl) };

            var context = _httpContextAccessor.HttpContext;
            if (context == null)
            {
                // HttpContext غير متاح
                // يمكنك تسجيل خطأ عبر ILogger إذا حقنته
                return client;
            }
            var session = context.Session;
            if (session == null)
            {
                // Session غير مفعل
                return client;
            }
            var token = session.GetString("JWToken");
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            return client;
        }

    }
}
