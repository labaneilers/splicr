using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Splicr
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var host = new WebHostBuilder()
                .UseKestrel()
                .UseConfiguration(config)
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseIISIntegration()
                //.ConfigureLogging(l => l.AddConsole(config.GetSection("Logging")))
                .ConfigureServices(s => s.AddRouting())
                .Configure(app =>
                {
                    // to do - wire in our HTTP endpoints
                    app.UseMiddleware<ProxyMiddleware>();
                })
                .Build();

            // TODO: Create a configuration system for this
            
            BackendRegistry.Register(new RegexBackend("http://localhost:5001", @"^\/content1\/(.*)", "/$1"));
            BackendRegistry.Register(new RegexBackend("http://localhost:5002", @"^\/content2\/(.*)", "/$1"));

            LayoutRegistry.Register("standard", "http://localhost:5001/standard.html", true);
            LayoutRegistry.Register("lite", "http://localhost:5001/lite.html", false);

            host.Run();
        }
    }
}
