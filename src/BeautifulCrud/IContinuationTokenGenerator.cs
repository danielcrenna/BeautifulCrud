namespace BeautifulCrud;

public interface IContinuationTokenGenerator
{
    string? Build(Type context, ResourceQuery query);
    ResourceQuery? Parse(Type type, string? continuationToken);
}