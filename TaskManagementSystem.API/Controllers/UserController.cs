using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagementSystem.API.Data;
using TaskManagementSystem.API.Models;

namespace TaskManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    //[Authorize] // أو حسب الصلاحيات: من يستطيع مشاهدة الموظفين مثلاً Manager أو Admin
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        public UserController(AppDbContext context) => _context = context;

        // GET: api/User
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Name = u.Name,
                    Email = u.Email,
                    Role = u.Role
                })
                .ToListAsync();
            return Ok(users);
        }

        
        // POST: api/User
        [HttpPost]
        // [Authorize(Roles = "Admin")] // فقط Admin يمكنه إضافة موظفين
        [AllowAnonymous] // للسماح المؤقت أثناء التطوير
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserModel model)
        {
            // تحقق وجود Email مسبقاً
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                return BadRequest("Email موجود مسبقاً.");

            // تجزئة كلمة المرور:
            var passwordHash = model.Password; // هنا يجب تجزئتها فعلياً

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                PasswordHash = passwordHash,
                Role = model.Role
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetUsers), new { id = user.Id }, new UserDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role
            });
        }

        // DTO لعدم إرسال PasswordHash
        public class UserDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public string Role { get; set; }
        }
        public class CreateUserModel
        {
            public string Name { get; set; }
            public string Email { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
        }

    }


}
