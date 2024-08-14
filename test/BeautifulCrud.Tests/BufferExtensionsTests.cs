using BeautifulCrud.Extensions;

namespace BeautifulCrud.Tests;

public class BufferExtensionsTests
{
    [Fact]
    public void NullableStrings()
    {
        const string expected = "BeautifulCrud.Tests.OpaqueQueryStoreTests+WeatherForecast, BeautifulCrud.Tests, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null";
        
        var ws = new MemoryStream();
        var bw = new BinaryWriter(ws);
        bw.WriteNullableString(expected);

        var buffer = ws.ToArray();

        var rs = new MemoryStream(buffer);
        var br = new BinaryReader(rs);
        
        var actual = br.ReadNullableString();
        Assert.Equal(expected, actual);
    }

}