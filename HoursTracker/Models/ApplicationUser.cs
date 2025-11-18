using System;
using Microsoft.AspNetCore.Identity;

namespace HoursTracker.Models
{
    /// <summary>
    /// Model đại diện cho người dùng trong hệ thống
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>
        /// Tên hiển thị của người dùng
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// Ngày tạo tài khoản
        /// </summary>
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}

