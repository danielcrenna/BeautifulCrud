using BeautifulCrud.Tests.Extensions;
using BeautifulCrud.Tests.Models;

#pragma warning disable xUnit1045

namespace BeautifulCrud.Tests;

public class BinaryResourceQuerySerializerTests
{
    [Theory, ClassData(typeof(ResourceQuerySerializers))]
    public void RoundTripTest(IResourceQuerySerializer serializer)
    {
        var expected = ResourceQueryFactory.BuildResourceQuery(DateTimeOffset.Now);
        
        byte[] buffer;

        {
            using var ms = new MemoryStream();
            using var bw = new BinaryWriter(ms);

            serializer.Serialize(expected, bw);

            buffer = ms.ToArray();
            
            using var br = new BinaryReader(new MemoryStream(buffer));
            var actual = serializer.Deserialize(br);
            expected.AssertValidResourceQuery(actual);
        }

        {
            using var compare = new MemoryCompareStream(buffer);
            using var bw = new BinaryWriter(compare);
            serializer.Serialize(expected, bw);
        }
    }

    
}