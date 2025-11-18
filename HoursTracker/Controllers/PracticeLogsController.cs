using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using HoursTracker.Data;
using HoursTracker.Models;

namespace HoursTracker.Controllers
{
    /// <summary>
    /// Controller quản lý các log luyện tập (Practice Logs)
    /// </summary>
    [Authorize]
    public class PracticeLogsController : Controller
    {
        private readonly HoursTrackerDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PracticeLogsController(HoursTrackerDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #region Create

        // GET: PracticeLogs/Create
        /// <summary>
        /// Hiển thị form tạo log luyện tập mới
        /// </summary>
        public async Task<IActionResult> Create(int? skillId)
        {
            var userId = _userManager.GetUserId(User);
            ViewData["SkillId"] = new SelectList(
                await _context.Skills.Where(s => s.UserId == userId).ToListAsync(), 
                "Id", "Name", skillId);
            return View();
        }

        // POST: PracticeLogs/Create
        /// <summary>
        /// Xử lý tạo log luyện tập mới và kiểm tra milestones
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("SkillId,PracticeDate,Minutes,Notes")] PracticeLog practiceLog)
        {
            // Kiểm tra log trùng ngày cho cùng skill
            var existingLog = await _context.PracticeLogs
                .FirstOrDefaultAsync(p => p.SkillId == practiceLog.SkillId && 
                                         p.PracticeDate.Date == practiceLog.PracticeDate.Date);
            
            if (existingLog != null)
            {
                ModelState.AddModelError("PracticeDate", 
                    $"Đã có log luyện tập cho ngày {practiceLog.PracticeDate.ToString("dd/MM/yyyy")}. Vui lòng chỉnh sửa log hiện có hoặc chọn ngày khác.");
            }

            // Kiểm tra skill thuộc về user hiện tại
            var userId = _userManager.GetUserId(User);
            var skill = await _context.Skills
                .Where(s => s.UserId == userId && s.Id == practiceLog.SkillId)
                .FirstOrDefaultAsync();
            
            if (skill == null)
            {
                ModelState.AddModelError("SkillId", "Kỹ năng không tồn tại hoặc không thuộc về bạn.");
            }

            if (ModelState.IsValid)
            {
                practiceLog.CreatedDate = DateTime.Now;
                _context.Add(practiceLog);
                await _context.SaveChangesAsync();

                // Kiểm tra milestones sau khi thêm log
                var newMilestones = await CheckAndCreateMilestones(practiceLog.SkillId);

                // Nếu có milestone mới, trả về JSON để hiển thị popup
                if (newMilestones.Any())
                {
                    return Json(new
                    {
                        success = true,
                        redirectUrl = Url.Action("Details", "Skills", new { id = practiceLog.SkillId }),
                        milestones = newMilestones.Select(m => new
                        {
                            hours = m.Hours,
                            message = $"🎉 Chúc mừng! Bạn đã đạt {m.Hours} giờ luyện tập!"
                        }).ToList()
                    });
                }

                return RedirectToAction("Details", "Skills", new { id = practiceLog.SkillId });
            }

            ViewData["SkillId"] = new SelectList(
                await _context.Skills.Where(s => s.UserId == userId).ToListAsync(), 
                "Id", "Name", practiceLog.SkillId);
            return View(practiceLog);
        }

        #endregion

        #region Edit

        // GET: PracticeLogs/Edit/5
        /// <summary>
        /// Hiển thị form chỉnh sửa log luyện tập
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var practiceLog = await _context.PracticeLogs
                .Include(p => p.Skill)
                .Where(p => p.Skill.UserId == userId)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (practiceLog == null)
            {
                return NotFound();
            }

            ViewData["SkillId"] = new SelectList(
                await _context.Skills.Where(s => s.UserId == userId).ToListAsync(), 
                "Id", "Name", practiceLog.SkillId);
            return View(practiceLog);
        }

        // POST: PracticeLogs/Edit/5
        /// <summary>
        /// Xử lý cập nhật log luyện tập
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,SkillId,PracticeDate,Minutes,Notes,CreatedDate")] PracticeLog practiceLog)
        {
            if (id != practiceLog.Id)
            {
                return NotFound();
            }

            // Kiểm tra log trùng ngày cho cùng skill (loại trừ chính log đang edit)
            var existingLog = await _context.PracticeLogs
                .FirstOrDefaultAsync(p => p.SkillId == practiceLog.SkillId && 
                                         p.PracticeDate.Date == practiceLog.PracticeDate.Date &&
                                         p.Id != practiceLog.Id);
            
            if (existingLog != null)
            {
                ModelState.AddModelError("PracticeDate", 
                    $"Đã có log luyện tập khác cho ngày {practiceLog.PracticeDate.ToString("dd/MM/yyyy")}. Vui lòng chọn ngày khác.");
            }

            // Kiểm tra ownership
            var userId = _userManager.GetUserId(User);
            var existingPracticeLog = await _context.PracticeLogs
                .Include(p => p.Skill)
                .Where(p => p.Skill.UserId == userId)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (existingPracticeLog == null)
            {
                return NotFound();
            }

            // Kiểm tra skill thuộc về user
            var skill = await _context.Skills
                .Where(s => s.UserId == userId && s.Id == practiceLog.SkillId)
                .FirstOrDefaultAsync();
            
            if (skill == null)
            {
                ModelState.AddModelError("SkillId", "Kỹ năng không tồn tại hoặc không thuộc về bạn.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Update trực tiếp vào existingPracticeLog đã được track
                    existingPracticeLog.SkillId = practiceLog.SkillId;
                    existingPracticeLog.PracticeDate = practiceLog.PracticeDate;
                    existingPracticeLog.Minutes = practiceLog.Minutes;
                    existingPracticeLog.Notes = practiceLog.Notes;
                    // Giữ nguyên CreatedDate
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PracticeLogExists(practiceLog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Skills", new { id = practiceLog.SkillId });
            }

            ViewData["SkillId"] = new SelectList(
                await _context.Skills.Where(s => s.UserId == userId).ToListAsync(), 
                "Id", "Name", practiceLog.SkillId);
            return View(practiceLog);
        }

        #endregion

        #region Delete
        // GET: PracticeLogs/Delete/5
        /// <summary>
        /// Hiển thị form xác nhận xóa log luyện tập
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var practiceLog = await _context.PracticeLogs
                .Include(p => p.Skill)
                .Where(p => p.Skill.UserId == userId)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (practiceLog == null)
            {
                return NotFound();
            }

            return View(practiceLog);
        }

        // POST: PracticeLogs/Delete/5
        /// <summary>
        /// Xử lý xóa log luyện tập
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = _userManager.GetUserId(User);
            var practiceLog = await _context.PracticeLogs
                .Include(p => p.Skill)
                .Where(p => p.Skill.UserId == userId)
                .FirstOrDefaultAsync(p => p.Id == id);
            
            if (practiceLog == null)
            {
                return NotFound();
            }

            var skillId = practiceLog.SkillId;
            _context.PracticeLogs.Remove(practiceLog);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Skills", new { id = skillId });
        }

        #endregion

        #region HelperFuncion

        /// <summary>
        /// Kiểm tra xem log có tồn tại không và thuộc về user hiện tại
        /// </summary>
        private bool PracticeLogExists(int id)
        {
            var userId = _userManager.GetUserId(User);
            return _context.PracticeLogs
                .Include(p => p.Skill)
                .Any(e => e.Id == id && e.Skill.UserId == userId);
        }

        /// <summary>
        /// Kiểm tra và tạo milestones mới khi đạt được
        /// </summary>
        private async Task<System.Collections.Generic.List<Milestone>> CheckAndCreateMilestones(int skillId)
        {
            var skill = await _context.Skills
                .Include(s => s.PracticeLogs)
                .FirstOrDefaultAsync(s => s.Id == skillId);

            if (skill == null) return new System.Collections.Generic.List<Milestone>();

            var totalHours = skill.TotalHours;
            var newMilestones = new System.Collections.Generic.List<Milestone>();

            // Danh sách các mốc milestone
            var milestoneHours = new[] { 100, 250, 500, 750, 1000, 1500, 2000, 2500, 3000 };

            foreach (var hours in milestoneHours)
            {
                // Kiểm tra xem đã đạt milestone này chưa
                var existingMilestone = await _context.Milestones
                    .FirstOrDefaultAsync(m => m.SkillId == skillId && m.Hours == hours);

                // Nếu chưa có và đã đạt được
                if (existingMilestone == null && totalHours >= hours)
                {
                    var milestone = new Milestone
                    {
                        SkillId = skillId,
                        Hours = hours,
                        AchievedDate = DateTime.Now,
                        IsNotified = false
                    };

                    _context.Milestones.Add(milestone);
                    newMilestones.Add(milestone);
                }
            }

            if (newMilestones.Any())
            {
                await _context.SaveChangesAsync();
            }

            return newMilestones;
        }
        #endregion
    }
}

