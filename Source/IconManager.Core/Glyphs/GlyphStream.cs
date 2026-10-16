using IconManager.Core.Icons;
using System;
using System.IO;

namespace IconManager.Core.Glyphs
{
    /// <summary>
    /// Creates an in-memory stream of glyph source data along with icon glyph properties.
    /// </summary>
    public class GlyphStream : MemoryStream
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new, non-resizable instance of the <see cref="GlyphStream"/> class
        /// based on the specified byte array.
        /// </summary>
        /// <param name="buffer">The array of unsigned bytes from which to create the current stream.</param>
        public GlyphStream(byte[] buffer)
            : base(buffer)
        {
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets or sets the <see cref="Icons.IconSet"/> that contains the icon glyph.
        /// </summary>
        /// <remarks>
        /// This should be used to interpret and differentiate the <see cref="UnicodePoint"/>.
        /// </remarks>
        public IconSet IconSet { get; set; } = IconSet.Undefined;

        /// <summary>
        /// Gets the original source of the icon glyph stream.
        /// </summary>
        public GlyphSource Source { get; set; } = GlyphSource.None;

        /// <summary>
        /// Gets the original source URI fo the icon glyph stream.
        /// </summary>
        public Uri? SourceUri { get; set; } = null;

        /// <summary>
        /// Gets or sets the 32-bit Unicode point of the icon glyph.
        /// </summary>
        public uint UnicodePoint { get; set; } = 0;
    }
}
