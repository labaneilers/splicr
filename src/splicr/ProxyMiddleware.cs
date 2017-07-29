using System;
using System.Net;
using System.Net.WebSockets;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Linq;
using System.Threading;

namespace Splicr 
{
    public class ProxyMiddleware
    {
        private readonly RequestDelegate _next;

        private static readonly string[] NotForwardedWebSocketHeaders = new[] { "Connection", "Host", "Upgrade", "Sec-WebSocket-Key", "Sec-WebSocket-Version" };

        public ProxyMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        private ISessionCreator GetSessionCreator()
        {
            return SessionCreator.Instance;
        }

        public Task Invoke(HttpContext context) => HandleHttpRequest(context);

        private async Task<string> InitializeSession(HttpContext httpContext)
        {
            const string SESSION_KEY = "splicrSessionId";

            string sessionId;
            if (!httpContext.Request.Cookies.TryGetValue(SESSION_KEY, out sessionId))
            {
                ISessionCreator sessionCreator = GetSessionCreator();
                sessionId = await sessionCreator.Create(httpContext);

                httpContext.Response.Cookies.Append(SESSION_KEY, sessionId);
            }

            return sessionId;
        }

        // private async Task HandleWebSocketRequest(HttpContext httpContext)
        // {
        //     string sessionId = await InitializeSession(httpContext);

        //    string backendUrl = BackendRegistry.GetUrl(httpContext.Request);

        //     using (var client = new ClientWebSocket())
        //     {
        //         foreach (var headerEntry in httpContext.Request.Headers)
        //         {
        //             if (!NotForwardedWebSocketHeaders.Contains(headerEntry.Key, StringComparer.OrdinalIgnoreCase))
        //             {
        //                 client.Options.SetRequestHeader(headerEntry.Key, headerEntry.Value);
        //             }
        //         }

        //         var wsScheme = string.Equals(_options.Scheme, "https", StringComparison.OrdinalIgnoreCase) ? "wss" : "ws";
        //         var uriString = $"{wsScheme}://{_options.Host}:{_options.Port}{context.Request.PathBase}{context.Request.Path}{context.Request.QueryString}";

        //         if (_options.WebSocketKeepAliveInterval.HasValue)
        //         {
        //             client.Options.KeepAliveInterval = _options.WebSocketKeepAliveInterval.Value;
        //         }

        //         try
        //         {
        //             await client.ConnectAsync(new Uri(uriString), context.RequestAborted);
        //         }
        //         catch (WebSocketException)
        //         {
        //             context.Response.StatusCode = 400;
        //             return;
        //         }

        //         using (var server = await context.WebSockets.AcceptWebSocketAsync(client.SubProtocol))
        //         {
        //             await Task.WhenAll(PumpWebSocket(client, server, context.RequestAborted), PumpWebSocket(server, client, context.RequestAborted));
        //         }
        //     }
        // }
        
        // private async Task PumpWebSocket(WebSocket source, WebSocket destination, CancellationToken cancellationToken)
        // {
        //     var buffer = new byte[_options.WebSocketBufferSize ?? DefaultWebSocketBufferSize];
        //     while (true)
        //     {
        //         WebSocketReceiveResult result;
        //         try
        //         {
        //             result = await source.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
        //         }
        //         catch (OperationCanceledException)
        //         {
        //             await destination.CloseOutputAsync(WebSocketCloseStatus.EndpointUnavailable, null, cancellationToken);
        //             return;
        //         }
        //         if (result.MessageType == WebSocketMessageType.Close)
        //         {
        //             await destination.CloseOutputAsync(source.CloseStatus.Value, source.CloseStatusDescription, cancellationToken);
        //             return;
        //         }

        //         await destination.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, cancellationToken);
        //     }
        // }

        public async Task HandleHttpRequest(HttpContext httpContext)
        {
            // Console.WriteLine($"Request for {httpContext.Request.Path} received ({httpContext.Request.ContentLength ?? 0} bytes)");
            
            try
            {   
                string backendUrl = BackendRegistry.GetUrl(httpContext.Request);

                if (backendUrl == null)
                {
                    // No backend found, continue through the middleware pipeline
                    await _next.Invoke(httpContext);
                    return;
                    //throw new Exception($"No backend found for path: {httpContext.Request.Path + httpContext.Request.QueryString}");
                }

                using (HttpResponseMessage response = await ProxyHttpClient.Send(
                    httpContext, 
                    backendUrl))
                {
                    httpContext.Response.StatusCode = (int)response.StatusCode;

                    httpContext.Response.Headers.Clear();

                    ProxyHttpClient.CopyResponseHeaders(response.Headers, httpContext.Response.Headers);
                    ProxyHttpClient.CopyResponseHeaders(response.Content.Headers, httpContext.Response.Headers);

                    string sessionId = await InitializeSession(httpContext);

                    // TODO: Find all statuses that shouldn't write content
                    // For a 304 response, don't write any content
                    if (response.StatusCode == HttpStatusCode.NotModified)
                    {
                        return;
                    }

                    httpContext.Response.Headers.Remove("transfer-encoding");

                    if (response.Content.Headers.ContentType.MediaType == "text/html")
                    {
                        httpContext.Response.Headers.Remove("content-encoding");
                        httpContext.Response.Headers.ContentLength = null;

                        Layout layout = LayoutRegistry.Get(response.Headers);

                        await layout.WriteHtmlHeader(httpContext, response);

                        await response.Content.CopyToAsync(httpContext.Response.Body);

                        await layout.WriteHtmlFooter(httpContext, response);
                    }
                    else
                    {
                        await response.Content.CopyToAsync(httpContext.Response.Body);
                    }
                }
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = 500;
                httpContext.Response.ContentType = "text/plain";
                await httpContext.Response.WriteAsync("Error: " + ex.Message + "\n");
                await httpContext.Response.WriteAsync("Source: " + ex.Source + "\n");
                await httpContext.Response.WriteAsync("Stack: " + ex.StackTrace + "\n");
            }
        }
    }
}