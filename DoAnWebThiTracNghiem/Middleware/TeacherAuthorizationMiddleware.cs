using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DoAnWebThiTracNghiem.Middleware
{
    public class TeacherAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public TeacherAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra nếu URL thuộc khu vực Teacher
            if (context.Request.Path.StartsWithSegments("/Teacher"))
            {
                var userRole = context.Session.GetString("UserRole");
                if (string.IsNullOrEmpty(userRole) || userRole != "Teacher")
                {
                    // Chuyển hướng đến trang đăng nhập nếu không phải Teacher
                    context.Response.Redirect("/Users/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}
