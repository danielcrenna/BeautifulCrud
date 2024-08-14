using Microsoft.AspNetCore.WebUtilities;

namespace BeautifulCrud;

public sealed class Base64QueryHashEncoder : IQueryHashEncoder
{
    public string Encode(ReadOnlySpan<byte> buffer) => EncodeInternal(buffer);
    internal static string EncodeInternal(ReadOnlySpan<byte> buffer) => WebEncoders.Base64UrlEncode(buffer);

    public byte[] Decode(string queryHash) => DecodeInternal(queryHash);
    internal static byte[] DecodeInternal(string queryHash) => WebEncoders.Base64UrlDecode(queryHash);
}