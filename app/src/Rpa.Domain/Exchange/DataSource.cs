namespace Rpa.Domain.Exchange;

public readonly record struct DataSource(string Name)
{
    public override string ToString() => Name;

    public static DataSource From(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Source name is required.", nameof(name));
        return new DataSource(name.Trim());
    }
}