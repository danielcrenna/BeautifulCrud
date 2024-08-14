using System.Collections.Concurrent;
using Sqids;
using WyHash;

namespace BeautifulCrud;

/// <summary>
/// Produces small continuation tokens, so that they are easier to reason about.
/// This comes at the cost of requiring a lookup on the server to fetch the underlying query data.
/// </summary>
/// <param name="timestamps"></param>
public sealed class InMemoryContinuationTokenGenerator(Func<DateTimeOffset> timestamps, IResourceQuerySerializer serializer) : IContinuationTokenGenerator
{
    private static readonly SqidsEncoder<ulong> Encoder = new (new SqidsOptions());
    private static readonly ConcurrentDictionary<ulong, (Type context, DateTimeOffset timestamp, byte[] buffer)> Cache = [];

    public string Build(Type context, ResourceQuery query)
    {
        var (continuationToken, buffer) = BuildContinuationToken(context, query, out var cacheKey);
        Cache.TryAdd(cacheKey, (context, timestamps(), buffer));
        return continuationToken;
    }

    public ResourceQuery? Parse(Type context, string? continuationToken)
    {
        var decoded = Encoder.Decode(continuationToken);
        if (decoded.Count == 0 || !Cache.TryGetValue(decoded[0], out var value))
            return null;

        using var ms = new MemoryStream(value.buffer);
        using var br = new BinaryReader(ms);
        var query = serializer.Deserialize(br);

        query.AsOfDateTime = query.IsDeltaQuery ? value.timestamp : null;
        return query;
    }

    private (string continuationToken, byte[] buffer) BuildContinuationToken(Type context, ResourceQuery query, out ulong key)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        serializer.Serialize(query, bw);

        var buffer = ms.ToArray();
        key = WyHash64.ComputeHash64(buffer, 1337);
        
        return (Encoder.Encode(key), buffer);
    }
}