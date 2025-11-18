using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HoursTracker.Models
{
    /// <summary>
    /// Model đại diện cho một log luyện tập
    /// </summary>
    public class PracticeLog
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kỹ năng là bắt buộc")]
        [Display(Name = "Kỹ năng")]
        public int SkillId { get; set; }

        [Required(ErrorMessage = "Ngày luyện tập là bắt buộc")]
        [Display(Name = "Ngày luyện tập")]
        [DataType(DataType.Date)]
        public DateTime PracticeDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Số phút là bắt buộc")]
        [Range(1, 1440, ErrorMessage = "Số phút phải từ 1 đến 1440 (24 giờ)")]
        [Display(Name = "Số phút")]
        public int Minutes { get; set; }

        [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
        [Display(Name = "Ghi chú")]
        [DataType(DataType.MultilineText)]
        public string Notes { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation property
        [ForeignKey("SkillId")]
        public virtual Skill Skill { get; set; }
    }
}

