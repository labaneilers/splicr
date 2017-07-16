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

        private void CopyHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> source, IHeaderDictionary dest)
        {
            foreach (var pair in source) {
                Console.WriteLine($"copying response header: {pair.Key}: {string.Join(", ", pair.Value)}");

                dest.Add(pair.Key, string.Join(", ", pair.Value));
            }   
        }

        public async Task Invoke(HttpContext httpContext)
        {
            Console.WriteLine($"Request for {httpContext.Request.Path} received ({httpContext.Request.ContentLength ?? 0} bytes)");
            
            try
            {
                var httpClient = new HttpClient();

                string pathAndQuery = httpContext.Request.Path + httpContext.Request.QueryString;

                httpClient.DefaultRequestHeaders.Clear();

                foreach (var pair in httpContext.Request.Headers) 
                {
                    if (pair.Key.Equals("host", StringComparison.OrdinalIgnoreCase)) {
                        continue;
                    }

                    Console.WriteLine($"request: {pair.Key}: {string.Join(", ", pair.Value)}");
                    httpClient.DefaultRequestHeaders.Add(pair.Key, string.Join(", ", pair.Value));
                }

                HttpResponseMessage response = await httpClient.GetAsync(
                    $"http://www.labaneilers.com{pathAndQuery}", 
                    HttpCompletionOption.ResponseHeadersRead);

                httpContext.Response.StatusCode = (int)response.StatusCode;

                CopyHeaders(response.Headers, httpContext.Response.Headers);
                CopyHeaders(response.Content.Headers, httpContext.Response.Headers);

                await response.Content.CopyToAsync(httpContext.Response.Body);

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