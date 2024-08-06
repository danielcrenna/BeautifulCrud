namespace BeautifulCrud;

public class One<T>
{
    public One() { }

    public One(T? value) : this()
    {
        Value = value;
    }

    public One(T? value, bool error) : this()
    {
        Value = value;
        Error = error;
    }

    public T? Value { get; set; }
	public bool Found => Value != null;
	public bool Error { get; set; }
}