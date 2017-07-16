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
    public class SyncSessionCreator : ISessionCreator
    {
        public async Task<string> Create(HttpContext httpContext)
        {
            //  TODO: Use a cryptographic key
            string sessionId = Guid.NewGuid().ToString();

            // Notify a service that a new session is detected
            HttpResponseMessage response = await ProxyHttpClient.Send(
                httpContext, 
                "http://www.labaneilers.com");

            // TODO: do something with response?
            // Handle some errors, perhaps?
            // Set some cookies based on response?

            return sessionId;
        }
    }
}
