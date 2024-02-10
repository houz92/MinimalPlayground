using FluentAssertions;

namespace MinimalPlayground.Client.Test;

public class WeatherClientTest
{
    [Fact]
    public async Task Client_can_retrieve_weather_from_running_instance()
    {
        // Arrange
        using var http = new HttpClient();
        var client = new WeatherClient("https://localhost:7124/", http);
        
        // Act
        var forecasts = await client.GetWeatherForecastAsync();

        // Assert
        forecasts.Should().NotBeNull();
    }
}