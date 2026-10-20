using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using System;
using System.Collections.Generic;
using System.IO;

namespace IconManager.IconSets
{
    /// <summary>
    /// Provides glyph sources for the Windows 11 Segoe Fluent font.
    /// </summary>
    public class SegoeFluentGlyphs : IGlyphSourceProvider
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="SegoeFluentGlyphs"/> class.
        /// </summary>
        public static readonly SegoeFluentGlyphs Instance = new();

        /// <summary>
        /// Caches the local font URI, including a missing-font result.
        /// </summary>
        private readonly Uri? _localFontUri;

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="SegoeFluentGlyphs"/> class.
        /// </summary>
        public SegoeFluentGlyphs()
        {
            this._localFontUri = FindLocalFontFileOnSystem();
        }

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
            var glyphSources = new List<GlyphSource>();

            if (iconSet != IconSet.SegoeFluent)
            {
                return glyphSources;
            }

            glyphSources.Add(GlyphSource.RemotePngFile);

            if (this._localFontUri is not null)
            {
                glyphSources.Insert(0, GlyphSource.LocalFontFile);
            }

            return glyphSources;
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
                IconSet.SegoeFluent => this._localFontUri,
                _ => null
            };
        }

        /// <summary>
        /// Searches for the Segoe Fluent font which may be installed on the system (usually requires Windows 11+).
        /// </summary>
        /// <returns>The Segoe Fluent font URI.</returns>
        private static Uri? FindLocalFontFileOnSystem()
        {
            if (!OperatingSystem.IsWindows())
            {
                return null;
            }

            string windowsDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Windows);

            if (string.IsNullOrWhiteSpace(windowsDirectory))
            {
                return null;
            }

            string systemFontPath = Path.Combine(
                windowsDirectory,
                "Fonts",
                "SegoeIcons.ttf");

            return File.Exists(systemFontPath)
                ? new Uri(systemFontPath)
                : null;
        }
    }
}
