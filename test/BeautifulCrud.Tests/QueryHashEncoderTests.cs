using BeautifulCrud.Tests.Models;
using Xunit.Abstractions;

#pragma warning disable xUnit1045

namespace BeautifulCrud.Tests;

public class QueryHashEncoderTests(ITestOutputHelper console)
{
    [Theory, ClassData(typeof(QueryHashEncoders))]
    public void RoundTripTest(IQueryHashEncoder encoder)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        var query = ResourceQueryFactory.BuildResourceQuery(DateTimeOffset.Now);
        BinaryResourceQuerySerializer.SerializeInternal(query, bw);

        var buffer = ms.ToArray();
        var encoded = encoder.Encode(buffer);
        var decoded = encoder.Decode(encoded);
        Assert.True(buffer.SequenceEqual(decoded));

        console.WriteLine($"Length: {encoded.Length}");
    }
}
