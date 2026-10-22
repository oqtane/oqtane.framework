using System.Collections.Generic;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Oqtane.Interfaces;

namespace Oqtane.Providers
{
    public class ServerHttpContext : IHttpContext
    {
        private readonly HttpContext _context;

        public ServerHttpContext(IHttpContextAccessor httpContextAccessor)
        {
            _context = httpContextAccessor.HttpContext;
        }

        public HttpRequest Request
        {
            get
            {
                return (_context != null) ? _context.Request : null;
            }
        }

        public HttpResponse Response
        {
            get
            {
                return (_context != null) ? _context.Response : null;
            }
        }

        public ConnectionInfo Connection
        {
            get
            {
                return (_context != null) ? _context.Connection : null;
            }
        }

        public WebSocketManager WebSockets
        {
            get
            {
                return (_context != null) ? _context.WebSockets : null;
            }
        }

        public ClaimsPrincipal User
        {
            get
            {
                return (_context != null) ? _context.User : null;
            }
        }

        public IDictionary<object, object> Items
        {
            get
            {
                return (_context != null) ? _context.Items : null;
            }
        }

        public string TraceIdentifier
        {
            get
            {
                return (_context != null) ? _context.TraceIdentifier : null;
            }
        }

        public ISession Session
        {
            get
            {
                return (_context != null) ? _context.Session : null;
            }
        }
    }
}
