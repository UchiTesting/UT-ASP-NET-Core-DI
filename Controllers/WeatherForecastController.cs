using Microsoft.AspNetCore.Mvc;

namespace ASP_DI.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    private readonly ILogger<WeatherForecastController> _logger;
    private readonly DependencyService1 _service1;
    private readonly DependencyService2 _service2;

    public WeatherForecastController(ILogger<WeatherForecastController> logger,
        DependencyService1 service1, DependencyService2 service2)
    {
        _logger = logger;
        _service1 = service1;
        _service2 = service2;
    }

    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<object> Get()
    {
        _service1.Write();
        _service2.Write();

        return Enumerable.Range(1, 5).Select(index => new
        {
            Id = index,
            forecast = new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            }
        })
        .ToArray();
    }
}
