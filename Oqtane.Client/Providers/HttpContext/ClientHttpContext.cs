using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Microsoft.AspNetCore.Http;
using Oqtane.Interfaces;

namespace Oqtane.Providers
{
    public class ClientHttpContext : IHttpContext
    {
        public HttpRequest Request { get; } = null;

        public HttpResponse Response { get; } = null;

        public ConnectionInfo Connection { get; } = null;

        public WebSocketManager WebSockets { get; } = null;

        public ClaimsPrincipal User { get; set; } = null;

        public IDictionary<object, object> Items { get; } = null;

        public string TraceIdentifier { get; } = null;

        public ISession Session { get; } = null;
    }
}
