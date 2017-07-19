using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Splicr
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddResponseCompression();

            // TODO convert to DI injected
            BackendRegistry.Register(new RegexBackend("http://localhost:5001", @"^\/content1\/(.*)", "/$1"));
            BackendRegistry.Register(new RegexBackend("http://www.labaneilers.com", @"^\/laban\/(.*)", "/$1"));
            BackendRegistry.Register(new RegexBackend("http://www.labaneilers.com", @"(.*)", "$1"));

            LayoutRegistry.Register("standard", "http://localhost:5001/standard.html", true);
            LayoutRegistry.Register("lite", "http://localhost:5001/lite.html", false);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            app.UseResponseCompression();            
            app.UseMiddleware<ProxyMiddleware>();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
        }
    }
}
