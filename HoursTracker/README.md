# 1.000 Hours Tracker

Web application ASP.NET Core 5 MVC để theo dõi quá trình luyện tập đến 1.000 giờ.

## Tính năng

- ✅ Quản lý danh sách kỹ năng (Skills): thêm, sửa, xóa, chọn màu hiển thị
- ✅ Ghi log luyện tập (Practice Logs): ngày luyện tập + số phút + ghi chú
- ✅ Tính tổng thời gian đã luyện tập (phút → giờ)
- ✅ Tính % hoàn thành mục tiêu 1.000 giờ (có thể chỉnh sửa)
- ✅ Hiển thị tiến độ bằng progress bar và circular progress
- ✅ Trang "Skill Detail" với:
  - Progress (% + tổng giờ)
  - Danh sách log theo ngày
  - Biểu đồ (Chart.js) - 30 ngày gần nhất
  - Danh sách milestone (100h, 250h, 500h, 750h, 1000h, 1500h, 2000h, 2500h, 3000h)
- ✅ Tự động kiểm tra milestone khi thêm log
- ✅ Popup chúc mừng với animation khi đạt milestone mới

## Yêu cầu

- .NET 5.0 SDK
- SQL Server hoặc SQL Server LocalDB
- Visual Studio 2019/2022 hoặc Visual Studio Code

## Cài đặt và Chạy

### 1. Khôi phục packages

```bash
dotnet restore
```

### 2. Cấu hình Connection String

Mở file `appsettings.json` và cập nhật connection string cho SQL Server của bạn:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=HoursTracker;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

**Lưu ý:** 
- Nếu dùng LocalDB: `Server=(localdb)\\mssqllocaldb;Database=HoursTracker;Trusted_Connection=True;MultipleActiveResultSets=true`
- Nếu dùng SQL Server Express: `Server=.\\SQLEXPRESS;Database=HoursTracker;Trusted_Connection=True;MultipleActiveResultSets=true`
- Nếu dùng SQL Server với username/password: `Server=YOUR_SERVER;Database=HoursTracker;User Id=YOUR_USER;Password=YOUR_PASSWORD;`

### 3. Tạo Database và Migrations

```bash
# Tạo migration
dotnet ef migrations add InitialCreate

# Cập nhật database
dotnet ef database update
```

### 4. Chạy ứng dụng

```bash
dotnet run
```

Hoặc nhấn F5 trong Visual Studio.

Ứng dụng sẽ chạy tại: `https://localhost:5001` hoặc `http://localhost:5000`

## Cấu trúc Project

```
HoursTracker/
├── Controllers/
│   ├── HomeController.cs
│   ├── SkillsController.cs      # Quản lý kỹ năng
│   └── PracticeLogsController.cs # Quản lý log luyện tập
├── Models/
│   ├── Skill.cs                 # Model kỹ năng
│   ├── PracticeLog.cs           # Model log luyện tập
│   └── Milestone.cs             # Model milestone
├── Data/
│   └── HoursTrackerDbContext.cs # DbContext
├── Views/
│   ├── Skills/                  # Views cho Skills
│   └── PracticeLogs/            # Views cho Practice Logs
└── wwwroot/
    ├── css/
    │   └── site.css             # Custom styles
    └── js/
        ├── circular-progress.js # Circular progress animation
        ├── skill-detail.js     # Chart initialization
        ├── practice-log-form.js # Milestone detection
        └── skill-form.js       # Form handling
```

## Công nghệ sử dụng

- **Backend:** ASP.NET Core 5 MVC
- **Database:** SQL Server với Entity Framework Core 5
- **Frontend:** Bootstrap 5, Bootstrap Icons
- **Charts:** Chart.js 3.9.1
- **JavaScript:** Vanilla JS (tách file riêng)

## Ghi chú

- Mục tiêu mặc định là 1000 giờ nhưng có thể chỉnh sửa cho mỗi kỹ năng
- Milestones tự động được tạo khi đạt được: 100h, 250h, 500h, 750h, 1000h, 1500h, 2000h, 2500h, 3000h
- Biểu đồ hiển thị dữ liệu 30 ngày gần nhất
- Circular progress và linear progress bar đều có animation

## License

MIT

