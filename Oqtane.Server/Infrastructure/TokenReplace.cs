using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;
using Oqtane.Interfaces;

namespace Oqtane.Infrastructure
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

    public class TokenReplace : ITokenReplace
    {
        public const string GenericName = "generic";

        private const string TokenExpression = "(?:(?<text>\\[\\])|\\[(?:(?<source>[^{}\\]\\[:]+):(?<key>[^\\]\\[\\|]+)|(?<key>[^\\]\\[\\|]+))(?:\\|(?:(?<format>[^\\]\\[]+)\\|(?<empty>[^\\]\\\\[]+))|\\|(?:(?<format>[^\\|\\]\\[]+)))?\\])|(?<text>\\[[^\\]\\[]+\\])|(?<text>\\[{0,1}[^\\]\\[]+\\]{0,1})";

        private Regex TokenizerRegex = new Regex(TokenExpression, RegexOptions.Compiled | RegexOptions.Singleline);
        private IDictionary<string, IDictionary<string, object>> _tokens;

        private readonly ILogManager _logger;

        public TokenReplace(ILogManager logger)
        {
            _tokens = new Dictionary<string, IDictionary<string, object>>();
            _logger = logger;
        }

        public void AddSource(ITokenSource source)
        {
            this.AddSource(GenericName, source);
        }

        public void AddSource(Func<IDictionary<string, object>> sourceFunc)
        {
            this.AddSource(GenericName, sourceFunc);
        }

        public void AddSource(IDictionary<string, object> source)
        {
            this.AddSource(GenericName, source);
        }

        public void AddSource(string key, object value)
        {
            this.AddSource(GenericName, key, value);
        }

        public void AddSource(string name, ITokenSource source)
        {
            var tokens = source.GetTokens();
            this.AddSource(name, tokens);
        }

        public void AddSource(string name, Func<IDictionary<string, object>> sourceFunc)
        {
            var tokens = sourceFunc();
            this.AddSource(name, tokens);
        }

        public void AddSource(string name, IDictionary<string, object> source)
        {
            if(source != null)
            {
                foreach (var key in source.Keys)
                {
                    this.AddSource(name, key, source[key]);
                }
            }
        }

        public void AddSource(string name, string key, object value)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                name = GenericName;
            }

            var source = _tokens.ContainsKey(name.ToLower()) ? _tokens[name.ToLower()] : null;
            if(source == null)
            {
                source = new Dictionary<string, object>();
            }
            source[key] = value;

            _tokens[name.ToLower()] = source;
        }

        public string ReplaceTokens(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
            {
                return source;
            }

            var result = new StringBuilder();
            source = source.Replace("[[", "[$_["); //avoid nested square bracket issue.
            foreach (Match match in this.TokenizerRegex.Matches(source))
            {
                var key = match.Result("${key}");
                if (!string.IsNullOrWhiteSpace(key))
                {
                    var sourceName = match.Result("${source}");
                    if (string.IsNullOrWhiteSpace(sourceName) || sourceName == "[")
                    {
                        sourceName = GenericName;
                    }

                    var format = match.Result("${format}");
                    var emptyReplacment = match.Result("${empty}");
                    var value = ReplaceTokenValue(sourceName, key, format);
                    if (string.IsNullOrWhiteSpace(value))
                    {
                        if(!string.IsNullOrWhiteSpace(emptyReplacment))
                        {
                            value = emptyReplacment;
                        }
                        else //keep the original content
                        {
                            value = match.Value;
                        }
                    }

                    result.Append(value);
                }
                else
                {
                    result.Append(match.Result("${text}"));
                }
            }
            result.Replace("[$_", "["); //restore the changes.
            return result.ToString();
        }

        private string ReplaceTokenValue(string sourceName, string key, string format)
        {
            if(!_tokens.ContainsKey(sourceName.ToLower()))
            {
                _logger.Log(Shared.LogLevel.Debug, this, Enums.LogFunction.Other, $"MissingSource:{sourceName}");
                return string.Empty;
            }

            var tokens = _tokens[sourceName.ToLower()];
            if(!tokens.ContainsKey(key))
            {
                _logger.Log(Shared.LogLevel.Debug, this, Enums.LogFunction.Other, $"MissingKey:{key}");
                return string.Empty;
            }

            var value = tokens[key];
            if(value == null)
            {
                return string.Empty;
            }

            //TODO: need to implement the format.
            return value.ToString();
        }
    }
}
