using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Spine.Core.ManageSubcription.ViewModel
{
    public class AdminNotificationVM
    {
        public Guid Id { get; set; }
        public string Description { get; set; }
        public string ReminderDate { get; set; }
        public string ReminderTime { get; set; }
        public bool IsRead { get; set; }
        public bool IsActive { get; set; }
        public string NotificationPath { get; set; }
        [JsonIgnore]
        public DateTime? CreatedOn { get; set; }
    }
    public class AdminNotificationDTO
    {
        public string Description { get; set; }
        public string ReminderDate { get; set; }
        public Guid? NotificationPathId { get; set; }
        public DateTime ReminderTime { get; set; }
    }
    public class NotificationReminder
    {
        public string ReminderDate { get; set; }
        public DateTime ReminderTime { get; set; }
    }

    public class AdminLogin
    {
        [Required(ErrorMessage = "Email address is required")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }

    public class ResetPasswordVM
    {
        [Required(ErrorMessage = "Reset code is required")]
        public string ResetCode { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Password and confirm password must match")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePassword
    {
        [JsonIgnore]
        public Guid UserId { get; set; }

        [JsonIgnore]
        public Guid CompanyId { get; set; }

        [Required(ErrorMessage = "Old password is required")]
        public string OldPassword { get; set; }

        [Required(ErrorMessage = "New Password is required")]
        //[DifferentFrom(nameof(OldPassword), ErrorMessage = "Current password and new password cannot be the same")]
        public string NewPassword { get; set; }

        [Compare(nameof(NewPassword), ErrorMessage = "New password and confirm password must match")]
        public string ConfirmNewPassword { get; set; }
    }

    public class ForgotPasswordVM
    {
        public string Email { get; set; }
    }
}
