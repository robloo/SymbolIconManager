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
        /// The glyph is locally available in an existing font file.
        /// </summary>
        /// <remarks>
        /// This is SOMETIMES suitable for use when building a font.
        /// It is not suitable for use when building a font if the local font is proprietary.
        /// </remarks>
        LocalFontFile,

        /// <summary>
        /// The glyph is locally available as an SVG image file.
        /// </summary>
        /// <remarks>
        /// This is generally suitable for use when building a font.
        /// </remarks>
        LocalSvgFile,

        /// <summary>
        /// The glyph is downloadable as a PNG image file from an online location.
        /// </summary>
        /// <remarks>
        /// This is suitable to preview the glyph only, it cannot be used to build a font.
        /// </remarks>
        RemotePngFile,

        /// <summary>
        /// The glyph is downloadable as an SVG image file from an online location.
        /// </summary>
        /// <remarks>
        /// This is generally suitable for use when building a font.
        /// </remarks>
        RemoteSvgFile
    }
}
