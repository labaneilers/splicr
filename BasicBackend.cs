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
    public class BasicBackend : IBackend
    {
        public bool ShouldHandle(HttpRequest request)
        {
            return true;
        }

        public async Task WriteHtmlHeader(HttpContext httpContext, HttpResponseMessage response)
        {
            if (response.Content.Headers?.ContentType?.MediaType != "text/html") {
                return;
            }

            await httpContext.Response.WriteAsync("<!-- start proxy -->");
        } 

        public async Task WriteHtmlFooter(HttpContext httpContext, HttpResponseMessage response)
        {
            if (response.Content.Headers?.ContentType?.MediaType != "text/html") {
                return;
            }

            await httpContext.Response.WriteAsync("<!-- end proxy -->");
        } 

        public string GetUrl(HttpRequest request)
        {
            string pathAndQuery = request.Path + request.QueryString;

            return $"http://www.labaneilers.com{pathAndQuery}";
        }
    }
}
