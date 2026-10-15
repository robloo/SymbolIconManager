using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using System;
using System.Collections.Generic;

namespace IconManager.IconSets
{
    /// <summary>
    /// Provides glyph sources for the Segoe Fluent font.
    /// </summary>
    public class SegoeFluentGlyphs : IGlyphSourceProvider
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="SegoeFluentGlyphs"/> class.
        /// </summary>
        public static readonly SegoeFluentGlyphs Instance = new();

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
                IconSet.SegoeFluent
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
                string baseUrl = @"https://docs.microsoft.com/en-us/windows/apps/design/style/images/glyphs/segoe-fluent-icons/";
                return new Uri(baseUrl + Icon.ToUnicodeHexString(unicodePoint) + ".png");
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
