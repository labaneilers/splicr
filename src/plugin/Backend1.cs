using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace plugin
{
    public class Backend1
    {
        private PathString _startSegment;

        private string _hostname;

        public Backend1(IDictionary<string, string> config)
        {
            string startSegment;
            if (!config.TryGetValue("startsegment", out startSegment))
            {
                throw new Exception("Start segment config value not found for Backend1");
            }
             _startSegment = new PathString(startSegment);

            if (!config.TryGetValue("hostname", out _hostname))
            {
                throw new Exception("hostname config value not found for Backend1");
            }
        }

        public string GetUrl(HttpRequest request)
        {
            PathString remaining;
            if (request.Path.StartsWithSegments(_startSegment, out remaining))
            {
                return _hostname + remaining + request.QueryString;
            }

            return null;
        }
    }
}
