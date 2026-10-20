using Avalonia.Platform;
using IconManager.Core.Glyphs;
using IconManager.Core.Icons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Threading;

namespace IconManager.IconSets
{
    /// <summary>
    /// Provides glyph sources for the Line Awesome font family.
    /// </summary>
    public class LineAwesomeGlyphs : IGlyphSourceProvider
    {
        /// <summary>
        /// Gets the singleton instance of the <see cref="LineAwesomeGlyphs"/> class.
        /// </summary>
        public static readonly LineAwesomeGlyphs Instance = new();

        private static List<string>? _cachedLocalLineAwesomeGlyphSourcePaths  = null;
        private static List<string>? _cachedRemoteLineAwesomeGlyphSourcePaths = null;

        private static Lock _cachedLocalGlyphSourcesLock  = new();
        private static Lock _cachedRemoteGlyphSourcesLock = new();

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
                IconSet.LineAwesomeBrand,
                IconSet.LineAwesomeRegular,
                IconSet.LineAwesomeSolid
            };
        }

        /// <inheritdoc/>
        public List<GlyphSource> GetGlyphSources(IconSet iconSet, uint unicodePoint)
        {
            if (iconSet != IconSet.LineAwesomeBrand &&
                iconSet != IconSet.LineAwesomeRegular &&
                iconSet != IconSet.LineAwesomeSolid)
            {
                return new List<GlyphSource>();
            }

            return new List<GlyphSource>()
            {
                GlyphSource.LocalFontFile,
                GlyphSource.LocalSvgFile,
                GlyphSource.RemoteSvgFile
            };
        }

        /// <inheritdoc/>
        public Uri? GetGlyphSourceUri(IconSet iconSet, uint unicodePoint, GlyphSource glyphSource)
        {
            string nameBase;

            if (glyphSource == GlyphSource.LocalSvgFile)
            {
                string finalGlyphFilePath = string.Empty;
                List<string>? glyphFilePaths = null;

                nameBase = IconSetBase.FindName(iconSet, unicodePoint);

                if (string.IsNullOrWhiteSpace(nameBase))
                {
                    return null;
                }

                // Naming does not match 1-to-1 with the URL and there are some special cases
                // Special cases are defined here, separately
                switch (nameBase)
                {
                    case "alternate-square-root":
                        nameBase = "square-root-alt-solid";
                        break;
                    case "at":
                        nameBase = "at-solid";
                        break;
                    case "i-beam-cursor":
                        nameBase = "i-cursor-solid";
                        break;
                    case "lightning-bolt":
                        nameBase = "bolt-solid";
                        break;
                }

                if (_cachedLocalLineAwesomeGlyphSourcePaths is null)
                {
                    LineAwesomeGlyphs.BuildLocalGlyphSourcePathsCache();
                }

                lock (_cachedLocalGlyphSourcesLock)
                {
                    glyphFilePaths = _cachedLocalLineAwesomeGlyphSourcePaths!.FindAll(s => s.EndsWith($@"{nameBase}.svg"));

                    if (glyphFilePaths.Count == 0)
                    {
                        // Only SVG sources are available for Line Awesome so the search can remove the extension
                        // This is necessary for the Line Awesome font family because exact names are not enforced
                        // There is often some slight variation such as 'unlink' name with 'unlink-solid.svg' file
                        // In this example some file names have the style added to the end
                        glyphFilePaths = _cachedLocalLineAwesomeGlyphSourcePaths!.FindAll(s => s.Contains(nameBase));
                    }
                }

                // Use the relativeGlyphUrl with the smallest directory structure
                // There are sometimes many variants with the exact same file name -- some for other cultures
                // Each culture is usually placed in it's own folder
                // We want the invariant culture (smallest directory structure), as best as possible
                if (glyphFilePaths is not null &&
                    glyphFilePaths.Count > 0)
                {
                    finalGlyphFilePath = glyphFilePaths[0];

                    for (int i = 1; i < glyphFilePaths.Count; i++)
                    {
                        // Not the most efficient to keep splitting, but it's easiest
                        if (glyphFilePaths[i].Split('\\').Length < finalGlyphFilePath.Split('\\').Length)
                        {
                            finalGlyphFilePath = glyphFilePaths[i];
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(finalGlyphFilePath) == false)
                {
                    return new Uri(finalGlyphFilePath);
                }
            }

            if (glyphSource == GlyphSource.RemoteSvgFile)
            {
                string relativeGlyphUrl = string.Empty;
                List<string>? relativeGlyphUrls = null;

                nameBase = IconSetBase.FindName(iconSet, unicodePoint);

                if (string.IsNullOrWhiteSpace(nameBase))
                {
                    return null;
                }

                // Naming does not match 1-to-1 with the URL and there are some special cases
                // Special cases are defined here, separately
                switch (nameBase)
                {
                    case "alternate-square-root":
                        nameBase = "square-root-alt-solid";
                        break;
                    case "at":
                        nameBase = "at-solid";
                        break;
                    case "i-beam-cursor":
                        nameBase = "i-cursor-solid";
                        break;
                    case "lightning-bolt":
                        nameBase = "bolt-solid";
                        break;
                }

                lock (_cachedRemoteGlyphSourcesLock)
                {
                    if (_cachedRemoteLineAwesomeGlyphSourcePaths is null)
                    {
                        // Rebuild the cache
                        var sources = new List<string>();

                        using (var sourceStream = AssetLoader.Open(new Uri(IconSets.Paths.LineAwesomeGlyphSources)))
                        using (var reader = new StreamReader(sourceStream))
                        {
                            string jsonString = reader.ReadToEnd();
                            var rawGlyphSources = JsonSerializer.Deserialize<string[]>(jsonString);

                            if (rawGlyphSources is not null)
                            {
                                foreach (var entry in rawGlyphSources)
                                {
                                    sources.Add(entry);
                                }
                            }
                        }

                        _cachedRemoteLineAwesomeGlyphSourcePaths = sources;
                    }

                    relativeGlyphUrls = _cachedRemoteLineAwesomeGlyphSourcePaths!.FindAll(s => s.EndsWith($@"{nameBase}.svg"));

                    if (relativeGlyphUrls.Count == 0)
                    {
                        // Only SVG sources are available for Line Awesome so the search can remove the extension
                        // This is necessary for the Line Awesome font family because exact names are not enforced
                        // There is often some slight variation such as 'unlink' name with 'unlink-solid.svg' file
                        // In this example some file names have the style added to the end
                        relativeGlyphUrls = _cachedRemoteLineAwesomeGlyphSourcePaths!.FindAll(s => s.Contains(nameBase));
                    }
                }

                // Use the relativeGlyphUrl with the smallest directory structure
                // There are sometimes many variants with the exact same file name -- some for other cultures
                // Each culture is usually placed in it's own folder
                // We want the invariant culture (smallest directory structure), as best as possible
                if (relativeGlyphUrls is not null &&
                    relativeGlyphUrls.Count > 0)
                {
                    relativeGlyphUrl = relativeGlyphUrls[0];

                    for (int i = 1; i < relativeGlyphUrls.Count; i++)
                    {
                        // Not the most efficient to keep splitting, but it's easiest
                        if (relativeGlyphUrls[i].Split('\\').Length < relativeGlyphUrl.Split('\\').Length)
                        {
                            relativeGlyphUrl = relativeGlyphUrls[i];
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(relativeGlyphUrl) == false)
                {
                    try
                    {
                        // Note that the relative URL determined above starts with '/'
                        string baseUrl = @"https://raw.githubusercontent.com/icons8/line-awesome/master/svg";
                        return new Uri($@"{baseUrl}{relativeGlyphUrl}");
                    }
                    catch { }
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public Uri? GetFontSourceUri(IconSet iconSet)
        {
            return iconSet switch
            {
                IconSet.LineAwesomeBrand   => new Uri(IconSets.Paths.LineAwesomeBrandsFont),
                IconSet.LineAwesomeRegular => new Uri(IconSets.Paths.LineAwesomeRegularFont),
                IconSet.LineAwesomeSolid   => new Uri(IconSets.Paths.LineAwesomeSolidFont),
                _ => null
            };
        }

        /***************************************************************************************
         *
         * Private Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Builds the full list of local glyph source paths.
        /// </summary>
        private static void BuildLocalGlyphSourcePathsCache()
        {
            lock (_cachedLocalGlyphSourcesLock)
            {
                List<string> glyphSourcePaths = [];

                string exePath = Assembly.GetExecutingAssembly().Location;
                string sourceDirectoryPath = new DirectoryInfo(exePath).Parent!.Parent!.Parent!.Parent!.Parent!.FullName;
                string searchDirectory = Path.Combine(sourceDirectoryPath, "IconManager.IconSets", "LineAwesome", "GlyphSources");

                foreach (string filePath in Directory.EnumerateFiles(searchDirectory, "*.*", SearchOption.AllDirectories))
                {
                    if (Path.GetExtension(filePath).Equals(".PDF", StringComparison.InvariantCultureIgnoreCase) ||
                        Path.GetExtension(filePath).Equals(".SVG", StringComparison.InvariantCultureIgnoreCase))
                    {
                        // Do NOT remove the search directory to make relative paths
                        // Keep the full system file path
                        glyphSourcePaths.Add(filePath);
                    }
                }

                _cachedLocalLineAwesomeGlyphSourcePaths = glyphSourcePaths;
            }

            return;
        }
    }
}
