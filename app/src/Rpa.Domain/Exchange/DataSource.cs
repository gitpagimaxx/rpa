namespace Rpa.Domain.Exchange;

/// <summary>
/// Value Object
/// </summary>
public readonly record struct DataSource(string Name)
{
    public override string ToString() => Name;

    public static DataSource From(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("O nome da fonte é obrigatório.", nameof(name));

        return new DataSource(name.Trim());
    }
}