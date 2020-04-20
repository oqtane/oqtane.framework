using Oqtane.Shared;
using System;
using Oqtane.Models;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Oqtane.Enums;
using Oqtane.Repository;
using Oqtane.Security;
// ReSharper disable StringIndexOfIsCultureSpecific.2
// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Oqtane.Infrastructure
{
    public class LogManager : ILogManager
    {
        private readonly ILogRepository _logs;
        private readonly ITenantResolver _tenantResolver;
        private readonly IConfigurationRoot _config;
        private readonly IUserPermissions _userPermissions;
        private readonly IHttpContextAccessor _accessor;

        public LogManager(ILogRepository logs, ITenantResolver tenantResolver, IConfigurationRoot config, IUserPermissions userPermissions, IHttpContextAccessor accessor)
        {
            _logs = logs;
            _tenantResolver = tenantResolver;
            _config = config;
            _userPermissions = userPermissions;
            _accessor = accessor;
        }

        public void Log(LogLevel level, object @class, LogFunction function, string message, params object[] args)
        {
            Log(-1, level, @class.GetType().AssemblyQualifiedName, function, null, message, args);
        }

        public void Log(LogLevel level, object @class, LogFunction function, Exception exception, string message, params object[] args)
        {
            Log(-1, level, @class.GetType().AssemblyQualifiedName, function, exception, message, args);
        }

        public void Log(int siteId, LogLevel level, object @class, LogFunction function, string message, params object[] args)
        {
            Log(siteId, level, @class.GetType().AssemblyQualifiedName, function, null, message, args);
        }

        public void Log(int siteId, LogLevel level, object @class, LogFunction function, Exception exception, string message, params object[] args)
        {
            Log log = new Log();
            if (siteId == -1)
            {
                log.SiteId = null;
                Alias alias = _tenantResolver.GetAlias();
                if (alias != null)
                {
                    log.SiteId = alias.SiteId;
                }
            }
            else
            {
                log.SiteId = siteId;
            }
            log.PageId = null;
            log.ModuleId = null;
            log.UserId = null;
            User user = _userPermissions.GetUser();
            if (user != null)
            {
                log.UserId = user.UserId;
            }
            HttpRequest request = _accessor.HttpContext.Request;
            if (request != null)
            {
                log.Url = $"{request.Scheme}://{request.Host}{request.Path}{request.QueryString}";
            }

            Type type = Type.GetType(@class.ToString());
            if (type != null)
            {
                log.Category = type.AssemblyQualifiedName;
                log.Feature = Utilities.GetTypeNameLastSegment(log.Category, 0);
            }
            else
            {
                log.Category = @class.ToString();
                log.Feature = log.Category;
            }
            log.Function = Enum.GetName(typeof(LogFunction), function);
            log.Level = Enum.GetName(typeof(LogLevel), level);
            if (exception != null)
            {
                log.Exception = exception.ToString();
            }
            log.Message = message;
            log.MessageTemplate = "";
            try
            {
                log.Properties = JsonSerializer.Serialize(args);
            }
            catch // serialization error occurred
            {
                log.Properties = "";
            }
            Log(log);
        }

        public void Log(Log log)
        {
            LogLevel minlevel = LogLevel.Information;
            var section = _config.GetSection("Logging:LogLevel:Default");
            if (section.Exists())
            {
                minlevel = Enum.Parse<LogLevel>(section.Value);
            }

            if (Enum.Parse<LogLevel>(log.Level) >= minlevel)
            {
                log.LogDate = DateTime.UtcNow;
                log.Server = Environment.MachineName;
                log.MessageTemplate = log.Message;
                log = ProcessStructuredLog(log);
                try
                {
                    _logs.AddLog(log);
                }
                catch
                {
                    // an error occurred writing to the database
                }
            }
        }

        private Log ProcessStructuredLog(Log log)
        {
            try
            {
                string message = log.Message;
                string properties = "";
                if (!string.IsNullOrEmpty(message) && message.Contains("{") && message.Contains("}") && !string.IsNullOrEmpty(log.Properties))
                {
                    // get the named holes in the message and replace values
                    object[] values = JsonSerializer.Deserialize<object[]>(log.Properties);
                    List<string> names = new List<string>();
                    int index = message.IndexOf("{");
                    while (index != -1)
                    {
                        if (message.IndexOf("}", index) != -1)
                        {
                            names.Add(message.Substring(index + 1, message.IndexOf("}", index) - index - 1));
                            if (values.Length > (names.Count - 1))
                            {
                                if (values[names.Count - 1] == null)
                                {
                                    message = message.Replace("{" + names[names.Count - 1] + "}", "null");
                                }
                                else
                                {
                                    message = message.Replace("{" + names[names.Count - 1] + "}", values[names.Count - 1].ToString());
                                }
                            }
                        }
                        index = message.IndexOf("{", index + 1);
                    }
                    // rebuild properties into dictionary
                    Dictionary<string, object> propertyDictionary = new Dictionary<string, object>();
                    for (int i = 0; i < values.Length; i++)
                    {
                        if (i < names.Count)
                        {
                            propertyDictionary.Add(names[i], values[i]);
                        }
                        else
                        {
                            propertyDictionary.Add("Property" + i.ToString(), values[i]);
                        }
                    }
                    properties = JsonSerializer.Serialize(propertyDictionary);
                }
                log.Message = message;
                log.Properties = properties;
            }
            catch
            {
                log.Properties = "";
            }
            return log;
        }
    }
}
