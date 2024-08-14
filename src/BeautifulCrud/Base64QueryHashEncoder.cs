using Microsoft.AspNetCore.WebUtilities;

namespace BeautifulCrud;

public sealed class Base64QueryHashEncoder : IQueryHashEncoder
{
    public string Encode(ReadOnlySpan<byte> buffer)
    {
        var encoded = WebEncoders.Base64UrlEncode(buffer);
        return encoded;
    }

    public byte[] Decode(string queryHash)
    {
        var decoded = WebEncoders.Base64UrlDecode(queryHash);
        return decoded;
    }
}