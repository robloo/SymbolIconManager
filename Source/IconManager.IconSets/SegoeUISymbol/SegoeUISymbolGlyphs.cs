using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using System;
using System.Collections.Generic;
using System.IO;

namespace IconManager.IconSets
{
    /// <summary>
    /// Provides glyph sources for the Windows 8/8.1 Segoe UI Symbol font.
    /// </summary>
    public class SegoeUISymbolGlyphs : IGlyphSourceProvider
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="SegoeUISymbolGlyphs"/> class.
        /// </summary>
        public static readonly SegoeUISymbolGlyphs Instance = new();

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
        /// Initializes a new instance of the <see cref="SegoeUISymbolGlyphs"/> class.
        /// </summary>
        public SegoeUISymbolGlyphs()
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
                IconSet.SegoeUISymbol
            };
        }

        /// <inheritdoc/>
        public List<GlyphSource> GetGlyphSources(IconSet iconSet, uint unicodePoint)
        {
            var glyphSources = new List<GlyphSource>();

            if (iconSet != IconSet.SegoeUISymbol)
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
                string name = SegoeUISymbol.FindName(unicodePoint).ToLowerInvariant();

                // Naming does not match 1-to-1 with the URL and there are some special cases
                // Special cases are defined here, separately
                switch (name)
                {
                    case "account":
                        name = "accounts";
                        break;
                    case "character":
                        name = "characters";
                        break;
                    case "closedcaption":
                        name = "cc";
                        break;
                    case "mailfilled":
                        name = "mail2";
                        break;
                    case "page":
                        name = "pageicon";
                        break;
                    case "setting":
                        name = "settings";
                        break;
                }

                return new Uri(@"https://docs.microsoft.com/en-us/previous-versions/windows/apps/images/jj841127." + name + @"(en-us,win.10).png");
            }

            return null;
        }

        /// <inheritdoc/>
        public Uri? GetFontSourceUri(IconSet iconSet)
        {
            return iconSet switch
            {
                IconSet.SegoeUISymbol => this._localFontUri,
                _ => null
            };
        }

        /// <summary>
        /// Searches for the Segoe UI Symbol font which may be installed on the system (usually requires Windows 8+).
        /// </summary>
        /// <returns>The Segoe UI Symbol font URI.</returns>
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
                "seguisym.ttf");

            return File.Exists(systemFontPath)
                ? new Uri(systemFontPath)
                : null;
        }
    }
}
