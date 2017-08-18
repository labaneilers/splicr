using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Splicr
{
    public class SessionBackendMiddleware
    {
        private readonly RequestDelegate _next;

        private string _url;

        private bool _isAsync;

        private string _cookieName;

        private ProxyHttpClient _proxyHttpClient;

        public SessionBackendMiddleware(RequestDelegate next, string url, bool isAsync = true, string cookieName = "sessionId", ProxyHttpClient proxyHttpClient = null)
        {
            _next = next;
            _url = url;
            _isAsync = isAsync;
            _cookieName = cookieName;
            _proxyHttpClient = proxyHttpClient ?? new ProxyHttpClient();
        }

        public async Task Invoke(HttpContext context)
        {
            string sessionId;
            if (!context.Request.Cookies.TryGetValue(_cookieName, out sessionId))
            {
                sessionId = await Create(context);

                context.Response.Cookies.Append(_cookieName, sessionId);
            }

            context.Items[_cookieName] = sessionId;

            await _next.Invoke(context);
        }

        public async Task<string> Create(HttpContext httpContext)
        {
            //  TODO: Use a cryptographic key
            string sessionId = Guid.NewGuid().ToString("N");

            // Notify a service that a new session is detected
            var task = _proxyHttpClient.Send(
                httpContext, 
                _url);

            if (_isAsync)
            {
                // Fire and forget
                task.Start();

                return sessionId;
            }
            else 
            {
                HttpResponseMessage response = await task.ContinueWith(t => t.Result);

                // TODO: do something with response?
                // Handle some errors, perhaps?
                // Set some cookies based on response?

                return sessionId;
            }
        }
    }
}
