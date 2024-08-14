using BeautifulCrud.Tests.Models;
using Xunit.Abstractions;

#pragma warning disable xUnit1045

namespace BeautifulCrud.Tests;

public class QueryHashEncoderTests(ITestOutputHelper console)
{
    [Theory, ClassData(typeof(QueryHashEncoders))]
    public void RoundTripTest(IQueryHashEncoder encoder)
    {
        var buffer = "This is a test."u8.ToArray();
        var encoded = encoder.Encode(buffer);
        var decoded = encoder.Decode(encoded);
        Assert.True(buffer.SequenceEqual(decoded));

        console.WriteLine($"Length: {encoded.Length}");
    }
}
