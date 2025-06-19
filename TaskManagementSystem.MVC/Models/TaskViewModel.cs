//using Microsoft.AspNetCore.Mvc.Rendering;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;

//namespace TaskManagementSystem.MVC.Models
//{
    //public class TaskViewModel
    //{
    //    public int Id { get; set; }

    //    [Required]
    //    public string Title { get; set; }

    //    public string Description { get; set; }

    //    [Required]
    //    public string Priority { get; set; } // "Low","Medium","High"

    //    public string System { get; set; }

    //    [Required]
    //    public string Status { get; set; } = "Pending"; // Default

    //    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    //    public DateTime? DueDate { get; set; }

    //    [Required]
    //    public int AssignedToUserId { get; set; }
    //    public string AssignedToName { get; set; }

    //    public List<SelectListItem> Employees { get; set; }
    //    public List<SelectListItem> StatusOptions { get; set; }
    //    public List<SelectListItem> PriorityOptions { get; set; }



    //}
    //public class TaskViewModel
    //{
    //    public int Id { get; set; }
    //    [Required] public string Title { get; set; }
    //    public string Description { get; set; }

    //     public string Status { get; set; } = "Pending"; // Default
    //    [Required] public string Priority { get; set; }
    //    public string System { get; set; }
    //    public DateTime? DueDate { get; set; }
    //    [Required] public int AssignedToUserId { get; set; }
    //    public string AssignedToName { get; set; }
    //    public List<SelectListItem> Employees { get; set; }
    //    public List<SelectListItem> StatusOptions { get; set; }
    //    public List<SelectListItem> PriorityOptions { get; set; }
    //}


//}
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.MVC.Models
{
    public class TaskViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "عنوان المهمة مطلوب")]
        [StringLength(200, ErrorMessage = "عنوان المهمة يجب أن يكون أقل من 200 حرف")]
        public string Title { get; set; }

        [StringLength(1000, ErrorMessage = "الوصف يجب أن يكون أقل من 1000 حرف")]
        public string Description { get; set; }

        [Required(ErrorMessage = "الأولوية مطلوبة")]
        public string Priority { get; set; }

        [StringLength(100, ErrorMessage = "اسم النظام يجب أن يكون أقل من 100 حرف")]
        public string System { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [DataType(DataType.Date)]
        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "يجب تحديد الموظف المسؤول")]
        [Range(1, int.MaxValue, ErrorMessage = "يجب اختيار موظف صحيح")]
        public int AssignedToUserId { get; set; }

        public string AssignedToName { get; set; }

        // خصائص للقوائم المنسدلة
        public List<SelectListItem> Employees { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> StatusOptions { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> PriorityOptions { get; set; } = new List<SelectListItem>();
    }

    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }       // الاسم الكامل
        public string Email { get; set; }
        public string PasswordHash { get; set; } // خزّن الهاش
        public string Role { get; set; }       // "Manager", "Support", "Admin"
    }
}


