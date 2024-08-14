namespace BeautifulCrud.Tests.Models;

public class ContinuationTokenGenerators : TheoryData<IContinuationTokenGenerator>
{
    public ContinuationTokenGenerators(Func<DateTimeOffset> timestamps)
    {
        Add(new PortableContinuationTokenGenerator(timestamps, new Base64QueryHashEncoder(), new BinaryResourceQuerySerializer()));
        Add(new InMemoryContinuationTokenGenerator(timestamps, new BinaryResourceQuerySerializer()));
    }
}