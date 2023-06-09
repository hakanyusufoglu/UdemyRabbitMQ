using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using UdemyRabbitMQWeb.ExcelCreate.Hubs;
using UdemyRabbitMQWeb.ExcelCreate.Models;
using UdemyRabbitMQWeb.ExcelCreate.Services;

namespace UdemyRabbitMQWeb.ExcelCreate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            //BackgroundService Consumer Received içerisinde asenkron bir methot kullandığımız için DispatchConsumersAsync=true olarak ayarlandı.
            builder.Services.AddSingleton(sp => new ConnectionFactory() { Uri = new Uri(builder.Configuration.GetConnectionString("RabbitMQ")), DispatchConsumersAsync = true });
            builder.Services.AddSingleton<RabbitMQPublisher>();
            builder.Services.AddSingleton<RabbitMQClientService>();
            //sql bağlantısını gerçekleştiriyoruz.
            builder.Services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
            });

            //identity ekliyoruz
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.User.RequireUniqueEmail= true;
            }).AddEntityFrameworkStores<AppDbContext>();
            builder.Services.AddControllersWithViews();
            builder.Services.AddSignalR();
            var app = builder.Build();

            using(var scope=app.Services.CreateScope())
            {
                var appDbContext=scope.ServiceProvider.GetRequiredService<AppDbContext>();
                //getrequiredservice kullanmamızın sebebi mutlaka bu servisin olması gerektiğini vurgulamak için kullanıyoruz
                var userManager=scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                appDbContext.Database.Migrate();

                //Any() kayıt var ise true döndürür
                if (!appDbContext.Users.Any())
                {
                    userManager.CreateAsync(new IdentityUser()
                    {
                        UserName = "deneme",
                        Email = "deneme@outlook.com"
                    }, "Password12*").Wait();

                    userManager.CreateAsync(new IdentityUser()
                    {
                        UserName = "deneme2",
                        Email = "deneme2@outlook.com"
                    }, "Password12*").Wait();
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            //identity eklediğimiz için tanımladık
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
            app.MapHub<MyHub>("/MyHub"); //bunun ile signalr ile iletişime geçilecek
            app.Run();
        }
    }
}