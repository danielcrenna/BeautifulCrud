using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using BeautifulCrud.Extensions;

namespace BeautifulCrud;

public sealed class OpaqueQueryStore(Func<DateTimeOffset> timestamps) : IQueryStore
{
    public string BuildQueryHash(Type type, ResourceQuery query)
    {
        ArgumentNullException.ThrowIfNull(type);

        var now = timestamps();

        var sb = new StringBuilder();
        {
            sb.Append(type.FullName);
            sb.Append('_');
            sb.Append(now.ToUnixTimeSeconds());
            sb.Append('_');
        };

        var key = sb.ToString();
        var data = SerializeResourceQuery(query);
        var value = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(key).Concat(data));
        return value;
    }

    public ResourceQuery? GetQueryFromHash(Type context, string? queryHash)
    {
        if (string.IsNullOrWhiteSpace(queryHash))
            return null;

        var continuationToken = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(queryHash));
        var segments = continuationToken.Split("_");

        var contextSegment = segments.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(contextSegment) || contextSegment != context.FullName)
            return null;
        
        var tokens = continuationToken.Split('_', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length != 3)
            return null;

        var serialized = tokens[2];
        var deserialized = DeserializeResourceQuery(Encoding.UTF8.GetBytes(serialized));

        var asOfSegment = segments.Skip(1).Take(1).SingleOrDefault();

        if (deserialized.IsDeltaQuery && asOfSegment != null)
        {
            if(long.TryParse(asOfSegment, out var asOf))
                deserialized.AsOfDateTime = DateTimeOffset.FromUnixTimeSeconds(asOf);
        }

        return deserialized;
    }

    private static byte[] SerializeResourceQuery(ResourceQuery query)
    {
        var ms = new MemoryStream();
        var bw = new BinaryWriter(ms);

        WriteFilter(bw, query);
        WriteSorting(bw, query);
        WritePaging(bw, query);
        WriteProjections(bw, query);
        WriteSearch(bw, query);

        bw.Write(query.CountTotalRows);
        bw.Write(query.IsDeltaQuery);
        bw.WriteNullableDateTimeOffset(query.AsOfDateTime);
        bw.WriteNullableString(query.ServerUri?.ToString());

        var data = ms.ToArray();
        return data;
    }

    private static ResourceQuery DeserializeResourceQuery(byte[] buffer)
    {
        var ms = new MemoryStream(buffer);
        var br = new BinaryReader(ms);

        var query = new ResourceQuery();

        ReadFilter(br, query);
        ReadSorting(br, query);
        ReadPaging(br, query);
        ReadProjections(br, query);
        ReadSearch(br, query);

        query.CountTotalRows = br.ReadBoolean();
        query.IsDeltaQuery = br.ReadBoolean();
        query.AsOfDateTime = br.ReadNullableDateTimeOffset();

        var serverUri = br.ReadNullableString();
        if (!string.IsNullOrWhiteSpace(serverUri))
            query.ServerUri = new Uri(serverUri, UriKind.Absolute);

        return query;
    }
    
    private static void WriteSorting(BinaryWriter bw, ResourceQuery query)
    {
        bw.Write(query.Sorting.Count);
        foreach (var (propertyInfo, name, sortDirection) in query.Sorting!)
        {
            if (!bw.WriteBoolean(propertyInfo != null))
                continue;

            bw.WriteNullableString(propertyInfo!.DeclaringType?.AssemblyQualifiedName);
            bw.WriteNullableString(propertyInfo.Name);
            bw.Write(name);
            bw.Write((byte)sortDirection);
        }
    }

    private static void ReadSorting(BinaryReader br, ResourceQuery query)
    {
        var sortingCount = br.ReadInt32();
        for (var i = 0; i < sortingCount; i++)
        {
            if (!br.ReadBoolean())
                continue;

            var declaringTypeName = br.ReadNullableString();
            var propertyInfoName = br.ReadNullableString();
            var name = br.ReadString();
            var direction = (SortDirection)br.ReadByte();

            if (string.IsNullOrWhiteSpace(declaringTypeName) || string.IsNullOrWhiteSpace(propertyInfoName))
                continue;

            var type = Type.GetType(declaringTypeName);
            if (type == null)
                continue;

            var propertyInfo = type.GetProperty(propertyInfoName);
            if (propertyInfo == null)
                continue;

            (PropertyInfo, string, SortDirection) sorting = new()
            {
                Item1 = propertyInfo,
                Item2 = name,
                Item3 = direction
            };
            query.Sorting ??= [];
            query.Sorting.Add(sorting);
        }
    }

    private static void WriteProjections(BinaryWriter bw, ResourceQuery query)
    {
        bw.Write(query.Projection.Count);
        foreach (var projection in query.Projection!)
            WriteProjectPath(bw, projection);
    }

    private static void ReadProjections(BinaryReader br, ResourceQuery query)
    {
        var projectionCount = br.ReadInt32();
        query.Projection = new List<ProjectionPath>(projectionCount);
        for (var i = 0; i < projectionCount; i++)
        {
            ReadProjection(br);
        }
    }

    private static void WriteProjectPath(BinaryWriter bw, ProjectionPath projection)
    {
        while (true)
        {
            bw.WriteNullableString(projection.Type?.AssemblyQualifiedName);
            bw.WriteNullableString(projection.Name);
            if (bw.WriteBoolean(projection.Next != null))
            {
                projection = projection.Next!;
                continue;
            }

            break;
        }
    }

    private static ProjectionPath ReadProjection(BinaryReader br)
    {
        var projection = new ProjectionPath();
        var typeName = br.ReadNullableString();
        if (typeName != null)
            projection.Type = Type.GetType(typeName);

        projection.Name = br.ReadNullableString();
        if (br.ReadBoolean())
            projection.Next = ReadProjection(br);

        return projection;
    }

    private static void WritePaging(BinaryWriter bw, ResourceQuery query)
    {
        if (!bw.WriteBoolean(query.Paging != null))
            return;

        bw.WriteNullableInt32(query.Paging!.PageOffset);
        bw.WriteNullableInt32(query.Paging!.PageSize);
        bw.WriteNullableInt32(query.Paging!.MaxPageSize);
    }

    private static void ReadPaging(BinaryReader br, ResourceQuery query)
    {
        if (!br.ReadBoolean())
            return;

        query.Paging = new Paging
        {
            PageOffset = br.ReadNullableInt32(),
            PageSize = br.ReadNullableInt32(),
            MaxPageSize = br.ReadNullableInt32()
        };
    }

    private static void WriteSearch(BinaryWriter bw, ResourceQuery query)
    {
        bw.Write(query.Search.Count);
        foreach (var (column, predicate) in query.Search!)
        {
            bw.WriteNullableString(column);
            bw.WriteNullableString(predicate);
        }
    }

    private static void ReadSearch(BinaryReader br, ResourceQuery query)
    {
        var searchCount = br.ReadInt32();
        for (var i = 0; i < searchCount; i++)
        {
            var column = br.ReadNullableString();
            var predicate = br.ReadNullableString();

            if (string.IsNullOrWhiteSpace(column) || string.IsNullOrWhiteSpace(predicate))
                continue;

            (string, string) search = new()
            {
                Item1 = column,
                Item2 = predicate
            };

            query.Search ??= [];
            query.Search.Add(search);
        }
    }

    private static void WriteFilter(BinaryWriter bw, ResourceQuery query)
    {
        bw.WriteNullableString(query.Filter);
    }

    private static void ReadFilter(BinaryReader br, ResourceQuery query)
    {
        query.Filter = br.ReadNullableString();
    }
}