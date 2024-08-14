namespace BeautifulCrud.Tests.Models;

public class ResourceQuerySerializers : TheoryData<IResourceQuerySerializer>
{
    public ResourceQuerySerializers()
    {
        Add(new BinaryResourceQuerySerializer());
    }
}