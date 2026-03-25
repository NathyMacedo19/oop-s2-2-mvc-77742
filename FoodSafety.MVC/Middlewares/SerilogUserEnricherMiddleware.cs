using Serilog.Context;

namespace FoodSafety.MVC.Middlewares
{
    public class SerilogUserEnricherMiddleware
    {
        private readonly RequestDelegate _next;

        public SerilogUserEnricherMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Try to get the logged-in user's name
            // If no user is logged in, use "Anonymous"
            var userName = context.User?.Identity?.Name ?? "Anonymous";

            // Add UserName to all logs during this request
            using (LogContext.PushProperty("UserName", userName))
            {
                // Continue processing the request
                await _next(context);
            }
        }
    }
}