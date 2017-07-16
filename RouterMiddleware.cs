using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
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

        private void CopyResponseHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> source, IHeaderDictionary dest)
        {
            foreach (var pair in source) {
                Console.WriteLine($"copying response header: {pair.Key}: {string.Join(", ", pair.Value)}");

                dest.Add(pair.Key, string.Join(", ", pair.Value));
            }   
        }

        private void CopyRequestHeaders(IHeaderDictionary source, HttpHeaders dest, ISet<string> exclude = null)
        {
            foreach (var pair in source) {
                if (exclude != null && exclude.Contains(pair.Key)) {
                    continue;
                }

                Console.WriteLine($"copying response header: {pair.Key}: {string.Join(", ", pair.Value)}");

                dest.Add(pair.Key, string.Join(", ", pair.Value));
            }   
        }

        private IBackend GetBackend(HttpRequest request)
        {
            return new BasicBackend();
        }

        public async Task Invoke(HttpContext httpContext)
        {
            Console.WriteLine($"Request for {httpContext.Request.Path} received ({httpContext.Request.ContentLength ?? 0} bytes)");
            
            try
            {
                var httpClient = new HttpClient();

                string pathAndQuery = httpContext.Request.Path + httpContext.Request.QueryString;

                IBackend backend = GetBackend(httpContext.Request);

                httpClient.DefaultRequestHeaders.Clear();

                CopyRequestHeaders(
                    httpContext.Request.Headers, 
                    httpClient.DefaultRequestHeaders, 
                    new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "host" } // Use HOST header for backend: it might use it for routing
                );

                HttpResponseMessage response = await httpClient.GetAsync(
                    backend.GetUrl(httpContext.Request), 
                    HttpCompletionOption.ResponseHeadersRead);

                httpContext.Response.StatusCode = (int)response.StatusCode;
                httpContext.Response.Headers.Clear();

                CopyResponseHeaders(response.Headers, httpContext.Response.Headers);
                CopyResponseHeaders(response.Content.Headers, httpContext.Response.Headers);

                await backend.WriteHtmlHeader(httpContext, response);

                await response.Content.CopyToAsync(httpContext.Response.Body);

                await backend.WriteHtmlFooter(httpContext, response);

                Console.WriteLine();
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = 500;
                await httpContext.Response.WriteAsync("Error: " + ex.Message);
            }

            // Call the next middleware delegate in the pipeline 
            //await _next.Invoke(httpContext);
        }
    }
}