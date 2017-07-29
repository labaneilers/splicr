using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Splicr
{
    public class ConfigurationLoader
    {
        public static void Load(IConfigurationRoot config)
        {
            LoadPlugins(config);
            LoadBackends(config);
            LoadLayouts(config);
            LoadSession(config);
        }

        private static void LoadPlugins(IConfigurationRoot config)
        {
            var plugins = new List<PluginConfig>();
            config.GetSection("plugins").Bind(plugins);

            foreach (PluginConfig pluginConfig in plugins)
            {
                PluginManager.LoadDirectory(pluginConfig.Dir, pluginConfig.Data);
            }
        }

        private static void LoadBackends(IConfigurationRoot config)
        {
            var backends = new List<BackendConfig>();
            config.GetSection("backends").Bind(backends);

            foreach (BackendConfig backendConfig in backends)
            {
                IBackend backend = null;
                if (backendConfig.Type == "regex")
                {
                    backend = new RegexBackend(backendConfig.Data["hostname"], backendConfig.Data["match"], backendConfig.Data["replace"]);
                }
                else if (backendConfig.Type == "plugin")
                {
                    if (!PluginManager.Backends.TryGetValue(backendConfig.Data["classname"], out backend))
                    {
                        throw new Exception($"Couldn't find plugin type: {backendConfig.Data["classname"]}");
                    }
                }
                else
                {
                    throw new Exception($"backend configuration type {backendConfig.Type} not valid");
                }

                BackendRegistry.Register(backend);
            }
        }

        private static void LoadLayouts(IConfigurationRoot config)
        {
            var layouts = new List<LayoutConfig>();
            config.GetSection("layouts").Bind(layouts);

            foreach (LayoutConfig layoutConfig in layouts)
            {
                LayoutRegistry.Register(layoutConfig.Name, layoutConfig.Url, layoutConfig.Default);
            }
        }

        private static void LoadSession(IConfigurationRoot config)
        {
            var session = new SessionConfig();
            config.GetSection("session").Bind(session);

            SessionCreator.Instance = new SessionCreator(session.Url, session.Async);
        }
    }

    public class BackendConfig
    {
        public string Type { get; set; }

        public Dictionary<string, string> Data { get; set; }
    }

    public class LayoutConfig
    {
        public string Name { get; set; }

        public string Url { get; set; }

        public bool Default { get; set; }
    }

    public class SessionConfig
    {
        public string Url { get; set; }
        
        public bool Async { get; set; }
    }

    public class PluginConfig
    {
        public string Dir { get; set; }

        public Dictionary<string, string> Data { get; set; }
    }

}