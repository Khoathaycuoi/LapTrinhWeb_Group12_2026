using BTL_NhaKhoa.Data;
using BTL_NhaKhoa.DTOs;
using BTL_NhaKhoa.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace BTL_NhaKhoa
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            var connectionString = builder.Configuration
                .GetConnectionString("NhaKhoa")
                ?? throw new InvalidOperationException("Chưa cấu hình kết nối MySQL.");

            builder.Services.AddDbContext<NhaKhoaDbContext>(options =>
                options.UseMySQL(connectionString));

            var jwtKey = builder.Configuration["Jwt:Key"]
                ?? throw new InvalidOperationException("Chưa cấu hình Jwt:Key trong User Secrets.");
            byte[] keyBytes;
            try
            {
                keyBytes = Convert.FromBase64String(jwtKey);
            }
            catch (FormatException exception)
            {
                throw new InvalidOperationException("Jwt:Key phải là chuỗi Base64.", exception);
            }
            if (keyBytes.Length < 32)
            {
                throw new InvalidOperationException("Jwt:Key phải có ít nhất 32 byte sau khi giải mã Base64.");
            }

            const string jwtIssuer = "BTL_NhaKhoa";
            const string jwtAudience = "BTL_NhaKhoa_React";
            var signingKey = new SymmetricSecurityKey(keyBytes);

            builder.Services.AddScoped<DangNhapService>();
            builder.Services.AddScoped<DangKyService>();
            builder.Services.AddScoped<TaiKhoanCaNhanService>();
            builder.Services.AddScoped<QuanLyNguoiDungService>();
            builder.Services.AddScoped<QuanLyDichVuService>();
            builder.Services.AddScoped<HuyLichHenService>();
            builder.Services.AddScoped<QuanLyLichHenService>();
            builder.Services.AddScoped<QuanLyPhongService>();
            builder.Services.AddScoped<QuanLyKhamBenhService>();
            builder.Services.AddScoped<QuanLyHoaDonService>();
            builder.Services.AddScoped<QuanLyChamCongService>();
            builder.Services.AddScoped<DashboardService>();
            builder.Services.AddHostedService<HetHanLichChoService>();
            builder.Services.AddSingleton(new CauHinhJwt(signingKey, jwtIssuer, jwtAudience));
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.MapInboundClaims = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = jwtAudience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = "sub",
                        RoleClaimType = "vai_tro"
                    };
                    options.Events = new JwtBearerEvents
                    {
                        OnTokenValidated = async context =>
                        {
                            if (!ulong.TryParse(context.Principal?.FindFirstValue("ma_tai_khoan"), out var maTaiKhoan))
                            {
                                context.Fail("Token không hợp lệ.");
                                return;
                            }

                            var db = context.HttpContext.RequestServices.GetRequiredService<NhaKhoaDbContext>();
                            var taiKhoan = await db.TaiKhoans.AsNoTracking().Include(x => x.ConNguoi)
                                .FirstOrDefaultAsync(x => x.MaTaiKhoan == maTaiKhoan);
                            if (taiKhoan == null || taiKhoan.TrangThai != "HOAT_DONG" ||
                                taiKhoan.MaVaiTro != context.Principal?.FindFirstValue("vai_tro") ||
                                taiKhoan.ConNguoi.SoDienThoai != context.Principal?.FindFirstValue("sub"))
                            {
                                context.Fail("Tài khoản không còn hiệu lực.");
                            }
                        }
                    };
                });
            builder.Services.AddAuthorization(options =>
            {
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
                options.AddPolicy("QuanLy", policy => policy.RequireRole("QUAN_LY"));
                options.AddPolicy("LeTan", policy => policy.RequireRole("LE_TAN"));
                options.AddPolicy("BacSi", policy => policy.RequireRole("BAC_SI"));
                options.AddPolicy("KhachHang", policy => policy.RequireRole("KHACH_HANG"));
            });
            builder.Services.AddCors(options => options.AddPolicy("React", policy =>
                policy.WithOrigins("http://localhost:5173")
                    .AllowAnyHeader()
                    .AllowAnyMethod()));

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(1);
                options.Cookie.Name = "NhaKhoa.BookingDraft";
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.SameSite = SameSiteMode.Lax;
                options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
                    ? CookieSecurePolicy.None : CookieSecurePolicy.Always;
            });

            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            var app = builder.Build();

            var soDienThoaiQuanLy = builder.Configuration["KhoiTaoQuanLy:SoDienThoai"];
            var hoTenQuanLy = builder.Configuration["KhoiTaoQuanLy:HoTen"];
            var matKhauQuanLy = builder.Configuration["KhoiTaoQuanLy:MatKhau"];
            var coCauHinhQuanLy = !string.IsNullOrWhiteSpace(soDienThoaiQuanLy) ||
                !string.IsNullOrWhiteSpace(hoTenQuanLy) ||
                !string.IsNullOrWhiteSpace(matKhauQuanLy);
            if (coCauHinhQuanLy)
            {
                if (string.IsNullOrWhiteSpace(soDienThoaiQuanLy) ||
                    string.IsNullOrWhiteSpace(hoTenQuanLy) ||
                    string.IsNullOrWhiteSpace(matKhauQuanLy))
                {
                    throw new InvalidOperationException("KhoiTaoQuanLy cần đủ SoDienThoai, HoTen và MatKhau.");
                }

                using var scope = app.Services.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<NhaKhoaDbContext>();
                if (!db.TaiKhoans.Any(x => x.MaVaiTro == "QUAN_LY"))
                {
                    var dangKy = scope.ServiceProvider.GetRequiredService<DangKyService>();
                    var ketQua = dangKy.DangKyAsync(new DangKyRequest
                    {
                        SoDienThoai = soDienThoaiQuanLy,
                        HoTen = hoTenQuanLy,
                        MatKhau = matKhauQuanLy
                    }, "QUAN_LY").GetAwaiter().GetResult();
                    if (ketQua.Loi != null)
                    {
                        throw new InvalidOperationException($"Không thể tạo Quản lý đầu tiên: {ketQua.Loi}");
                    }
                }
            }

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseCors("React");
            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
