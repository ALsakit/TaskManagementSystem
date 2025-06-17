using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.MVC.Models
{
    public class TaskViewModel
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string Priority { get; set; } // "Low","Medium","High"

        public string System { get; set; }

        [Required]
        public string Status { get; set; } = "Pending"; // Default

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DueDate { get; set; }

        [Required]
        public int AssignedToUserId { get; set; }
        public string AssignedToName { get; set; }

        public List<SelectListItem> Employees { get; set; }
        public List<SelectListItem> StatusOptions { get; set; }
        public List<SelectListItem> PriorityOptions { get; set; }
       


    }
}
