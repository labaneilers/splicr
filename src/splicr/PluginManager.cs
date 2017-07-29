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
        public static IDictionary<string, Type> BackendTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

        public static void LoadDirectory(string path)
        {
            var files = Directory.EnumerateFiles(path, "*.dll", SearchOption.AllDirectories).ToList();
            foreach (string file in files)
            {
                LoadFile(Path.GetFullPath(file));
            }
        }

        private static void LoadFile(string file)
        {
            var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(file);
            foreach (Type type in myAssembly.GetExportedTypes())
            {
                if (type.Name.StartsWith("Backend"))
                {
                    BackendTypes.Add(type.AssemblyQualifiedName, type);
                }
            }
        }
    }
}