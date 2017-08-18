using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Runtime.Loader;

namespace Splicr
{
    // Loads assemblies, recursively, from arbitrary directories
    public static class PluginLoader
    {
        public static void Load(IEnumerable<string> paths)
        {
            foreach (string path in paths)
            {
                var files = Directory
                    .EnumerateFiles(path, "*.dll", SearchOption.AllDirectories)
                    .Select(Path.GetFullPath);
                    
                foreach (string file in files)
                {
                    AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
                }
            }
        }

        public static void Load(IConfigurationSection config)
        {
            var dirs = new List<string>();
            config.Bind(dirs);

            Load(dirs);
        }
    }
}