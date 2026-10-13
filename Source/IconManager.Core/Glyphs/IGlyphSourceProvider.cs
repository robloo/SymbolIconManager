using IconManager.Core.Icons;
using System;
using System.Collections.Generic;

namespace IconManager.Core.Glyphs
{
    /// <summary>
    /// Defines a provider of glyph source data.
    /// </summary>
    /// <remarks>
    /// Glyph source data is the information used to construct glyphs -- it is not actual glyphs which
    /// must be separately retrieved, processed and rendered.
    /// </remarks>
    public interface IGlyphSourceProvider
    {
        /// <summary>
        /// Gets a list of all icon sets supported by the glyph provider.
        /// </summary>
        /// <returns>All icon sets supported by the glyph provider.</returns>
        List<IconSet> GetIconSets();

        /// <summary>
        /// Gets all possible glyph sources for the given icon set and Unicode point.
        /// </summary>
        /// <remarks>
        /// Note that 'possible' here means it may exist, but is not guaranteed.
        /// Use an associated GetGlyphSource method to confirm.
        /// </remarks>
        /// <param name="iconSet">The icon set containing the glyph.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <returns>The list of possible glyph sources.</returns>
        List<GlyphSource> GetGlyphSources(IconSet iconSet, uint unicodePoint);

        /// <summary>
        /// Gets the data source URI of the defined glyph.
        /// </summary>
        /// <param name="iconSet">The icon set containing the glyph.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <param name="glyphSource">The type of glyph source to provide the URL for.</param>
        /// <returns>The URI of the glyph's image data source; otherwise, null.</returns>
        Uri? GetGlyphSourceUri(IconSet iconSet, uint unicodePoint, GlyphSource glyphSource);

        /// <summary>
        /// Gets the font data source URI of the defined icon set.
        /// </summary>
        /// <param name="iconSet">The icon set to get the font URI for.</param>
        /// <returns>The URI for the icon set's font data source; otherwise, null.</returns>
        Uri? GetFontSourceUri(IconSet iconSet);
    }
}
