
using MessageWeb1.Hubs;
using MessageWeb1.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using MessageWeb1.Hubs;
using MessageWeb1.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using System;
// Thêm using
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using MessageWeb1.Controllers;
using static System.Collections.Specialized.BitVector32;

namespace MessageWeb1
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddDbContext<KloMessageContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnectionStringDB")));

            builder.Services.AddSignalR();
            builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            builder.Services.AddHttpContextAccessor();

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Login/Index";
                }); 


            var app = builder.Build();

            if (!app.Environment.IsDevelopment()) {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            var env = builder.Environment;


            if (env.IsDevelopment()) {
                app.UseDeveloperExceptionPage(); // bật để xem lỗi cụ thể
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();
            app.UseAuthentication();
            app.UseAuthorization();


            //            action là tên method trong controller.

            //Route mặc định { controller = Chat}/{ action = Index}
            //            nghĩa là:

            //Nếu không ghi cụ thể, hệ thống sẽ gọi:
            //ChatController.Index().
            app.MapControllerRoute(
          name: "default",
          pattern: "{controller=ScreenQR}/{action=Index}");


            app.MapHub<ChatHub>("/chatHub");

            app.Run();
        }
    }
}
