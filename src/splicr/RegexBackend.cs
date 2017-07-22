using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Splicr
{
    public class RegexBackend : IBackend
    {
        private Regex _matcher;

        private string _replaceString;

        private string _hostname;

        private static object _lock = new Object();

        public RegexBackend(string hostname, string matchString, string replaceString)
        {
            _hostname = hostname;
            _matcher = new Regex(matchString, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            _replaceString = replaceString;
        }

        public string GetUrl(HttpRequest request)
        {
            string pathAndQuery = request.Path + request.QueryString;

            if (_matcher.IsMatch(pathAndQuery)) 
            {
                string backendPathAndQuery = _matcher.Replace(pathAndQuery, _replaceString);

                return _hostname + backendPathAndQuery;
            }
            
            return null;
        }
    }
}
