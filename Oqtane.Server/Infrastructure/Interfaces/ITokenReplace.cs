using System;
using System.Collections.Generic;
using Oqtane.Interfaces;

namespace Oqtane.Infrastructure.Interfaces
{
    public interface ITokenReplace
    {
        void AddSource(ITokenSource source);

        void AddSource(Func<IDictionary<string, object>> sourceFunc);

        void AddSource(IDictionary<string, object> source);

        void AddSource(string key, object value);

        void AddSource(string name, ITokenSource source);

        void AddSource(string name, Func<IDictionary<string, object>> sourceFunc);

        void AddSource(string name, IDictionary<string, object> source);

        void AddSource(string name, string key, object value);

        string ReplaceTokens(string source);
    }
}
