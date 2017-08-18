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
    public static class SessionBackendMiddlewareExtensions
    {
        public static IApplicationBuilder UseSessionBackend(this IApplicationBuilder app, IConfigurationSection sessionSection)
        {
            if (sessionSection == null)
            {
                return app;
            }
            
            var config = new SessionConfig();
            sessionSection.Bind(config);

            return app.UseMiddleware<SessionBackendMiddleware>(config.Url, config.Async, config.CookieName);
        }

        private class SessionConfig
        {
            public string Url { get; set; }
            
            public bool Async { get; set; }

            public string CookieName { get; set; }
        }
    }
}