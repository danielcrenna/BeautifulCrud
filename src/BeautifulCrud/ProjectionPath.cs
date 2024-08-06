namespace BeautifulCrud;

public sealed class ProjectionPath
{
    public Type? Type { get; set; }
    public string? Name { get; set; }
    public ProjectionPath? Next { get; set; }
}