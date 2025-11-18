using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HoursTracker.Data;
using HoursTracker.Models;

namespace HoursTracker.Controllers
{
    /// <summary>
    /// Controller quản lý các kỹ năng (Skills)
    /// </summary>
    public class SkillsController : Controller
    {
        private readonly HoursTrackerDbContext _context;

        public SkillsController(HoursTrackerDbContext context)
        {
            _context = context;
        }

        // GET: Skills
        /// <summary>
        /// Hiển thị danh sách tất cả các kỹ năng
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var skills = await _context.Skills
                .Include(s => s.PracticeLogs)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            return View(skills);
        }

        // GET: Skills/Details/5
        /// <summary>
        /// Hiển thị chi tiết kỹ năng với progress, logs, biểu đồ và milestones
        /// </summary>
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skill = await _context.Skills
                .Include(s => s.PracticeLogs.OrderByDescending(p => p.PracticeDate))
                .FirstOrDefaultAsync(m => m.Id == id);

            if (skill == null)
            {
                return NotFound();
            }

            // Lấy danh sách milestones đã đạt được
            var milestones = await _context.Milestones
                .Where(m => m.SkillId == id)
                .OrderByDescending(m => m.Hours)
                .ToListAsync();

            ViewBag.Milestones = milestones;

            // Tính toán dữ liệu cho biểu đồ (30 ngày gần nhất)
            dynamic chartData = GetChartData(skill);
            ViewBag.ChartLabels = chartData.labels;
            ViewBag.ChartDataValues = chartData.data;

            return View(skill);
        }

        // GET: Skills/Create
        /// <summary>
        /// Hiển thị form tạo kỹ năng mới
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        // POST: Skills/Create
        /// <summary>
        /// Xử lý tạo kỹ năng mới
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Color,TargetHours")] Skill skill)
        {
            if (ModelState.IsValid)
            {
                skill.CreatedDate = DateTime.Now;
                _context.Add(skill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(skill);
        }

        // GET: Skills/Edit/5
        /// <summary>
        /// Hiển thị form chỉnh sửa kỹ năng
        /// </summary>
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
            {
                return NotFound();
            }
            return View(skill);
        }

        // POST: Skills/Edit/5
        /// <summary>
        /// Xử lý cập nhật kỹ năng
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Color,TargetHours,CreatedDate")] Skill skill)
        {
            if (id != skill.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillExists(skill.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(skill);
        }

        // GET: Skills/Delete/5
        /// <summary>
        /// Hiển thị form xác nhận xóa kỹ năng
        /// </summary>
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var skill = await _context.Skills
                .Include(s => s.PracticeLogs)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (skill == null)
            {
                return NotFound();
            }

            return View(skill);
        }

        // POST: Skills/Delete/5
        /// <summary>
        /// Xử lý xóa kỹ năng
        /// </summary>
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Kiểm tra xem kỹ năng có tồn tại không
        /// </summary>
        private bool SkillExists(int id)
        {
            return _context.Skills.Any(e => e.Id == id);
        }

        /// <summary>
        /// Lấy dữ liệu cho biểu đồ (30 ngày gần nhất)
        /// </summary>
        private object GetChartData(Skill skill)
        {
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-29);

            var logs = skill.PracticeLogs
                .Where(p => p.PracticeDate >= startDate && p.PracticeDate <= endDate)
                .GroupBy(p => p.PracticeDate)
                .Select(g => new
                {
                    Date = g.Key,
                    Minutes = g.Sum(p => p.Minutes)
                })
                .OrderBy(x => x.Date)
                .ToList();

            // Tạo danh sách đầy đủ 30 ngày
            var chartData = new
            {
                labels = Enumerable.Range(0, 30)
                    .Select(i => startDate.AddDays(i).ToString("dd/MM"))
                    .ToArray(),
                data = Enumerable.Range(0, 30)
                    .Select(i =>
                    {
                        var date = startDate.AddDays(i);
                        var log = logs.FirstOrDefault(l => l.Date.Date == date.Date);
                        return log != null ? log.Minutes : 0;
                    })
                    .ToArray()
            };

            return chartData;
        }
    }
}

