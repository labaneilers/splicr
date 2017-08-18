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
    public class LayoutRegistry
    {
        private Layout _defaultLayout;

        private Dictionary<string, Layout> _layouts = new Dictionary<string, Layout>(StringComparer.OrdinalIgnoreCase);

        public void Register(string name, string url, bool isDefault = false)
        {
            var layout = new Layout(name, url);
            _layouts.Add(name, layout);

            if (isDefault) {
                _defaultLayout = layout;
            }
        }

        public Layout Get(HttpHeaders headers)
        {
            IEnumerable<string> values;
            if (headers.TryGetValues("x-splicr-layout", out values))
            {
                return Get(values.FirstOrDefault());
            }

            return _defaultLayout;
        }

        public Layout Get(string name)
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
