using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Splicr
{
    public class ProxyHttpClient
    {
        private static HttpClient _httpClient = new HttpClient();

        public static async Task<HttpResponseMessage> Send(HttpContext httpContext, string url)
        {
            _httpClient.DefaultRequestHeaders.Clear();

            CopyRequestHeaders(
                httpContext.Request.Headers, 
                _httpClient.DefaultRequestHeaders, 
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "host" } // Use HOST header for backend: it might use it for routing
            );

            var method = new HttpMethod(httpContext.Request.Method);
            var message = new HttpRequestMessage(method, url);
            if (method.Method == "POST" || method.Method == "PUT")
            {
                message.Content = new StreamContent(httpContext.Request.Body);
            }

            return await _httpClient.SendAsync(
                message, 
                HttpCompletionOption.ResponseHeadersRead);
        }

        public static void CopyResponseHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> source, IHeaderDictionary dest)
        {
            foreach (var pair in source) {
                // Console.WriteLine($"copying response header: {pair.Key}: {string.Join(", ", pair.Value)}");

                dest.Add(pair.Key, string.Join(", ", pair.Value));
            }   
        }

        public static void CopyRequestHeaders(IHeaderDictionary source, HttpHeaders dest, ISet<string> exclude = null)
        {
            foreach (var pair in source) {
                if (exclude != null && exclude.Contains(pair.Key)) {
                    continue;
                }

                //Console.WriteLine($"copying response header: {pair.Key}: {string.Join(", ", pair.Value)}");

                try
                {
                    dest.Add(pair.Key, string.Join(", ", pair.Value));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error writing header: {pair.Key} {pair.Value} {ex.Message}");
                }
            }   
        }
    }
}
