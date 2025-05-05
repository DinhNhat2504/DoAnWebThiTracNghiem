using DoAnWebThiTracNghiem.Data;
using DoAnWebThiTracNghiem.Middleware;
using DoAnWebThiTracNghiem.Repositories;
using DoAnWebThiTracNghiem.Services;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<AppDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
           .EnableSensitiveDataLogging()); // B?t logging chi ti?t


builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IUserRepository, EFUserRepository>();
builder.Services.AddScoped<ISubjectRepository, EFSubjectRepository>();
builder.Services.AddScoped<IClassTnRepository, EFClassTnRepository>();
builder.Services.AddScoped<IExamRepository, EFExamRepository>();
builder.Services.AddScoped<IQuestionRepository, EFQuestionRepository>();
builder.Services.AddSingleton<ChatbotService>();

builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 10 * 1024 * 1024;
});
builder.Services.AddTransient<EmailService>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}
app.UseStaticFiles();
app.UseSession();
app.UseRouting();
app.UseMiddleware<AdminAuthorizationMiddleware>();
app.UseMiddleware<TeacherAuthorizationMiddleware>();
app.UseMiddleware<StudentAuthorizationMiddleware>();
app.UseAuthorization();
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
