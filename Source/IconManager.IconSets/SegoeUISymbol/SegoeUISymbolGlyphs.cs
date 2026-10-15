using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using System;
using System.Collections.Generic;

namespace IconManager.IconSets
{
    /// <summary>
    /// Provides glyph sources for the Segoe UI Symbol font.
    /// </summary>
    public class SegoeUISymbolGlyphs : IGlyphSourceProvider
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="SegoeUISymbolGlyphs"/> class.
        /// </summary>
        public static readonly SegoeUISymbolGlyphs Instance = new();

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
                IconSet.SegoeUISymbol
            };
        }

        /// <inheritdoc/>
        public List<GlyphSource> GetGlyphSources(IconSet iconSet, uint unicodePoint)
        {
            return new List<GlyphSource>()
            {
                GlyphSource.RemotePngFile
            };
        }

        /// <inheritdoc/>
        public Uri? GetGlyphSourceUri(IconSet iconSet, uint unicodePoint, GlyphSource glyphSource)
        {
            if (glyphSource == GlyphSource.RemotePngFile)
            {
                string name = SegoeUISymbol.FindName(unicodePoint).ToLowerInvariant();

                // Naming does not match 1-to-1 with the URL and there are some special cases
                // Special cases are defined here, separately
                switch (name)
                {
                    case "account":
                        name = "accounts";
                        break;
                    case "character":
                        name = "characters";
                        break;
                    case "closedcaption":
                        name = "cc";
                        break;
                    case "mailfilled":
                        name = "mail2";
                        break;
                    case "page":
                        name = "pageicon";
                        break;
                    case "setting":
                        name = "settings";
                        break;
                }

                return new Uri(@"https://docs.microsoft.com/en-us/previous-versions/windows/apps/images/jj841127." + name + @"(en-us,win.10).png");
            }

            return null;
        }

        /// <inheritdoc/>
        public Uri? GetFontSourceUri(IconSet iconSet)
        {
            return iconSet switch
            {
                _ => null
            };
        }
    }
}
