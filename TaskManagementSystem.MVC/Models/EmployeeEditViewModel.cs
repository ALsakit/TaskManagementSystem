using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.MVC.Models
{
    public class EmployeeEditViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }

        public List<SelectListItem> Roles { get; set; } = new()
        {
            new SelectListItem("Manager", "Manager"),
            new SelectListItem("Support", "Support"),
            new SelectListItem("Admin", "Admin")
        };
    }

}
