using System.Reflection;
using Microsoft.OpenApi.Models;
using MinimalPlayground;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = builder.Environment.ApplicationName, Version = "v1" });
    options.UseAllOfForInheritance();
    options.UseOneOfForPolymorphism();
    options.ExampleFilters();
    
    options.MapType<TimeSpan>(() => new OpenApiSchema { Type = "string", Format = "0.00:00:00", Reference = null, Nullable = false });
    options.MapType<TimeSpan?>(() => new OpenApiSchema { Type = "string", Format = "0.00:00:00", Reference = null, Nullable = true });
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, new WeatherEndpoint.PolymorphicTypeResolver());
});

builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.EnableTryItOutByDefault();
    });
}

app.UseHttpsRedirection();
app.MapWeatherEndpoint();
