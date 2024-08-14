using BeautifulCrud.Tests.Extensions;
using BeautifulCrud.Tests.Models;
using Xunit.Abstractions;

#pragma warning disable xUnit1045

namespace BeautifulCrud.Tests;

public class ContinuationTokenGeneratorTests(ITestOutputHelper console)
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Now;

    public static TheoryData<IContinuationTokenGenerator> ContinuationTokenGeneratorData => new ContinuationTokenGenerators(() => Now);

    [Theory, MemberData(nameof(ContinuationTokenGeneratorData))]
    public void RoundTripTest(IContinuationTokenGenerator continuationTokenGenerator)
    {
        var type = typeof(WeatherForecast);
        var expected = ResourceQueryFactory.BuildResourceQuery(Now);

        var firstHash = continuationTokenGenerator.Build(type, expected);
        Assert.NotNull(firstHash);
        Assert.NotEmpty(firstHash);

        var actual = continuationTokenGenerator.Parse(type, firstHash);
        Assert.NotNull(actual);
        expected.AssertValidResourceQuery(actual);

        var secondHash = continuationTokenGenerator.Build(type, actual);
        Assert.NotNull(secondHash);
        Assert.NotEmpty(secondHash);

        Assert.Equal(firstHash, secondHash);

        console.WriteLine($"Length: {firstHash.Length}");
    }
}