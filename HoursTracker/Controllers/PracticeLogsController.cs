using System;
using System.Linq;
using System.Threading.Tasks;
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
    public class PracticeLogsController : Controller
    {
        private readonly HoursTrackerDbContext _context;

        public PracticeLogsController(HoursTrackerDbContext context)
        {
            _context = context;
        }

        // GET: PracticeLogs/Create
        /// <summary>
        /// Hiển thị form tạo log luyện tập mới
        /// </summary>
        public async Task<IActionResult> Create(int? skillId)
        {
            ViewData["SkillId"] = new SelectList(await _context.Skills.ToListAsync(), "Id", "Name", skillId);
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

            ViewData["SkillId"] = new SelectList(await _context.Skills.ToListAsync(), "Id", "Name", practiceLog.SkillId);
            return View(practiceLog);
        }

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

            var practiceLog = await _context.PracticeLogs.FindAsync(id);
            if (practiceLog == null)
            {
                return NotFound();
            }

            ViewData["SkillId"] = new SelectList(await _context.Skills.ToListAsync(), "Id", "Name", practiceLog.SkillId);
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

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(practiceLog);
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

            ViewData["SkillId"] = new SelectList(await _context.Skills.ToListAsync(), "Id", "Name", practiceLog.SkillId);
            return View(practiceLog);
        }

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

            var practiceLog = await _context.PracticeLogs
                .Include(p => p.Skill)
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
            var practiceLog = await _context.PracticeLogs.FindAsync(id);
            var skillId = practiceLog.SkillId;
            _context.PracticeLogs.Remove(practiceLog);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Skills", new { id = skillId });
        }

        /// <summary>
        /// Kiểm tra xem log có tồn tại không
        /// </summary>
        private bool PracticeLogExists(int id)
        {
            return _context.PracticeLogs.Any(e => e.Id == id);
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
    }
}

