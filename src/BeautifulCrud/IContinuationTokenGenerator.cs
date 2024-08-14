namespace BeautifulCrud;

public interface IContinuationTokenGenerator
{
    string? Build(Type context, ResourceQuery query);
    ResourceQuery? Parse(Type context, string? continuationToken);
}