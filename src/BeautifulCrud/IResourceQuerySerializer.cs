namespace BeautifulCrud;

public interface IResourceQuerySerializer
{
    void Serialize(ResourceQuery query, BinaryWriter bw);
    ResourceQuery Deserialize(BinaryReader br);
}