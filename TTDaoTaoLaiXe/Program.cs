using Microsoft.EntityFrameworkCore;
using QuestPDF.Infrastructure;
using TTDaoTaoLaiXe.Models;
using TTDaoTaoLaiXe.Services.Implementations;
using TTDaoTaoLaiXe.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);
string matKhauMaHoa =
    TTDaoTaoLaiXe.Helpers.PasswordHelper.Hash("123456");

Console.WriteLine("MAT KHAU BCRYPT: " + matKhauMaHoa);

builder.Services.AddDbContext<TTDaoTaoLaiXeContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IVnPayService, VnPayService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddAuthentication(
    Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });
QuestPDF.Settings.License = LicenseType.Community;
var app = builder.Build();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();