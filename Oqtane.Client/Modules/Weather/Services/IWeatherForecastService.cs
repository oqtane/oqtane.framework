using System;
using System.Threading.Tasks;

namespace Oqtane.Modules.Weather.Services
{
    public interface IWeatherForecastService
    {
        Task<WeatherForecast[]> GetForecastAsync(DateTime startDate);
    }
}
