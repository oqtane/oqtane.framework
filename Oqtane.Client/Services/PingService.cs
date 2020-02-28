using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Oqtane.Shared;

namespace Oqtane.Services
{   
    public class PingService : ServiceBase, IPingService
    {
        private readonly HttpClient _http;

        public PingService(HttpClient Http)
        {
            _http = Http;
        }


        public async Task<string> Ping(string url)
        {
            try
            {
                Console.WriteLine($"PING {url}");
                return await _http.GetStringAsync(url);
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
    }

    public interface IPingService
    {
        Task<string> Ping(string url);
    }
}