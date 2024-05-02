using Avalonia;
using Avalonia.Media;

namespace Mdk.Avalonia.Example;

internal static class FontExtension
{
    internal static AppBuilder UseCHSFonts(this AppBuilder builder)
    {
        return builder.With(new FontManagerOptions
        {
            DefaultFamilyName = "Microsoft YaHei",
            FontFallbacks =
            [
                new FontFallback
                {
                    FontFamily = new FontFamily("Segoe UI")
                },
                new FontFallback
                {
                    FontFamily = new FontFamily("WenQuanYi Micro Hei")
                }
            ]
        });
    }
}