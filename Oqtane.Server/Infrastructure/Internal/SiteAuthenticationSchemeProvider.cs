using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Oqtane.Infrastructure
{
    internal class SiteAuthenticationSchemeProvider : IAuthenticationSchemeProvider
    {
        public SiteAuthenticationSchemeProvider(IOptions<AuthenticationOptions> options)
            : this(options, new Dictionary<string, AuthenticationScheme>(StringComparer.Ordinal))
        {
        }

        public SiteAuthenticationSchemeProvider(IOptions<AuthenticationOptions> options, IDictionary<string, AuthenticationScheme> schemes)
        {
            _optionsProvider = options;

            _schemes = schemes ?? throw new ArgumentNullException(nameof(schemes));
            _requestHandlers = new List<AuthenticationScheme>();

            foreach (var builder in _optionsProvider.Value.Schemes)
            {
                var scheme = builder.Build();
                AddScheme(scheme);
            }
        }

        private readonly IOptions<AuthenticationOptions> _optionsProvider;
        private readonly object _lock = new object();

        private readonly IDictionary<string, AuthenticationScheme> _schemes;
        private readonly List<AuthenticationScheme> _requestHandlers;

        private Task<AuthenticationScheme> GetDefaultSchemeAsync()
            => _optionsProvider.Value.DefaultScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultScheme)
            : Task.FromResult<AuthenticationScheme>(null);

        public virtual Task<AuthenticationScheme> GetDefaultAuthenticateSchemeAsync()
            => _optionsProvider.Value.DefaultAuthenticateScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultAuthenticateScheme)
            : GetDefaultSchemeAsync();

        public virtual Task<AuthenticationScheme> GetDefaultChallengeSchemeAsync()
            => _optionsProvider.Value.DefaultChallengeScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultChallengeScheme)
            : GetDefaultSchemeAsync();

        public virtual Task<AuthenticationScheme> GetDefaultForbidSchemeAsync()
            => _optionsProvider.Value.DefaultForbidScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultForbidScheme)
            : GetDefaultChallengeSchemeAsync();

        public virtual Task<AuthenticationScheme> GetDefaultSignInSchemeAsync()
            => _optionsProvider.Value.DefaultSignInScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultSignInScheme)
            : GetDefaultSchemeAsync();

        public virtual Task<AuthenticationScheme> GetDefaultSignOutSchemeAsync()
            => _optionsProvider.Value.DefaultSignOutScheme != null
            ? GetSchemeAsync(_optionsProvider.Value.DefaultSignOutScheme)
            : GetDefaultSignInSchemeAsync();

        public virtual Task<AuthenticationScheme> GetSchemeAsync(string name)
            => Task.FromResult(_schemes.ContainsKey(name) ? _schemes[name] : null);

        public virtual Task<IEnumerable<AuthenticationScheme>> GetRequestHandlerSchemesAsync()
            => Task.FromResult<IEnumerable<AuthenticationScheme>>(_requestHandlers);

        public virtual void AddScheme(AuthenticationScheme scheme)
        {
            if (_schemes.ContainsKey(scheme.Name))
            {
                throw new InvalidOperationException("Scheme already exists: " + scheme.Name);
            }
            lock (_lock)
            {
                if (_schemes.ContainsKey(scheme.Name))
                {
                    throw new InvalidOperationException("Scheme already exists: " + scheme.Name);
                }
                if (typeof(IAuthenticationRequestHandler).IsAssignableFrom(scheme.HandlerType))
                {
                    _requestHandlers.Add(scheme);
                }
                _schemes[scheme.Name] = scheme;
            }
        }

        public virtual void RemoveScheme(string name)
        {
            if (!_schemes.ContainsKey(name))
            {
                return;
            }
            lock (_lock)
            {
                if (_schemes.ContainsKey(name))
                {
                    var scheme = _schemes[name];
                    _requestHandlers.Remove(scheme);
                    _schemes.Remove(name);
                }
            }
        }

        public virtual Task<IEnumerable<AuthenticationScheme>> GetAllSchemesAsync()
            => Task.FromResult<IEnumerable<AuthenticationScheme>>(_schemes.Values);
    }
}
