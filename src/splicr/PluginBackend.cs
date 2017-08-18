using System;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using System.Collections.Generic;

namespace Splicr
{
    class PluginBackend : IBackend
    {
        private Func<HttpRequest, string> _pointer;
        private object _instance;

        public PluginBackend(Type type, IDictionary<string, string> config)
        {
            ConstructorInfo ctor = type.GetConstructor(new [] { typeof(IDictionary<string, string>)});

            _instance = ctor.Invoke(new object[] { config });

            MethodInfo methodInfo = type.GetMethod("GetUrl");
            _pointer = (Func<HttpRequest, string>)methodInfo.CreateDelegate(typeof(Func<HttpRequest, string>), _instance);
        }
        
        public string GetUrl(HttpRequest request)
        {
            return _pointer(request);
        }
    }
}