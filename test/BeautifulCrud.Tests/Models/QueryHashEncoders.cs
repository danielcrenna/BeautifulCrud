namespace BeautifulCrud.Tests.Models;

public class QueryHashEncoders : TheoryData<IQueryHashEncoder>
{
    public QueryHashEncoders()
    {
        Add(new Base64QueryHashEncoder());
    }
}