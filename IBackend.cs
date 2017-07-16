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
    public interface IBackend
    {
        bool ShouldHandle(HttpRequest request);

        Task WriteHtmlHeader(HttpContext httpContext, HttpResponseMessage response);

        Task WriteHtmlFooter(HttpContext httpContext, HttpResponseMessage response);
       
        string GetUrl(HttpRequest request);
        
    }
}
