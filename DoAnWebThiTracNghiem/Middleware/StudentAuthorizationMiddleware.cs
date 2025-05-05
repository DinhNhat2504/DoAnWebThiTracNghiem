using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DoAnWebThiTracNghiem.Middleware
{
    public class StudentAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public StudentAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra nếu URL thuộc khu vực Student
            if (context.Request.Path.StartsWithSegments("/Student"))
            {
                var userRole = context.Session.GetString("UserRole");
                if (string.IsNullOrEmpty(userRole) || userRole != "Student")
                {
                    // Chuyển hướng đến trang đăng nhập nếu không phải Student
                    context.Response.Redirect("/Users/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}
