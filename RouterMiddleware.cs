using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Splicr 
{
    public class RouterMiddleware
    {
        private readonly RequestDelegate _next;

        public RouterMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            Console.WriteLine($"Request for {httpContext.Request.Path} received ({httpContext.Request.ContentLength ?? 0} bytes)");
            
            // Call the next middleware delegate in the pipeline 
            await _next.Invoke(httpContext);
        }
    }
}