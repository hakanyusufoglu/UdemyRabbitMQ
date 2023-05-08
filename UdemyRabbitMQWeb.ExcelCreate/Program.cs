using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UdemyRabbitMQWeb.ExcelCreate.Models;

namespace UdemyRabbitMQWeb.ExcelCreate
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            
            // Add services to the container.

            //sql ba�lant�s�n� ger�ekle�tiriyoruz.
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
            
            var app = builder.Build();

            using(var scope=app.Services.CreateScope())
            {
                var appDbContext=scope.ServiceProvider.GetRequiredService<AppDbContext>();
                //getrequiredservice kullanmam�z�n sebebi mutlaka bu servisin olmas� gerekti�ini vurgulamak i�in kullan�yoruz
                var userManager=scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                appDbContext.Database.Migrate();

                //Any() kay�t var ise true d�nd�r�r
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
            //identity ekledi�imiz i�in tan�mlad�k
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}