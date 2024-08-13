using BeautifulCrud;

namespace Demo.Shared;

// ReSharper disable once NonReadonlyMemberInGetHashCode

public sealed class WeatherForecast : IKeyed<Guid>, IEquatable<WeatherForecast>
{
    public Guid Id { get; set; }
    public DateOnly Date { get; set; }
    public int TemperatureC { get; set; }
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    public string? Summary { get; set; }

    public bool Equals(WeatherForecast? other) => !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || Id.Equals(other.Id));
    public override bool Equals(object? obj) => ReferenceEquals(this, obj) || obj is WeatherForecast other && Equals(other);
    public override int GetHashCode() => Id.GetHashCode();
    public static bool operator ==(WeatherForecast? left, WeatherForecast? right) => Equals(left, right);
    public static bool operator !=(WeatherForecast? left, WeatherForecast? right) => !Equals(left, right);
}