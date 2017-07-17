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
    public static class BackendRegistry
    {
        private static List<IBackend> _backends = new List<IBackend>();

        public static void Register(IBackend backend)
        {
            _backends.Add(backend);
        }

        public static string GetUrl(HttpRequest request)
        {
            foreach (IBackend backend in _backends)
            {
                string url = backend.GetUrl(request);
                if (url != null)
                {
                    return url;
                }
            }
            return null;
        }
    }
}
