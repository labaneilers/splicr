using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace plugin
{
    public class Backend1
    {
        public Backend1(IDictionary<string, string> config)
        {
            
        }

        public string GetUrl(HttpRequest request)
        {
            return "https://msdn.microsoft.com/en-us/library/1009fa28(v=vs.110).aspx";
        }
    }
}
