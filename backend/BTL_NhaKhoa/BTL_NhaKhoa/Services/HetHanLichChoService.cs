using BTL_NhaKhoa.Data;
using Microsoft.EntityFrameworkCore;

namespace BTL_NhaKhoa.Services;

public sealed class HetHanLichChoService(IServiceScopeFactory scopeFactory,
    ILogger<HetHanLichChoService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var db = scope.ServiceProvider.GetRequiredService<NhaKhoaDbContext>();
                var lucNay = DateTime.Now;
                await db.LichHens
                    .Where(x => x.TrangThai == "CHO_XAC_NHAN" && x.ThoiGianBatDau <= lucNay)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(x => x.TrangThai, "HET_HAN"),
                        stoppingToken);
            }
            catch (Exception exception) when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(exception, "Không thể cập nhật lịch chờ xác nhận đã hết hạn.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
