using System.Runtime.CompilerServices;
using BeautifulCrud.Extensions;

namespace BeautifulCrud;

/// <summary>
/// Produces continuation tokens that are parseable, so no server lookups are required.
/// This comes at the cost of token length, and a deserialization step.
/// </summary>
public sealed class PortableContinuationTokenGenerator(Func<DateTimeOffset> timestamps, IQueryHashEncoder encoder, IResourceQuerySerializer serializer) : IContinuationTokenGenerator
{
    public string Build(Type context, ResourceQuery query)
    {
        ArgumentNullException.ThrowIfNull(context);

        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);

        bw.WriteNullableString(context.FullName);
        bw.WriteNullableDateTimeOffset(timestamps());
        serializer.Serialize(query, bw);

        var buffer = ms.ToArray();
        return encoder.Encode(buffer);
    }
    
    public ResourceQuery? Parse(Type context, string? continuationToken)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (string.IsNullOrWhiteSpace(continuationToken))
            return null;

        var buffer = encoder.Decode(continuationToken);

        using var ms = new MemoryStream(buffer);
        using var br = new BinaryReader(ms);

        var typeFullName = br.ReadNullableString();
        if (string.IsNullOrWhiteSpace(typeFullName) || typeFullName != context.FullName)
            return null;
        
        var asOfDateTime = br.ReadNullableDateTimeOffset();
        var query = serializer.Deserialize(br);
        query.AsOfDateTime = query.IsDeltaQuery ? asOfDateTime : null;

        return query;
    }
}