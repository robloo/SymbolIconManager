using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using System;
using System.Collections.Generic;
using System.IO;

namespace IconManager.IconSets
{
    /// <summary>
    /// Provides glyph sources for the Windows 10 Segoe MDL2 Assets font.
    /// </summary>
    public class SegoeMDL2AssetsGlyphs : IGlyphSourceProvider
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="SegoeMDL2AssetsGlyphs"/> class.
        /// </summary>
        public static readonly SegoeMDL2AssetsGlyphs Instance = new();

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
        /// Initializes a new instance of the <see cref="SegoeMDL2AssetsGlyphs"/> class.
        /// </summary>
        public SegoeMDL2AssetsGlyphs()
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
                IconSet.SegoeMDL2Assets
            };
        }

        /// <inheritdoc/>
        public List<GlyphSource> GetGlyphSources(IconSet iconSet, uint unicodePoint)
        {
            var glyphSources = new List<GlyphSource>();

            if (iconSet != IconSet.SegoeMDL2Assets)
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
                string baseUrl = @"https://docs.microsoft.com/en-us/windows/apps/design/style/images/segoe-mdl/";
                return new Uri(baseUrl + Icon.ToUnicodeHexString(unicodePoint) + ".png");
            }

            return null;
        }

        /// <inheritdoc/>
        public Uri? GetFontSourceUri(IconSet iconSet)
        {
            return iconSet switch
            {
                IconSet.SegoeMDL2Assets => this._localFontUri,
                _ => null
            };
        }

        /// <summary>
        /// Searches for the Segoe MDL2 Assets font which may be installed on the system (usually requires Windows 10+).
        /// </summary>
        /// <returns>The Segoe MDL2 Assets font URI.</returns>
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
                "segmdl2.ttf");

            return File.Exists(systemFontPath)
                ? new Uri(systemFontPath)
                : null;
        }
    }
}
