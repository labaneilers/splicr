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
    public class Layout
    {
        private string _templateUrl;
        private string _templateStart;
        private string _templateEnd;
        private string _name;

        private static object _lock = new Object();

        public Layout(string name, string templateUrl)
        {
            _name = name;
            _templateUrl = templateUrl;
        }

        private void Load()
        {
            var httpClient = new HttpClient();
            HttpResponseMessage response = httpClient.GetAsync(_templateUrl).Result;
            response.EnsureSuccessStatusCode();
            string content = response.Content.ReadAsStringAsync().Result;

            const string CONTENT_TOKEN = "{{content}}";
            int contentPos = content.IndexOf(CONTENT_TOKEN);
            if (contentPos < 0) 
            {
                throw new Exception("Layout content invalid: No content region found");
            }

            _templateStart = content.Substring(0, contentPos);
            _templateEnd = content.Substring(contentPos + CONTENT_TOKEN.Length);
        }

        private void EnsureTemplate()
        {
            if (_templateStart == null)
            {
                lock (_lock)
                {
                    if (_templateStart == null)
                    {
                        Load();
                    }
                }
            }
        }

        public async Task WriteHtmlHeader(HttpContext httpContext, HttpResponseMessage response)
        {
            EnsureTemplate();

            await httpContext.Response.WriteAsync(_templateStart);
        } 

        public async Task WriteHtmlFooter(HttpContext httpContext, HttpResponseMessage response)
        {
            EnsureTemplate();

            await httpContext.Response.WriteAsync(_templateEnd);
        } 
    }
}
