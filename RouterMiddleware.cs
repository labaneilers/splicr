using System;
using System.Net.Http;
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

                HttpResponseMessage response = await httpClient.GetAsync($"http://www.labaneilers.com{pathAndQuery}");
                string responseBody = await response.Content.ReadAsStringAsync();
                httpContext.Response.StatusCode = (int)response.StatusCode;

                foreach (var pair in response.Headers) {
                    httpContext.Response.Headers.Add(pair.Key, string.Join(", ", pair.Value));
                }

                await httpContext.Response.WriteAsync(responseBody);

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