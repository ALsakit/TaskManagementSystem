using System.ComponentModel.DataAnnotations;

namespace TaskManagementSystem.MVC.Models
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "الاسم مطلوب")]
        [Display(Name = "الاسم")]
        public string Name { get; set; }

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "البريد الإلكتروني غير صالح")]
        [Display(Name = "البريد الإلكتروني")]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [Required(ErrorMessage = "تأكيد كلمة المرور مطلوب")]
        [DataType(DataType.Password)]
        [Display(Name = "تأكيد كلمة المرور")]
        [Compare("Password", ErrorMessage = "كلمة المرور وتأكيدها غير متطابقين")]
        public string ConfirmPassword { get; set; }

        // في حال احتجت دور اختيار (dropdown) للمسؤولين:
        // يمكنك إضافة حقل Role هنا ثم عرضه في الواجهة كقائمة. 
        // لكن عادةً التسجيل العادي للمستخدمين يكون بدور افتراضي مثل "User".
        // [Display(Name = "الدور")]
        // public string Role { get; set; } = "User";
    }
}
