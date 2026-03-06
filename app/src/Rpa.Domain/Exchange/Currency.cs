namespace Rpa.Domain.Exchange;

public readonly record struct Currency(string Code)
{
    public static Currency USD => new("USD");
    public static Currency BRL => new("BRL");

    public override string ToString() => Code;

    public static Currency From(string code)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Currency code is required.", nameof(code));

        code = code.Trim().ToUpperInvariant();
        if (code.Length is < 3 or > 8)
            throw new ArgumentException("Invalid currency code length.", nameof(code));

        return new Currency(code);
    }
}