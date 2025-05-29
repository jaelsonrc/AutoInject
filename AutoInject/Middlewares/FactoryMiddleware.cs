using Microsoft.AspNetCore.Http;

namespace AutoInject.Middlewares
{
    /// <summary>
    /// Middleware to automatically dispose Factory scopes at the end of each request
    /// </summary>
    public class FactoryMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Execute the next middleware in the pipeline
                await _next(context);
            }
            finally
            {
                // Dispose the current request scope at the end
                Factory.DisposeRequestScope();
            }
        }
    }
