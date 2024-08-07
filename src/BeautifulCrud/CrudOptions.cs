namespace BeautifulCrud;

public sealed class CrudOptions
{
    public Features Features { get; set; } = Features.All;

    public string CountOperator { get; set; } = "$count";
    public string ExcludeOperator { get; set; } = "$exclude";
    public string FilterOperator { get; set; } = "$filter";
    public string IncludeOperator { get; set; } = "$include";
    public string MaxPageSizeOperator { get; set; } = "$maxpagesize";
    public string OrderByOperator { get; set; } = "$orderBy";
    public string SelectOperator { get; set; } = "$select";
    public string SkipOperator { get; set; } = "$skip";
    public string TopOperator { get; set; } = "$top";

    public int DefaultPageSize { get; set; } = 25;
}