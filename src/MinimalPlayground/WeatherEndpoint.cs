using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Any;

namespace MinimalPlayground;

public static class WeatherEndpoint
{
    private static readonly string[] Summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    public static void MapWeatherEndpoint(this WebApplication app)
    {
        app.MapGet("/weatherforecast", () =>
            {
                var forecast =  Enumerable.Range(1, 5).Select(index =>
                        new WeatherForecast
                        (
                            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                            Random.Shared.Next(-20, 55),
                            Summaries[Random.Shared.Next(Summaries.Length)]
                        ))
                    .ToArray();
                return forecast;
            })
            .WithName("GetWeatherForecast")
            .WithOpenApi(opt =>
            {
                opt.Summary = "Just another test endpoint summary";
                opt.Description = "The default weather endpoint description";
                return opt;
            });
        
        app.MapGet("/hello", ([Required]string name) => new Hello(name))
            .WithName("SayHello")
            .WithOpenApi(opt =>
            {
                opt.Summary = "Hello to the workd";
                opt.Description = "The welcome service endpoint";
                opt.Parameters[0].Example = new OpenApiString("Bob");
                return opt;
            });
        
        app.MapGet("/complexInput", ([Required]AbstractSetting setting) => new SomeResult { IsOk = true, Setting = setting })
            .WithName("ProcessComplexBody")
            .WithOpenApi(opt =>
            {
                opt.Summary = "Checking how complex body with inheritance is handled";
                opt.Description = "A complex input and output example";
                // opt.Parameters[0].Examples.Add("doubleSetting", new OpenApiObject(new DoubleSetting
                // {
                //     Id = new Guid("8AF6FAEE-6F52-438A-9146-B59D6F3E580F"),
                //     Number = 99,
                //     Name = "My example"
                // }));
                
                return opt;
            });

        app.Run();
    }

    record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
    {
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }

    record Hello(string Message);

    public abstract class AbstractSetting
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
    
    public abstract class DoubleSetting : AbstractSetting
    {
        public double Number { get; set; }
    }
    
    public abstract class StringSetting : AbstractSetting
    {
        public double Comment { get; set; }
    }

    public class SomeResult
    {
        public bool IsOk { get; set; }

        public AbstractSetting Setting { get; set; }
    }
}