using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DoAnWebThiTracNghiem.Middleware
{
    public class AdminAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminAuthorizationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra nếu URL thuộc khu vực Admin
            if (context.Request.Path.StartsWithSegments("/Admin"))
            {
                var userRole = context.Session.GetString("UserRole");
                if (string.IsNullOrEmpty(userRole) || userRole != "Admin")
                {
                    // Chuyển hướng đến trang đăng nhập nếu không phải Admin
                    context.Response.Redirect("/Users/Login");
                    return;
                }
            }

            await _next(context);
        }
    }
}
