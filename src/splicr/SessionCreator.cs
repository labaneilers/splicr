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
    public class SessionCreator : ISessionCreator
    {
        public static ISessionCreator Instance { get; set; }

        private string _url;
        private bool _isAsync;

        public SessionCreator(string url, bool isAsync)
        {
            _url = url;
            _isAsync = isAsync;
        }

        public async Task<string> Create(HttpContext httpContext)
        {
            //  TODO: Use a cryptographic key
            string sessionId = Guid.NewGuid().ToString("N");

            // Notify a service that a new session is detected
            var task = ProxyHttpClient.Send(
                httpContext, 
                _url);

            if (_isAsync)
            {
                // Fire and forget
                task.Start();

                return sessionId;
            }
            else 
            {
                HttpResponseMessage response = await task.ContinueWith(t => t.Result);

                // TODO: do something with response?
                // Handle some errors, perhaps?
                // Set some cookies based on response?

                return sessionId;
            }
        }
    }
}
