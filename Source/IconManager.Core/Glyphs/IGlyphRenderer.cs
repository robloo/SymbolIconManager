using IconManager.Core.Icons;
using System.Threading.Tasks;

namespace IconManager.Core.Glyphs
{
    /// <summary>
    /// Defines a renderer of icon glyphs.
    /// </summary>
    /// <typeparam name="TGlyph">
    /// The specific type of rendered glyph.
    /// </typeparam>
    public interface IGlyphRenderer<TGlyph>
    {
        /// <summary>
        /// Gets the height, in pixels, of the rendered glyph.
        /// </summary>
        public uint RenderHeight { get; }

        /// <summary>
        /// Gets the width, in pixels, of the rendered glyph.
        /// </summary>
        public uint RenderWidth { get; }

        /// <summary>
        /// Gets a rendered glyph for the defined icon set and Unicode point.
        /// </summary>
        /// <param name="iconSet">The icon set containing the Unicode point.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <param name="glyphSourceProvider">The optional, specific glyph source data provider.</param>
        /// <param name="glyphSource">The optional, specific glyph source type to use (if available).</param>
        /// <returns>A render of the specified icon glyph; otherwise, null.</returns>
        Task<TGlyph?> RenderGlyphAsync(
            IconSet iconSet,
            uint unicodePoint,
            IGlyphSourceProvider? glyphSourceProvider = null,
            GlyphSource glyphSource = GlyphSource.None);
    }
}
