using Oqtane.Shared;
using System;
using Oqtane.Models;
using System.Text.Json;
using Oqtane.Repository;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Collections.Generic;

namespace Oqtane.Infrastructure
{
    public class LogManager : ILogManager
    {
        private readonly ILogRepository Logs;
        private readonly ITenantResolver TenantResolver;
        private readonly IConfigurationRoot Config;
        private readonly IHttpContextAccessor Accessor;

        public LogManager(ILogRepository Logs, ITenantResolver TenantResolver, IConfigurationRoot Config, IHttpContextAccessor Accessor)
        {
            this.Logs = Logs;
            this.TenantResolver = TenantResolver;
            this.Config = Config;
            this.Accessor = Accessor;
        }

        public void Log(LogLevel Level, object Class, LogFunction Function, string Message, params object[] Args)
        {
            Log(Level, Class.GetType().AssemblyQualifiedName, Function, null, Message, Args);
        }

        public void Log(LogLevel Level, object Class, LogFunction Function, Exception Exception, string Message, params object[] Args)
        {
            Alias alias = TenantResolver.GetAlias();
            Log log = new Log();
            log.SiteId = alias.SiteId;
            log.PageId = null;
            log.ModuleId = null;
            if (Accessor.HttpContext.User.FindFirst(ClaimTypes.PrimarySid) != null)
            {
                log.UserId = int.Parse(Accessor.HttpContext.User.FindFirst(ClaimTypes.PrimarySid).Value);
            }
            HttpRequest request = Accessor.HttpContext.Request;
            if (request != null)
            {
                log.Url = request.Scheme.ToString() + "://" + request.Host.ToString() + request.Path.ToString() + request.QueryString.ToString();
            }

            Type type = Type.GetType(Class.ToString());
            if (type != null)
            {
                log.Category = type.AssemblyQualifiedName;
                log.Feature = Utilities.GetTypeNameLastSegment(log.Category, 0);
            }
            else
            {
                log.Category = Class.ToString();
                log.Feature = log.Category;
            }
            log.Function = Enum.GetName(typeof(LogFunction), Function);
            log.Level = Enum.GetName(typeof(LogLevel), Level);
            if (Exception != null)
            {
                log.Exception = Exception.ToString();
            }
            log.Message = Message;
            log.MessageTemplate = "";
            try
            {
                log.Properties = JsonSerializer.Serialize(Args);
            }
            catch // serialization error occurred
            {
                log.Properties = "";
            }
            Log(log);
        }

        public void Log(Log Log)
        {
            LogLevel minlevel = LogLevel.Information;
            var section = Config.GetSection("Logging:LogLevel:Default");
            if (section.Exists())
            {
                minlevel = Enum.Parse<LogLevel>(Config.GetSection("Logging:LogLevel:Default").ToString());
            }

            if (Enum.Parse<LogLevel>(Log.Level) >= minlevel)
            {
                Log.LogDate = DateTime.UtcNow;
                Log.Server = Environment.MachineName;
                Log.MessageTemplate = Log.Message;
                Log = ProcessStructuredLog(Log);
                Logs.AddLog(Log);
            }
        }

        private Log ProcessStructuredLog(Log Log)
        {
            string message = Log.Message;
            string properties = "";
            if (!string.IsNullOrEmpty(message) && message.Contains("{") && message.Contains("}") && !string.IsNullOrEmpty(Log.Properties))
            {
                // get the named holes in the message and replace values
                object[] values = JsonSerializer.Deserialize<object[]>(Log.Properties);
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
                Dictionary<string, object> propertydictionary = new Dictionary<string, object>();
                for (int i = 0; i < values.Length; i++)
                {
                    if (i < names.Count)
                    {
                        propertydictionary.Add(names[i], values[i]);
                    }
                    else
                    {
                        propertydictionary.Add("Property" + i.ToString(), values[i]);
                    }
                }
                properties = JsonSerializer.Serialize(propertydictionary);
            }
            Log.Message = message;
            Log.Properties = properties;
            return Log;
        }
    }
}