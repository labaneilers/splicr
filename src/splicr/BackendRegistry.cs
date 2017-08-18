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
    public class BackendRegistry
    {
        private List<IBackend> _backends = new List<IBackend>();

        public BackendRegistry(IEnumerable<IBackend> backends)
        {
            _backends.AddRange(backends);
        }

        public void Register(IBackend backend)
        {
            _backends.Add(backend);
        }

        public string GetUrl(HttpRequest request)
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
