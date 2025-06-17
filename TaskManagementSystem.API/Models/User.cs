//namespace TaskManagementSystem.API.Models
//{
//    public class User
//    {
//    }
//}
using System.Collections.Generic;

namespace TaskManagementSystem.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }       // الاسم الكامل
        public string Email { get; set; }
        public string PasswordHash { get; set; } // خزّن الهاش
        public string Role { get; set; }       // "Manager", "Support", "Admin"
        public ICollection<TaskItem> Tasks { get; set; }
        public ICollection<TaskComment> Comments { get; set; }
    }
}
