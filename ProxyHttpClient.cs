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
            var requestMessage = new HttpRequestMessage();
            var requestMethod = httpContext.Request.Method;
            
            if (!HttpMethods.IsGet(requestMethod)&&
                !HttpMethods.IsHead(requestMethod) &&
                !HttpMethods.IsDelete(requestMethod)&&
                !HttpMethods.IsTrace(requestMethod))
            {
                var streamContent = new StreamContent(httpContext.Request.Body);
                requestMessage.Content = streamContent;
            }

            _httpClient.DefaultRequestHeaders.Clear();

            CopyRequestHeaders(
                httpContext.Request.Headers, 
                requestMessage.Headers, 
                requestMessage.Content?.Headers,
                new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "host" } // Use HOST header for backend: it might use it for routing
            );

            requestMessage.RequestUri = new Uri(url);
            requestMessage.Method = new HttpMethod(requestMethod);

            return await _httpClient.SendAsync(
                requestMessage, 
                HttpCompletionOption.ResponseHeadersRead,
                httpContext.RequestAborted);
        }

        public static void CopyResponseHeaders(IEnumerable<KeyValuePair<string, IEnumerable<string>>> source, IHeaderDictionary dest)
        {
            foreach (var pair in source) {
                // Console.WriteLine($"copying response header: {pair.Key}: {string.Join(", ", pair.Value)}");

                dest.Add(pair.Key, string.Join(", ", pair.Value));
            }   
        }

        public static void CopyRequestHeaders(IHeaderDictionary source, HttpHeaders dest, HttpHeaders contentDest, ISet<string> exclude = null)
        {
            foreach (var pair in source) 
            {
                if (exclude != null && exclude.Contains(pair.Key)) {
                    continue;
                }

                if (!dest.TryAddWithoutValidation(pair.Key, pair.Value.ToArray()) && contentDest != null)
                {
                    contentDest.TryAddWithoutValidation(pair.Key, pair.Value.ToArray());
                }
            }   
        }
    }
}
