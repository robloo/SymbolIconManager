using IconManager.Core.Icons;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace IconManager.Core.Glyphs
{
    /// <summary>
    /// Contains extension methods used with <see cref="IGlyphSourceProvider"/> instances.
    /// </summary>
    public static class IGlyphSourceProviderExtensions
    {
        /// <summary>
        /// Gets the image data stream (usually SVG or PNG format) for the defined glyph.
        /// </summary>
        /// <remarks>
        /// It is up to the caller to ensure the data stream is a supported format.
        /// This will be downloaded from an online source if needed.
        /// </remarks>
        /// <param name="glyphSourceProvider">The <see cref="IGlyphSourceProvider"/> to execute with.</param>
        /// <param name="iconSet">The icon set containing the glyph.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <returns>The glyph source image data stream; otherwise, null.</returns>
        public static async Task<GlyphStream?> GetDefaultGlyphSourceStreamAsync(
            this IGlyphSourceProvider glyphSourceProvider,
            IconSet iconSet,
            uint unicodePoint)
        {
            // Note: There is a priority to glyph sources that should be respected
            // The most local, highest fidelity data should be used first
            // Font glyph sources are NOT returned here (since they are a collection of glyphs)

            GlyphSource[] glyphSources =
            {
                GlyphSource.LocalSvgFile,
                GlyphSource.RemoteSvgFile,
                GlyphSource.RemotePngFile
            };

            foreach (var glyphSource in glyphSources)
            {
                Uri? glyphUri = glyphSourceProvider.GetGlyphSourceUri(
                    iconSet,
                    unicodePoint,
                    glyphSource);

                if (glyphUri is null)
                {
                    continue;
                }

                GlyphStream? glyphStream = await glyphSourceProvider.GetGlyphSourceStreamAsync(glyphUri);

                if (glyphStream is null)
                {
                    continue;
                }

                glyphStream.IconSet      = iconSet;
                glyphStream.Source       = glyphSource;
                glyphStream.SourceUri    = glyphUri;
                glyphStream.UnicodePoint = unicodePoint;

                return glyphStream;
            }

            return null;
        }

        /// <summary>
        /// Gets the image data stream of the specified source type for the defined glyph.
        /// </summary>
        /// <param name="glyphSourceProvider">The <see cref="IGlyphSourceProvider"/> to execute with.</param>
        /// <param name="iconSet">The icon set containing the glyph.</param>
        /// <param name="unicodePoint">The Unicode point of the glyph.</param>
        /// <param name="glyphSource">The type of glyph source to provide the image data stream for.</param>
        /// <returns>The glyph source image data stream; otherwise, null.</returns>
        public static async Task<GlyphStream?> GetGlyphSourceStreamAsync(
            this IGlyphSourceProvider glyphSourceProvider,
            IconSet iconSet,
            uint unicodePoint,
            GlyphSource glyphSource)
        {
            Uri? glyphUri = glyphSourceProvider.GetGlyphSourceUri(
                iconSet,
                unicodePoint,
                glyphSource);

            if (glyphUri is not null)
            {
                GlyphStream? glyphStream = await glyphSourceProvider.GetGlyphSourceStreamAsync(glyphUri);

                glyphStream?.IconSet      = iconSet;
                glyphStream?.Source       = glyphSource;
                glyphStream?.SourceUri    = glyphUri;
                glyphStream?.UnicodePoint = unicodePoint;

                return glyphStream;
            }

            return null;
        }

        /// <summary>
        /// Gets the image data stream (usually SVG or PNG format) of the glyph image file at the given URI.
        /// </summary>
        /// <remarks>
        /// It is up to the caller to ensure the data stream is a supported format.
        /// This will be downloaded from an online source if needed.
        /// </remarks>
        /// <param name="glyphSourceProvider">The <see cref="IGlyphSourceProvider"/> to execute with.</param>
        /// <param name="uri">The URI of the glyph source image file to download.</param>
        /// <returns>The glyph source image data stream; otherwise, null.</returns>
        public static async Task<GlyphStream?> GetGlyphSourceStreamAsync(
            this IGlyphSourceProvider glyphSourceProvider,
            Uri uri)
        {
            if (uri is null)
            {
                return null;
            }

            try
            {
                byte[] data;

                // Note: LocalPath MUST be used (AbsolutePath doesn't work in all cases)
                if (uri.IsFile &&
                    File.Exists(uri.LocalPath))
                {
                    data = await File.ReadAllBytesAsync(uri.LocalPath);
                }
                else
                {
                    using (var client = new HttpClient())
                    {
                        data = await client.GetByteArrayAsync(uri);
                    }
                }

                if (data is not null &&
                    data.Length > 0)
                {
                    return new GlyphStream(data)
                    {
                        SourceUri = uri
                    };
                }
            }
            catch { }

            return null;
        }
    }
}
