using System;
using System.Threading.Tasks;
using Oqtane.Modules.Weather.Models;

namespace Oqtane.Modules.Weather.Services
{
    public interface IWeatherForecastService
    {
        Task<WeatherForecast[]> GetForecastAsync(DateTime startDate);
    }
}
