using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Models;
using TaskManagementSystem.API.Services; // لاحقًا للخدمات مثل OverdueTaskChecker
using TaskManagementSystem.API.Hubs;     // لاحقًا لـ SignalR
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);
// 1. Configuration
var configuration = builder.Configuration;

// 2. DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));


// 3. Authentication (JWT)
var jwtSettings = configuration.GetSection("JwtSettings");
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];
var secretKey = jwtSettings["SecretKey"];

var keyBytes = Encoding.UTF8.GetBytes(secretKey);
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        ValidIssuer = issuer,
//        ValidAudience = audience,
//        IssuerSigningKey = new SymmetricSecurityKey(keyBytes)
//    };
//});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["JwtSettings:Issuer"],
            ValidAudience = configuration["JwtSettings:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"])),

            NameClaimType = ClaimTypes.NameIdentifier

        };
        // 👇 السماح بقراءة التوكن من WebSocket عبر query string
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];

                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/notificationHub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });


// 8. CORS إن احتجنا (مثلاً للسماح للـ MVC بطلبات):
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//    {
//        policy.AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials() 
//              .WithOrigins("https://localhost:7241", "http://localhost:5242");
//        // عدِّل Origins حسب عناوين MVC أو الواجهة الأمامية
//    });
//});
// 4. Authorization
builder.Services.AddAuthorization();

// 5. Controllers
builder.Services.AddControllers();
// 6. SignalR
builder.Services.AddSignalR();

// 7.Hosted Service لفحص المهام المتأخرة
builder.Services.AddHostedService<OverdueTaskChecker>();
// Add services to the container.
builder.Services.AddRazorPages();


//builder.Services.AddScoped<NotificationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvcClient", policy =>
    {
        policy
            .WithOrigins("https://localhost:7241") // ⚠️ منفذ الـ MVC
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // ⚠️ مهم جدًا لـ SignalR
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseCors("AllowMvcClient");


app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();


app.MapRazorPages();
// 10. Map SignalR Hubs
app.MapHub<NotificationHub>("/notificationHub");
// تم اضافته Ezz
//app.UseEndpoints(endpoints =>
//{
//    endpoints.MapControllers();
//    endpoints.MapHub<NotificationHub>("/notificationHub");
//});

app.Run();
