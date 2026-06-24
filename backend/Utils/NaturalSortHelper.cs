namespace Backend.Utils;

public static class NaturalSortHelper
{
    public static IEnumerable<T> ByCode<T>(IEnumerable<T> source, Func<T, string> key)
        => source.OrderBy(x => GetPrefix(key(x))).ThenBy(x => GetNumber(key(x)));

    private static string GetPrefix(string code)
    {
        var i = code.Length - 1;
        while (i >= 0 && char.IsDigit(code[i])) i--;
        return code[..(i + 1)].ToUpperInvariant();
    }

    private static int GetNumber(string code)
    {
        var i = code.Length - 1;
        while (i >= 0 && char.IsDigit(code[i])) i--;
        var num = code[(i + 1)..];
        return num.Length > 0 ? int.Parse(num) : int.MaxValue;
    }
}
