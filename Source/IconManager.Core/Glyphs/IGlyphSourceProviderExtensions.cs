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
        public static async Task<MemoryStream?> GetDefaultGlyphSourceStreamAsync(
            this IGlyphSourceProvider glyphSourceProvider,
            IconSet iconSet,
            uint unicodePoint)
        {
            Uri? glyphUri = null;

            // Note: There is a priority to glyph sources that should be respected
            // The most local, highest fidelity data should be used first
            // Font glyph sources are NOT returned here (since they are a collection of glyphs)

            glyphUri ??= glyphSourceProvider.GetGlyphSourceUri(
                iconSet,
                unicodePoint,
                GlyphSource.LocalSvgFile);

            glyphUri ??= glyphSourceProvider.GetGlyphSourceUri(
                iconSet,
                unicodePoint,
                GlyphSource.RemoteSvgFile);

            glyphUri ??= glyphSourceProvider.GetGlyphSourceUri(
                iconSet,
                unicodePoint,
                GlyphSource.RemotePngFile);

            if (glyphUri is not null)
            {
                return await glyphSourceProvider.GetGlyphSourceStreamAsync(glyphUri);
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
        public static async Task<MemoryStream?> GetGlyphSourceStreamAsync(
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
                return await glyphSourceProvider.GetGlyphSourceStreamAsync(glyphUri);
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
        public static async Task<MemoryStream?> GetGlyphSourceStreamAsync(
            this IGlyphSourceProvider glyphSourceProvider,
            Uri uri)
        {
            try
            {
                if (uri is not null)
                {
                    // Note: LocalPath MUST be used (AbsolutePath doesn't work in all cases)
                    if (uri.IsFile &&
                        File.Exists(uri.LocalPath))
                    {
                        using (FileStream fileStream = File.OpenRead(uri.LocalPath))
                        {
                            var memoryStream = new MemoryStream(); 
                            fileStream.CopyTo(memoryStream);

                            return memoryStream;
                        }
                    }
                    else
                    {
                        using (var client = new HttpClient())
                        {
                            var data = await client.GetByteArrayAsync(uri);
                            return new MemoryStream(data);
                        }
                    }
                }
            }
            catch { }

            return null;
        }
    }
}
