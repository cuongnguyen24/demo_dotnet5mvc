using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using HoursTracker.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HoursTracker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            
            try
            {
                using (var scope = host.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    var context = services.GetRequiredService<HoursTrackerDbContext>();
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    
                    logger.LogInformation("Đang kiểm tra và cập nhật database...");
                    
                    // Chạy migrations tự động
                    context.Database.Migrate();
                    
                    logger.LogInformation("Database đã sẵn sàng!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cảnh báo: Không thể kết nối database. Lỗi: {ex.Message}");
                Console.WriteLine("Hãy kiểm tra connection string và đảm bảo SQL Server đang chạy.");
                Console.WriteLine("Hoặc chạy migration thủ công: dotnet ef database update");
                Console.WriteLine("Ứng dụng vẫn sẽ chạy nhưng có thể gặp lỗi khi truy cập database.\n");
            }
            
            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
