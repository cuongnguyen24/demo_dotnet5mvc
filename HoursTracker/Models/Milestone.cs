using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HoursTracker.Models
{
    /// <summary>
    /// Model đại diện cho một milestone đã đạt được
    /// </summary>
    public class Milestone
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Kỹ năng")]
        public int SkillId { get; set; }

        [Required]
        [Display(Name = "Mốc (giờ)")]
        public int Hours { get; set; }

        [Display(Name = "Ngày đạt được")]
        public DateTime AchievedDate { get; set; } = DateTime.Now;

        [Display(Name = "Đã thông báo")]
        public bool IsNotified { get; set; } = false;

        // Navigation property
        [ForeignKey("SkillId")]
        public virtual Skill Skill { get; set; }
    }
}

