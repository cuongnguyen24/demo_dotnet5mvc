using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HoursTracker.Data;
using HoursTracker.Models;

namespace HoursTracker.Controllers
{
    /// <summary>
    /// Controller quản lý các kỹ năng (Skills)
    /// </summary>
    [Authorize]
    public class SkillsController : Controller
    {
        #region Fields & Constructor

        private readonly HoursTrackerDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SkillsController(HoursTrackerDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        #endregion

        #region Index & Details

        // GET: Skills
        /// <summary>
        /// Hiển thị danh sách tất cả các kỹ năng của user hiện tại
        /// </summary>
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var skills = await _context.Skills
                .Where(s => s.UserId == userId)
                .Include(s => s.PracticeLogs)
                .OrderByDescending(s => s.CreatedDate)
                .ToListAsync();

            return View(skills);
        }

        // GET: Skills/Details/5
        /// <summary>
        /// Hiển thị chi tiết kỹ năng với progress, logs, biểu đồ và milestones
        /// </summary>
        public async Task<IActionResult> Details(int? id, int page = 1)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);

            // Load skill với PracticeLogs để tính TotalHours, ProgressPercentage
            var skill = await _context.Skills
                .Where(s => s.UserId == userId)
                .Include(s => s.PracticeLogs)
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

            // Tính toán dữ liệu cho biểu đồ (30 ngày gần nhất - mặc định)
            dynamic chartData = GetChartData(skill, "30days");
            ViewBag.ChartLabels = chartData.labels;
            ViewBag.ChartDataValues = chartData.data;
            ViewBag.ChartPeriod = "30days"; // Mặc định

            // Phân trang PracticeLogs
            const int pageSize = 10; // Số items mỗi trang
            var totalLogs = skill.PracticeLogs.Count;
            var totalPages = (int)Math.Ceiling(totalLogs / (double)pageSize);
            
            // Đảm bảo page hợp lệ
            if (page < 1) page = 1;
            if (page > totalPages && totalPages > 0) page = totalPages;

            var paginatedLogs = skill.PracticeLogs
                .OrderByDescending(p => p.PracticeDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            ViewBag.PracticeLogs = paginatedLogs;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;
            ViewBag.TotalLogs = totalLogs;
            ViewBag.PageSize = pageSize;

            return View(skill);
        }

        #endregion

        #region API Actions

        // GET: Skills/GetChartData/5?period=30days
        /// <summary>
        /// API endpoint để lấy dữ liệu biểu đồ theo khoảng thời gian
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetChartData(int id, string period = "30days")
        {
            var userId = _userManager.GetUserId(User);
            var skill = await _context.Skills
                .Where(s => s.UserId == userId)
                .Include(s => s.PracticeLogs)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (skill == null)
            {
                return NotFound();
            }

            var chartData = GetChartData(skill, period);
            return Json(chartData);
        }

        #endregion

        #region Create
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
            // Loại bỏ UserId khỏi ModelState validation vì nó sẽ được set từ User hiện tại
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("PracticeLogs");
            
            if (ModelState.IsValid)
            {
                skill.UserId = _userManager.GetUserId(User);
                skill.CreatedDate = DateTime.Now;
                _context.Add(skill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(skill);
        }
        #endregion

        #region Edit

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

            var userId = _userManager.GetUserId(User);
            var skill = await _context.Skills
                .Where(s => s.UserId == userId)
                .FirstOrDefaultAsync(s => s.Id == id);
            
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

            var userId = _userManager.GetUserId(User);
            
            // Kiểm tra ownership
            var existingSkill = await _context.Skills
                .Where(s => s.UserId == userId)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (existingSkill == null)
            {
                return NotFound();
            }

            // Loại bỏ UserId, User, PracticeLogs khỏi ModelState validation
            ModelState.Remove("UserId");
            ModelState.Remove("User");
            ModelState.Remove("PracticeLogs");

            if (ModelState.IsValid)
            {
                try
                {
                    // Update trực tiếp vào existingSkill đã được track để tránh tracking conflict
                    existingSkill.Name = skill.Name;
                    existingSkill.Description = skill.Description;
                    existingSkill.Color = skill.Color;
                    existingSkill.TargetHours = skill.TargetHours;
                    // Giữ nguyên UserId và CreatedDate
                    
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
        #endregion

        #region Delete
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

            var userId = _userManager.GetUserId(User);
            var skill = await _context.Skills
                .Where(s => s.UserId == userId)
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
            var userId = _userManager.GetUserId(User);
            var skill = await _context.Skills
                .Where(s => s.UserId == userId)
                .FirstOrDefaultAsync(s => s.Id == id);
            
            if (skill == null)
            {
                return NotFound();
            }

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        #endregion

        #region Private Helper Methods

        /// <summary>
        /// Kiểm tra xem kỹ năng có tồn tại không và thuộc về user hiện tại
        /// </summary>
        private bool SkillExists(int id)
        {
            var userId = _userManager.GetUserId(User);
            return _context.Skills.Any(e => e.Id == id && e.UserId == userId);
        }

        /// <summary>
        /// Lấy dữ liệu cho biểu đồ theo khoảng thời gian
        /// </summary>
        /// <param name="skill">Skill object</param>
        /// <param name="period">Khoảng thời gian: "7days", "30days", "90days", "6months", "12months", "year"</param>
        private static object GetChartData(Skill skill, string period = "30days")
        {
            DateTime startDate, endDate;
            int daysCount;
            string labelFormat;
            Func<DateTime, string> labelFormatter;

            // Xác định khoảng thời gian
            switch (period?.ToLower())
            {
                case "7days":
                    endDate = DateTime.Today;
                    startDate = endDate.AddDays(-6);
                    daysCount = 7;
                    labelFormat = "dd/MM";
                    labelFormatter = d => d.ToString("dd/MM");
                    break;

                case "30days":
                    endDate = DateTime.Today;
                    startDate = endDate.AddDays(-29);
                    daysCount = 30;
                    labelFormat = "dd/MM";
                    labelFormatter = d => d.ToString("dd/MM");
                    break;

                case "90days":
                    endDate = DateTime.Today;
                    startDate = endDate.AddDays(-89);
                    daysCount = 90;
                    labelFormat = "dd/MM";
                    labelFormatter = d => d.ToString("dd/MM");
                    break;

                case "6months":
                    endDate = DateTime.Today;
                    startDate = new DateTime(endDate.Year, endDate.Month, 1).AddMonths(-5);
                    daysCount = (int)(endDate - startDate).TotalDays + 1;
                    labelFormat = "MM/yyyy";
                    labelFormatter = d => d.ToString("MM/yyyy");
                    break;

                case "12months":
                case "year":
                    endDate = DateTime.Today;
                    startDate = new DateTime(endDate.Year, 1, 1);
                    daysCount = (int)(endDate - startDate).TotalDays + 1;
                    labelFormat = "MM/yyyy";
                    labelFormatter = d => d.ToString("MM/yyyy");
                    break;

                default:
                    // Mặc định 30 ngày
                    endDate = DateTime.Today;
                    startDate = endDate.AddDays(-29);
                    daysCount = 30;
                    labelFormat = "dd/MM";
                    labelFormatter = d => d.ToString("dd/MM");
                    break;
            }

            // Lấy logs trong khoảng thời gian
            var logs = skill.PracticeLogs
                .Where(p => p.PracticeDate.Date >= startDate.Date && p.PracticeDate.Date <= endDate.Date)
                .ToList();

            // Xử lý theo loại period
            if (period?.ToLower() == "6months" || period?.ToLower() == "12months" || period?.ToLower() == "year")
            {
                // Nhóm theo tháng
                var monthlyData = logs
                    .GroupBy(p => new { p.PracticeDate.Year, p.PracticeDate.Month })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Minutes = g.Sum(p => p.Minutes)
                    })
                    .ToList();

                // Tạo danh sách đầy đủ các tháng
                var labels = new List<string>();
                var data = new List<int>();

                var currentDate = new DateTime(startDate.Year, startDate.Month, 1);
                var endMonth = new DateTime(endDate.Year, endDate.Month, 1);

                while (currentDate <= endMonth)
                {
                    labels.Add(currentDate.ToString("MM/yyyy"));
                    var monthData = monthlyData.FirstOrDefault(m => m.Year == currentDate.Year && m.Month == currentDate.Month);
                    data.Add(monthData != null ? monthData.Minutes : 0);
                    currentDate = currentDate.AddMonths(1);
                }

                return new
                {
                    labels = labels.ToArray(),
                    data = data.ToArray(),
                    period = period
                };
            }
            else
            {
                // Nhóm theo ngày
                var dailyData = logs
                    .GroupBy(p => p.PracticeDate.Date)
                    .Select(g => new
                    {
                        Date = g.Key,
                        Minutes = g.Sum(p => p.Minutes)
                    })
                    .ToList();

                // Tạo danh sách đầy đủ các ngày
                var labels = new List<string>();
                var data = new List<int>();

                for (int i = 0; i < daysCount; i++)
                {
                    var date = startDate.AddDays(i);
                    labels.Add(labelFormatter(date));
                    var dayData = dailyData.FirstOrDefault(d => d.Date.Date == date.Date);
                    data.Add(dayData != null ? dayData.Minutes : 0);
                }

                return new
                {
                    labels = labels.ToArray(),
                    data = data.ToArray(),
                    period = period
                };
            }
        }

        #endregion
    }
}

