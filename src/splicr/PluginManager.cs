using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Runtime.Loader;

namespace Splicr
{
    public static class PluginManager
    {
        public static IDictionary<string, IBackend> Backends = new Dictionary<string, IBackend>(StringComparer.OrdinalIgnoreCase);

        public static void LoadDirectory(string path, IDictionary<string, string> config)
        {
            var files = Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories).ToList();
            foreach (string file in files)
            {
                LoadFile(Path.GetFullPath(file), config);
            }
        }

        private static void LoadFile(string file, IDictionary<string, string> config)
        {
            var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
            foreach (Type type in myAssembly.GetExportedTypes())
            {
                if (type.Name.StartsWith("Backend"))
                {
                    var backend = new PluginBackend(type, config);
                    Backends.Add(type.AssemblyQualifiedName, backend);
                }
            }
        }
    }
}