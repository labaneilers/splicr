using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Splicr
{
    public static class LayoutRegistry
    {
        private static Layout _defaultLayout;

        private static Dictionary<string, Layout> _layouts = new Dictionary<string, Layout>(StringComparer.OrdinalIgnoreCase);

        public static void Register(string name, string url, bool isDefault = false)
        {
            var layout = new Layout(name, url);
            _layouts.Add(name, layout);

            if (isDefault) {
                _defaultLayout = layout;
            }
        }

        public static Layout Get(HttpHeaders headers)
        {
            IEnumerable<string> values;
            if (headers.TryGetValues("x-splicr-layout", out values))
            {
                return Get(values.FirstOrDefault());
            }

            return _defaultLayout;
        }

        public static Layout Get(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                Layout layout;
                if (_layouts.TryGetValue(name, out layout))
                {
                    return layout;
                }
            }

            return _defaultLayout;
        }
    }
}
