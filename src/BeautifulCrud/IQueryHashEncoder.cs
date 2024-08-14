namespace BeautifulCrud;

public interface IQueryHashEncoder
{
    string Encode(ReadOnlySpan<byte> buffer);
    byte[] Decode(string queryHash);
}