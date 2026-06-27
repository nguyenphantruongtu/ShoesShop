using System.Text;
using System.Text.RegularExpressions;

namespace ShoesShop.Shared.Helpers;

public static class SlugHelper
{
    public static string Generate(string text)
    {
        // Normalize Unicode (remove diacritics for Vietnamese)
        var normalized = text.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var c in normalized)
        {
            var category = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (category != System.Globalization.UnicodeCategory.NonSpacingMark)
                sb.Append(c);
        }

        var slug = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug.Trim('-');
    }

    public static string GenerateUnique(string text, string? suffix = null)
    {
        var base_ = Generate(text);
        return suffix is null ? base_ : $"{base_}-{suffix}";
    }
}
