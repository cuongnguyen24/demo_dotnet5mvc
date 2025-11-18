using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace HoursTracker.Models
{
    /// <summary>
    /// Model đại diện cho một kỹ năng cần luyện tập
    /// </summary>
    public class Skill
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên kỹ năng là bắt buộc")]
        [StringLength(100, ErrorMessage = "Tên kỹ năng không được vượt quá 100 ký tự")]
        [Display(Name = "Tên kỹ năng")]
        public string Name { get; set; }

        [StringLength(500, ErrorMessage = "Mô tả không được vượt quá 500 ký tự")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Màu sắc là bắt buộc")]
        [Display(Name = "Màu sắc")]
        public string Color { get; set; } = "#007bff"; // Màu mặc định

        [Display(Name = "Mục tiêu (giờ)")]
        [Range(1, 10000, ErrorMessage = "Mục tiêu phải từ 1 đến 10000 giờ")]
        public int TargetHours { get; set; } = 1000; // Mục tiêu mặc định 1000 giờ

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Foreign key cho User
        [Required]
        public string UserId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }
        public virtual ICollection<PracticeLog> PracticeLogs { get; set; } = new List<PracticeLog>();

        /// <summary>
        /// Tính tổng số phút đã luyện tập
        /// </summary>
        public int TotalMinutes
        {
            get
            {
                return PracticeLogs?.Sum(log => log.Minutes) ?? 0;
            }
        }

        /// <summary>
        /// Tính tổng số giờ đã luyện tập
        /// </summary>
        public double TotalHours
        {
            get
            {
                return Math.Round(TotalMinutes / 60.0, 2);
            }
        }

        /// <summary>
        /// Tính phần trăm hoàn thành mục tiêu
        /// </summary>
        public double ProgressPercentage
        {
            get
            {
                if (TargetHours <= 0) return 0;
                var percentage = (TotalHours / TargetHours) * 100;
                return Math.Min(Math.Round(percentage, 2), 100); // Tối đa 100%
            }
        }
    }
}

