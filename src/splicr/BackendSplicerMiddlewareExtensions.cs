using System;
using System.Net;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Splicr 
{
    public static class BackendSplicerMiddlewareExtensions
    {
        public static IApplicationBuilder UseBackendSplicer(this IApplicationBuilder app, IConfigurationSection config)
        {
            var layoutRegistry = BuildLayoutRegistry(config);
            
            var backendRegistry = new BackendRegistry(GetBackends(config));

            return app.UseMiddleware<BackendSplicerMiddleware>(layoutRegistry, backendRegistry);
        }

        private static LayoutRegistry BuildLayoutRegistry(IConfigurationSection config)
        {
            var layouts = new List<LayoutConfig>();
            config.GetSection("Layouts").Bind(layouts);

            var layoutRegistry = new LayoutRegistry();
            foreach (LayoutConfig layoutConfig in layouts)
            {
                layoutRegistry.Register(layoutConfig.Name, layoutConfig.Url, layoutConfig.Default);
            }
            return layoutRegistry;
        }
        
        private static IEnumerable<IBackend> GetBackends(IConfigurationSection config)
        {
            var backends = new List<BackendConfig>();
            config.GetSection("Backends").Bind(backends);

            foreach (BackendConfig backendConfig in backends)
            {
                IBackend backend = null;
                if (backendConfig.Type == "regex")
                {
                    backend = new RegexBackend(backendConfig.Data["hostname"], backendConfig.Data["match"], backendConfig.Data["replace"]);
                }
                else if (backendConfig.Type == "plugin")
                {
                    Type pluginType = Type.GetType(backendConfig.Data["classname"], false, true);
                    if (pluginType == null)
                    {
                        throw new Exception($"Couldn't find plugin type: {backendConfig.Data["classname"]}");
                    }

                    backend = new PluginBackend(pluginType, backendConfig.Data);
                }
                else
                {
                    throw new Exception($"backend configuration type {backendConfig.Type} not valid");
                }

                yield return backend;
            }
        }

        private class LayoutConfig
        {
            public string Name { get; set; }

            public string Url { get; set; }

            public bool Default { get; set; }
        }

        private class BackendConfig
        {
            public string Type { get; set; }

            public Dictionary<string, string> Data { get; set; }
        }
    }
}