using Avalonia;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;

namespace IconManager
{
    public class FontBuilder
    {
        // File & directory names
        private const string BuildRootDirectoryName = @"BuildFont";
        private const string GlyphSubDirectoryName  = @"glyph_sources";
        private const string PythonScriptFileName   = @"build_font.py";
        private const string WindowsScriptFileName  = @"build_win.bat";
        private const string OutputFontFileName     = @"output_font.ttf";

        // File paths
        private const string FontForgeFilePath = @"C:\Program Files (x86)\FontForgeBuilds\run_fontforge.exe";

        private static List<string>? cachedFluentUISystemGlyphSources = null;

        private static object directoryMutex = new object();
        private static object cacheMutex     = new object();

        /// <summary>
        /// Defines possible sources for a glyph.
        /// </summary>
        private enum GlyphSource
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

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public FontBuilder()
        {
        }

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/



        public void BuildFont(IconMappingList mappings)
        {
            if (mappings.Count == 0)
            {
                return;
            }

            // Create the directories
            string outputDirectory = this.CreateNewBuildDirectory();
            Directory.CreateDirectory(Path.Combine(outputDirectory, GlyphSubDirectoryName));

            // Write the platform-dependent build scripts
            using (MemoryStream winScript = this.BuildWindowsScript())
            {
                this.WriteStreamToFile(
                    winScript,
                    Path.Combine(outputDirectory, WindowsScriptFileName));
            }

            // Assemble all glyphs in the sources directory


            // Write the python script to build the actual font with FontForge
            using (MemoryStream pythonScript = this.BuildPythonFontScript(mappings, outputDirectory))
            {
                this.WriteStreamToFile(
                    pythonScript,
                    Path.Combine(outputDirectory, PythonScriptFileName));
            }

            return;
        }

        private string CreateNewBuildDirectory()
        {
            string directory;
            int currSuffix = -1;

            // We don't want potentially more than 1 thread determining the next available
            // directory at the same time
            lock (directoryMutex)
            {
                do
                {
                    currSuffix++;

                    // Start from the directory of the running application
                    directory = Path.Combine(
                        Directory.GetCurrentDirectory(),
                        BuildRootDirectoryName + currSuffix.ToString(CultureInfo.InvariantCulture));

                } while (Directory.Exists(directory));
            }

            return directory;
        }

        private MemoryStream BuildPythonFontScript(
            IconMappingList mappings,
            string outputDirectory)
        {
            var sb = new StringBuilder();
            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;

            sb.AppendLine(@"import fontforge");
            sb.AppendLine();
            sb.AppendLine(@"# Create a new, empty font");
            sb.AppendLine(@"font = fontforge.font()");
            sb.AppendLine();

            //      Source: Name is ignored, use IconSet+UnicodePoint to lookup the glyph
            // Destination: IconSet and Name is ignored, the only relevant data is Unicode Point

            foreach (IconMapping mapping in mappings)
            {
                if (mapping.Source.IconSet == IconSet.FluentUISystemFilled ||
                    mapping.Source.IconSet == IconSet.FluentUISystemRegular)
                {
                    var nameComponents = new FluentUISystem.IconName(mapping.Source.Name);

                    lock (cacheMutex)
                    {
                        if (cachedFluentUISystemGlyphSources == null)
                        {
                            RebuildCache();
                        }
                    }

                    // Find all possible sources of the SVG file
                    string svgName = $@"{nameComponents.Name}.svg";
                    string relativeGlyphUrl = string.Empty;
                    List<string> relativeGlyphUrls = cachedFluentUISystemGlyphSources!.FindAll(s => s.EndsWith(svgName));

                    // Use the relativeGlyphUrl with the smallest directory structure
                    // There are sometimes many variants with the exact same name -- some for other cultures
                    // Each culture is usually placed in it's own folder
                    // We want the invariant culture, as best as possible
                    if (relativeGlyphUrls.Count > 0)
                    {
                        relativeGlyphUrl = relativeGlyphUrls[0];

                        for (int i = 1; i < relativeGlyphUrls.Count; i++)
                        {
                            if (relativeGlyphUrls[i].Split('\\').Length < relativeGlyphUrl.Split('\\').Length)
                            {
                                relativeGlyphUrl = relativeGlyphUrls[i];
                            }
                        }
                    }

                    Uri svgUrl = new Uri($@"https://raw.githubusercontent.com/microsoft/fluentui-system-icons/master/assets/{relativeGlyphUrl}");

                    using (var webClient = new WebClient())
                    {
                        webClient.DownloadDataCompleted += (sender, args) =>
                        {
                            var filePath = Path.Combine(outputDirectory, GlyphSubDirectoryName, svgName);
                            using (var fileStream = File.OpenWrite(filePath))
                            {
                                fileStream.Write(args.Result);
                            }
                        };

                        webClient.DownloadDataAsync(svgUrl);
                    }

                    sb.AppendLine($@"char = font.createChar(0x{mapping.Destination.UnicodeString})");
                    sb.AppendLine($@"char.importOutlines('{GlyphSubDirectoryName}\{svgName}')");
                    sb.AppendLine();
                }
            }

            sb.AppendLine($@"font.generate('{OutputFontFileName}')");

            return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        private MemoryStream BuildWindowsScript()
        {
            var sb = new StringBuilder();

            sb.AppendLine(@"set scriptPath=%cd%");
            sb.AppendLine();
            sb.AppendLine($@"""{FontForgeFilePath}"" -script ""%scriptPath%\{PythonScriptFileName}""");
            sb.AppendLine();
            sb.AppendLine(@"PAUSE");

            return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        private void WriteStreamToFile(MemoryStream stream, string filePath)
        {

            if (string.IsNullOrWhiteSpace(filePath) == false)
            {
                if (File.Exists(filePath))
                {
                    // Delete the existing file, it will be replaced
                    File.Delete(filePath);
                }

                if (Directory.Exists(Path.GetDirectoryName(filePath)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filePath));
                }

                using (var fileStream = File.OpenWrite(filePath))
                {
                    stream.WriteTo(fileStream);
                }
            }

            return;
        }

        private static void RebuildCache()
        {
            var sources = new List<string>();
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            string sourceDataPath = "avares://IconManager/Data/Sources/FluentUISystemGlyphSources.json";

            // Currently only the FluentUISystem is supported
            using (var sourceStream = assets.Open(new Uri(sourceDataPath)))
            using (var reader = new StreamReader(sourceStream))
            {
                string jsonString = reader.ReadToEnd();
                var rawGlyphSources = JsonSerializer.Deserialize<string[]>(jsonString);

                if (rawGlyphSources != null)
                {
                    foreach (var entry in rawGlyphSources)
                    {
                        sources.Add(entry);
                    }
                }
            }

            lock (cacheMutex)
            {
                cachedFluentUISystemGlyphSources = sources;
            }

            return;
        }
    }
}
