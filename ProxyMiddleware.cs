using System;
using System.Net;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;

namespace Splicr 
{
    public class ProxyMiddleware
    {
        private readonly RequestDelegate _next;

        public ProxyMiddleware(RequestDelegate next)
        {
            _next = next;
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

                string backendUrl = BackendRegistry.GetUrl(httpContext.Request);

                HttpResponseMessage response = await ProxyHttpClient.Send(
                    httpContext, 
                    backendUrl);

                httpContext.Response.StatusCode = (int)response.StatusCode;

                httpContext.Response.Headers.Clear();

                httpContext.Response.Cookies.Append(SESSION_KEY, sessionId);

                ProxyHttpClient.CopyResponseHeaders(response.Headers, httpContext.Response.Headers);
                ProxyHttpClient.CopyResponseHeaders(response.Content.Headers, httpContext.Response.Headers);

                // TODO: Find all statuses that shouldn't write content
                // For a 304 response, don't write any content
                if (response.StatusCode == HttpStatusCode.NotModified)
                {
                    return;
                }

                // TODO: Something's occasionally not working with chunked encoding
                // Look into this
                httpContext.Response.ContentLength = null;

                Layout layout = LayoutRegistry.Get(response.Headers);

                await layout.WriteHtmlHeader(httpContext, response);

                await response.Content.CopyToAsync(httpContext.Response.Body);

                await layout.WriteHtmlFooter(httpContext, response);
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