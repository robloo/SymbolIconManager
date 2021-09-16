namespace IconManager
{
    /// <summary>
    /// Defines a potential source for a glyph.
    /// </summary>
    public enum GlyphSource
    {
        /// <summary>
        /// No glyph source is available.
        /// </summary>
        None = 0,

        /// <summary>
        /// The glyph is available in an existing font file.
        /// </summary>
        /// <remarks>
        /// This is suitable for use when building a font.
        /// </remarks>
        FontFile,

        /// <summary>
        /// The glyph is available as an image in an online (downloadable) file.
        /// </summary>
        /// <remarks>
        /// This is suitable to preview the glyph only, it cannot be used to build a font.
        /// </remarks>
        OnlineImage,

        /// <summary>
        /// The glyph is available in an SVG file.
        /// </summary>
        /// <remarks>
        /// This is suitable for use when building a font.
        /// </remarks>
        SvgFile
    }
}
