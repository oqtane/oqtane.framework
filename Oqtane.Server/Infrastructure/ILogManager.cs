using Oqtane.Models;
using Oqtane.Shared;
using System;

namespace Oqtane.Infrastructure
{
    public interface ILogManager
    {
        void AddLog(string Category, LogLevel Level, string Message, params object[] Args);
        void AddLog(string Category, LogLevel Level, Exception Exception, string Message, params object[] Args);
        void AddLog(Log Log);
    }
}
