using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Any;
using Swashbuckle.AspNetCore.Filters;

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
        
        app.MapPost("/complexInput", ([Required][FromBody]AbstractSetting setting) 
                => new SomeResult { IsOk = true, Setting = setting })
            .WithName("ProcessComplexBody")
            .WithOpenApi(opt =>
            {
                opt.Summary = "Checking how complex body with inheritance is handled";
                opt.Description = "A complex input and output example";
                
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
        [JsonPropertyName("$type")]
        [JsonPropertyOrder(-1)]
        public string Discriminator { 
            get => GetType().Name;
            init { }
        }
        
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
    
    public class DoubleSetting : AbstractSetting
    {
        public double Number { get; set; }
    }
    
    public class CommentSetting : AbstractSetting
    {
        public string Comment { get; set; }
    }

    public class SomeResult
    {
        public bool IsOk { get; set; }

        public AbstractSetting Setting { get; set; }
    }

    public class AbstractSettingExampleProvider : IMultipleExamplesProvider<AbstractSetting>
    {
        public IEnumerable<SwaggerExample<AbstractSetting>> GetExamples()
        {
            yield return SwaggerExample.Create<AbstractSetting>(
                name: "Double setting", 
                value: new DoubleSetting
                {
                    Id   = new Guid("96C74E6F-9F90-4D04-A61E-173CA9849792"),
                    Name = "A double setting example",
                    Number = 123.456
                }
            );
            
            yield return SwaggerExample.Create<AbstractSetting>(
                name: "Comment setting", 
                value: new CommentSetting() 
                {
                    Id   = new Guid("1B1134C0-D329-40E3-8F82-B3DDFFB819CF"),
                    Name = "A string setting example",
                    Comment = "Weather is nice today",
                }
            ); 
        }
    }
    
    public class PolymorphicTypeResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            JsonTypeInfo jsonTypeInfo = base.GetTypeInfo(type, options);

            Type baseSettingType = typeof(AbstractSetting);
            if (jsonTypeInfo.Type == baseSettingType)
            {
                jsonTypeInfo.PolymorphismOptions = new JsonPolymorphismOptions
                {
                    TypeDiscriminatorPropertyName = "$type",
                    IgnoreUnrecognizedTypeDiscriminators = true,
                    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FailSerialization,
                    DerivedTypes =
                    {
                        new JsonDerivedType(typeof(DoubleSetting), typeof(DoubleSetting).Name),
                        new JsonDerivedType(typeof(CommentSetting), typeof(CommentSetting).Name)
                    }
                };
            }

            return jsonTypeInfo;
        }
    }
}