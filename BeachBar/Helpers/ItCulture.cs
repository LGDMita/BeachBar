using System.Globalization;

namespace BeachBar.Helpers;

/// <summary>
/// Cultura italiana condivisa tra i componenti Razor.
/// Evita di istanziare CultureInfo ad ogni chiamata di ToString nelle view.
/// </summary>
internal static class ItCulture
{
    public static readonly CultureInfo IT = new("it-IT");
}
