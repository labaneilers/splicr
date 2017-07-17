using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Splicr 
{
    public class ProxyMiddleware
    {
        private readonly RequestDelegate _next;

        public ProxyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private IBackend GetBackend(HttpRequest request)
        {
            return new BasicBackend("http://localhost:5001/standard.html");
        }

        private ISessionCreator GetSessionCreator()
        {
            return new SyncSessionCreator();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            // Console.WriteLine($"Request for {httpContext.Request.Path} received ({httpContext.Request.ContentLength ?? 0} bytes)");
            
            try
            {   
                const string SESSION_KEY = "splicrSessionId";

                string sessionId;
                if (!httpContext.Request.Cookies.TryGetValue(SESSION_KEY, out sessionId))
                {
                    ISessionCreator sessionCreator = GetSessionCreator();
                    sessionId = await sessionCreator.Create(httpContext);
                }

                IBackend backend = GetBackend(httpContext.Request);

                HttpResponseMessage response = await ProxyHttpClient.Send(
                    httpContext, 
                    backend.GetUrl(httpContext.Request));

                httpContext.Response.StatusCode = (int)response.StatusCode;

                Console.WriteLine($"{response.Content.Headers.ContentLength}: {httpContext.Request.Path}");

                httpContext.Response.Headers.Clear();

                //httpContext.Response.Cookies.Append(SESSION_KEY, sessionId);

                ProxyHttpClient.CopyResponseHeaders(response.Headers, httpContext.Response.Headers);
                ProxyHttpClient.CopyResponseHeaders(response.Content.Headers, httpContext.Response.Headers);

                httpContext.Response.ContentLength = null;

                await backend.WriteHtmlHeader(httpContext, response);

                await response.Content.CopyToAsync(httpContext.Response.Body);

                await backend.WriteHtmlFooter(httpContext, response);

                //Console.WriteLine();
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.ContentType = "text/plain";
                await httpContext.Response.WriteAsync("Error: " + ex.Message);
                await httpContext.Response.WriteAsync("Source: " + ex.Source);
                await httpContext.Response.WriteAsync("Stack: " + ex.StackTrace);
            }

            // Call the next middleware delegate in the pipeline 
            //await _next.Invoke(httpContext);
        }
    }
}