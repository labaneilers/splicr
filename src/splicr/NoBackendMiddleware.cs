using System;
using System.Net;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Builder;

namespace Splicr 
{
    public class NoBackendMiddleware
    {
        private readonly RequestDelegate _next;

        public NoBackendMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {                                         
            httpContext.Response.StatusCode = 500;
            httpContext.Response.ContentType = "text/plain";
            await httpContext.Response.WriteAsync($"Error: no backend configured for path {httpContext.Request.Path}\n");
        }
    }

    public static class NoBackendMiddlewareExtensions
    {
        public static IApplicationBuilder UseNoBackend(this IApplicationBuilder app)
        {
            return app.UseMiddleware<NoBackendMiddleware>();
        }
    }
}