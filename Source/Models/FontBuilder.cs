using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

        private static object directoryMutex = new object();

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

        /// <summary>
        /// Writes all scripts and assembles all files necessary to generate a new font using FontForge.
        /// An output folder will be created with a script that can be run to generate the font.
        /// </summary>
        /// <param name="mappings">The icon glyph mappings to generate the font with.</param>
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

            // Write the python script to build the actual font with FontForge
            using (MemoryStream pythonScript = this.BuildPythonFontScript(mappings, outputDirectory))
            {
                this.WriteStreamToFile(
                    pythonScript,
                    Path.Combine(outputDirectory, PythonScriptFileName));
            }

            // Open the output location for the end-user
            // This currently only works on Windows
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    Arguments = outputDirectory,
                    FileName = "explorer.exe"
                });
            }
            catch { }

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
            var copyright = $@"Copyright (c) {DateTime.Now.Year}, Unnamed";
            var familyName = @"Symbols";
            var fontName = @"Symbols";
            //var comments = @"Built using FontForge and SymbolIconManager";
            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;

            sb.AppendLine(@"import fontforge");
            sb.AppendLine();
            sb.AppendLine( @"# Create a new, empty font");
            sb.AppendLine( @"font = fontforge.font()");
            sb.AppendLine($@"font.copyright  = '{copyright}'");
            sb.AppendLine($@"font.familyname = '{familyName}'");
            sb.AppendLine($@"font.fontname   = '{fontName}'");
            sb.AppendLine();
            sb.AppendLine(@"# Each character's glyph is created and added to the font below.");
            sb.AppendLine(@"# Glyphs are created by automatically importing from an SVG source when possible.");
            sb.AppendLine(@"# FontForge uses the following metrics automatically when importing from SVG:");
            sb.AppendLine(@"#   - Assumes the SVG is 1000px-by-1000px and will scale an SVG of a different size");
            sb.AppendLine(@"#   - Sets the baseline at 200px from the bottom");
            sb.AppendLine(@"# The width of the glyph is not automatically set when automatically importing an SVG.");
            sb.AppendLine(@"# This is true even though the SVG width is scaled to 1000px.");
            sb.AppendLine(@"# To work-around this, the glyph width is set for all characters as well.");
            sb.AppendLine(@"#");
            sb.AppendLine(@"# Further information about scripting FontForge can be found at the below links:");
            sb.AppendLine(@"#   1. https://fontforge.org/docs/scripting/python.html");
            sb.AppendLine(@"#   2. https://fontforge.org/docs/scripting/python/fontforge.html#glyph");
            sb.AppendLine();

            //      Source: Name is ignored, use IconSet+UnicodePoint to lookup the glyph
            // Destination: IconSet is ignored, the only relevant data is Unicode Point and Name

            foreach (IconMapping mapping in mappings)
            {
                var possibleGlyphSources = GlyphRenderer.GetPossibleGlyphSources(
                    mapping.Source.IconSet,
                    mapping.Source.UnicodePoint);

                if (mapping.IsValidForFont &&
                    possibleGlyphSources.Contains(GlyphSource.RemoteSvgFile))
                {
                    var svgUrl = GlyphRenderer.GetGlyphSourceUrl(
                        mapping.Source.IconSet,
                        mapping.Source.UnicodePoint);

                    // Calculate the SVG name from the URL itself instead of the Icon name
                    // This ensures the name calculation is done only once inside the URL calculation
                    string svgName = Path.GetFileName(svgUrl?.LocalPath ?? string.Empty);

                    // Download the SVG image file
                    // This can be done totally async with no need to await
                    // The file is just being added to the file system for external use later
                    Task.Run(async () =>
                    {
                        if (svgUrl != null)
                        {
                            using (var stream = await GlyphRenderer.GetGlyphSourceStreamAsync(svgUrl!))
                            {
                                if (stream != null)
                                {
                                    var filePath = Path.Combine(
                                        outputDirectory,
                                        GlyphSubDirectoryName,
                                        svgName);

                                    using (var fileStream = File.OpenWrite(filePath))
                                    {
                                        stream.WriteTo(fileStream);
                                    }
                                }
                            }
                        }
                    });

                    if (string.IsNullOrWhiteSpace(mapping.Destination.Name) == false)
                    {
                        sb.AppendLine($@"# {mapping.Destination.Name}");
                    }

                    sb.AppendLine($@"glyph = font.createChar(0x{mapping.Destination.UnicodeHexString})");
                    sb.AppendLine($@"glyph.importOutlines('{GlyphSubDirectoryName}\{svgName}')");
                    sb.AppendLine($@"glyph.width = 1000");

                    // Only override the default FontForge name if one is provided
                    if (string.IsNullOrWhiteSpace(mapping.Destination.Name) == false)
                    {
                        sb.AppendLine($@"glyph.glyphname = '{mapping.Destination.Name}'");
                    }

                    sb.AppendLine();
                }
            }

            sb.AppendLine( @"# Export the newly created font");
            sb.AppendLine($@"font.generate('{OutputFontFileName}')");
            sb.AppendLine( @"font.close()");

            return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        private MemoryStream BuildWindowsScript()
        {
            var sb = new StringBuilder();

            sb.AppendLine(@"@ECHO OFF");
            sb.AppendLine(@"SET scriptPath=%cd%");
            sb.AppendLine();
            sb.AppendLine($@"""{FontForgeFilePath}"" -script ""%scriptPath%\{PythonScriptFileName}""");
            sb.AppendLine();
            sb.AppendLine($@"ECHO FontForge has finished building the {OutputFontFileName} font.");
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
    }
}
