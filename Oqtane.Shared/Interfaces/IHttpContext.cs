using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Http;

namespace Oqtane.Interfaces
{
    public interface IHttpContext
    {
        public HttpRequest Request { get; }

        public HttpResponse Response { get; }

        public ConnectionInfo Connection { get; }

        public WebSocketManager WebSockets { get; }

        public ClaimsPrincipal User { get; }

        public IDictionary<object, object> Items { get; }

        public string TraceIdentifier { get; }

        public ISession Session { get; }
    }
}
