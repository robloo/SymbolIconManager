using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using System;
using System.Collections.Generic;

namespace IconManager.IconSets
{
    /// <summary>
    /// Provides glyph sources for the WinJS symbols font.
    /// </summary>
    public class WinJSSymbolsGlyphs
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="WinJSSymbolsGlyphs"/> class.
        /// </summary>
        public static readonly WinJSSymbolsGlyphs Instance = new();

        /***************************************************************************************
         *
         * Public Methods
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        public List<IconSet> GetIconSets()
        {
            return new List<IconSet>()
            {
                IconSet.WinJSSymbols
            };
        }

        /// <inheritdoc/>
        public List<GlyphSource> GetGlyphSources(IconSet iconSet, uint unicodePoint)
        {
            return new List<GlyphSource>()
            {
                GlyphSource.LocalFontFile
            };
        }

        /// <inheritdoc/>
        public Uri? GetGlyphSourceUri(IconSet iconSet, uint unicodePoint, GlyphSource glyphSource)
        {
            return null;
        }

        /// <inheritdoc/>
        public Uri? GetFontSourceUri(IconSet iconSet)
        {
            return iconSet switch
            {
                IconSet.WinJSSymbols => new Uri(Paths.WinJSSymbolsFont),
                _ => null
            };
        }
    }
}
