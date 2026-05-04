public static class CurrencyFormatter
{
    public static string Format(long value)
    {
        if (value >= 10_000_000_000L) return (value / 1_000_000_000L) + "b";
        if (value >= 10_000_000L)     return (value / 1_000_000L) + "m";
        if (value >= 10_000L)         return (value / 1_000L) + "k";
        return value.ToString();
    }
}