using Avalonia;
using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using System;
using System.IO;
using Bitmap = Avalonia.Media.Imaging.Bitmap;

namespace IconManager
{
    /// <summary>
    /// Holds an Avalonia <see cref="Bitmap"/> image along with icon glyph properties.
    /// </summary>
    public class GlyphBitmap : Bitmap
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap"/> class.
        /// </summary>
        /// <param name="stream">The stream to read the bitmap from.</param>
        public GlyphBitmap(Stream stream)
            : base (stream)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Bitmap"/> class.
        /// </summary>
        /// <param name="stream">The glyph stream to read the bitmap from.</param>
        public GlyphBitmap(GlyphStream stream)
            : base (stream)
        {
            this.IconSet      = stream.IconSet;
            this.Source       = stream.Source;
            this.SourceUri    = stream.SourceUri;
            this.UnicodePoint = stream.UnicodePoint;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <inheritdoc cref="GlyphStream.IconSet"/>
        public IconSet IconSet { get; set; } = IconSet.Undefined;

        /// <summary>
        /// Gets or sets any SVG path data from the original source before it was rendered.
        /// </summary>
        public string PathData { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the SVG path data view box used to interpret overall dimensions.
        /// </summary>
        public Rect PathViewBox { get; set; } = default(Rect);

        /// <inheritdoc cref="GlyphStream.Source"/>
        public GlyphSource Source { get; set; } = GlyphSource.None;

        /// <inheritdoc cref="GlyphStream.SourceUri"/>
        public Uri? SourceUri { get; set; } = null;

        /// <inheritdoc cref="GlyphStream.UnicodePoint"/>
        public uint UnicodePoint { get; set; } = 0;
    }
}
