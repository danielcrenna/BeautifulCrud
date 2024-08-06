namespace BeautifulCrud;

public interface IKeyed<TKey> : IKeyed
{
    TKey Id { get; set; }
}

public interface IKeyed;