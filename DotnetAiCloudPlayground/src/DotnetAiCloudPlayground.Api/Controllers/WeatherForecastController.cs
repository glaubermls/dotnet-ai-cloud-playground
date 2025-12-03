using Microsoft.AspNetCore.Mvc;

namespace DotnetAiCloudPlayground.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Produces("application/json")]
    [Tags("Weather")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries =
        [
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        ];

        /// <summary>
        /// Gets the weather forecast for the next 5 days
        /// </summary>
        /// <returns>A list of weather forecasts</returns>
        /// <response code="200">Returns the weather forecast list</response>
        [HttpGet(Name = "GetWeatherForecast")]
        [ProducesResponseType(typeof(IEnumerable<WeatherForecast>), StatusCodes.Status200OK)]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }
    }
}
