using System.Globalization;
using System.Text;

namespace ShoesShop.Business.Helpers;

public static class CsvBuilder
{
    // Excel trên Windows mở CSV tiếng Việt ổn định nhất với UTF-16 LE + BOM (FF FE).
    private static readonly UnicodeEncoding Utf16LeBom = new(bigEndian: false, byteOrderMark: true);

    public static byte[] Build(string[] headers, IEnumerable<IEnumerable<object?>> rows)
    {
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(",", headers.Select(Escape)));

        foreach (var row in rows)
        {
            sb.AppendLine(string.Join(",", row.Select(value => Escape(FormatValue(value)))));
        }

        return Utf16LeBom.GetBytes(sb.ToString());
    }

    private static string? FormatValue(object? value) => value switch
    {
        null => null,
        DateTime dt => dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
        decimal d => d.ToString("0.##", CultureInfo.InvariantCulture),
        double d => d.ToString("0.##", CultureInfo.InvariantCulture),
        float f => f.ToString("0.##", CultureInfo.InvariantCulture),
        bool b => b ? "Có" : "Không",
        _ => value.ToString()
    };

    private static string Escape(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var needsQuotes = value.Contains(',') || value.Contains('"') || value.Contains('\n') || value.Contains('\r');
        if (!needsQuotes)
        {
            return value;
        }

        return $"\"{value.Replace("\"", "\"\"")}\"";
    }
}
