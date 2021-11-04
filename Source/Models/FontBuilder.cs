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
        private const string MacOSScriptFileName    = @"build_macOS.command";
        private const string WindowsScriptFileName  = @"build_win.bat";
        private const string OutputFontFileName     = @"output_font.ttf";

        // File paths
        private const string DefaultFontForgeFilePathWindows = @"C:\Program Files (x86)\FontForgeBuilds\run_fontforge.exe";
        private const string DefaultFontForgeFilePathMacOS   = @"/Applications/FontForge.app";

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
        /// <param name="fontFileName">The name of the output font file.
        /// This must end in extension '.ttf'.</param>
        public void BuildFont(
            IconMappingList mappings,
            string fontFileName = OutputFontFileName)
        {
            var buildLog = new Log();

            if (mappings.Count == 0)
            {
                return;
            }

            // Force .ttf extension
            if (Path.GetExtension(fontFileName).ToLowerInvariant() != ".ttf")
            {
                fontFileName = Path.GetFileNameWithoutExtension(fontFileName);
                fontFileName = fontFileName + ".ttf";
            }

            // Create the directories
            string outputDirectory = this.CreateNewBuildDirectory();
            Directory.CreateDirectory(Path.Combine(outputDirectory, GlyphSubDirectoryName));

            // Write the platform-dependent build scripts
            using (MemoryStream macScript = this.BuildMacOSScript(fontFileName))
            {
                this.WriteStreamToFile(
                    macScript,
                    Path.Combine(outputDirectory, MacOSScriptFileName));
            }

            using (MemoryStream winScript = this.BuildWindowsScript(fontFileName))
            {
                this.WriteStreamToFile(
                    winScript,
                    Path.Combine(outputDirectory, WindowsScriptFileName));
            }

            // Write the python script to build the actual font with FontForge
            using (MemoryStream pythonScript = this.BuildPythonFontScript(mappings, outputDirectory, fontFileName, buildLog))
            {
                this.WriteStreamToFile(
                    pythonScript,
                    Path.Combine(outputDirectory, PythonScriptFileName));
            }

            // Write the build log
            if (buildLog.IsEmpty == false)
            {
                buildLog.Export(Path.Combine(outputDirectory, "log.txt"));
            }

            // Open the output location for the end-user
            // This currently only works on Windows
            try
            {
                if (System.OperatingSystem.IsWindows())
                {
                    Process.Start(new ProcessStartInfo
                    {
                        Arguments = outputDirectory,
                        FileName = "explorer.exe"
                    });
                }
                else if (System.OperatingSystem.IsMacOS())
                {
                    Process.Start(new ProcessStartInfo
                    {
                        Arguments = $"-R {outputDirectory}",
                        FileName = "open"
                    });
                }
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
            string outputDirectory,
            string fontFileName,
            Log buildLog)
        {
            var sb = new StringBuilder();
            var props = new FontProperties(); // May be passed in the future
            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;

            sb.AppendLine(@"import fontforge");
            sb.AppendLine();
            sb.AppendLine( @"# Create a new, empty font");
            sb.AppendLine( @"font = fontforge.font()");
            sb.AppendLine($@"font.copyright  = '{props.Copyright}'");
            sb.AppendLine($@"font.familyname = '{props.FamilyName}'");
            sb.AppendLine($@"font.fontname   = '{props.Name}'");
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

                    if (svgUrl == null)
                    {
                        buildLog.Error($"Missing SVG source URL, mapping skipped src=0x{mapping.Source.UnicodeHexString}, dst=0x{mapping.Destination.UnicodeHexString} ({mapping.Source.Name})");
                        continue; // Fatal error
                    }

                    // Calculate the initial SVG file name from the URL itself (instead of with Icon name)
                    // This ensures the name calculation is done only once inside the URL calculation
                    string svgFileName = Path.GetFileName(svgUrl?.LocalPath ?? string.Empty);

                    // Transform the SVG file name to:
                    //  1. Remove illegal Python characters such as '-'
                    //  2. Add the icon set as a prefix ensuring file names are unique
                    svgFileName = svgFileName.Replace('-', '_');
                    svgFileName = mapping.Source.IconSet.ToString() + "_" + svgFileName;

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
                                        svgFileName);

                                    using (var fileStream = File.OpenWrite(filePath))
                                    {
                                        stream.WriteTo(fileStream);
                                    }
                                }
                                else
                                {
                                    buildLog.Error($"Missing source glyph data for {svgUrl}");
                                }
                            }
                        }
                    });

                    if (string.IsNullOrWhiteSpace(mapping.Destination.Name) == false)
                    {
                        sb.AppendLine($@"# {mapping.Destination.Name}");
                    }

                    sb.AppendLine($@"glyph = font.createChar(0x{mapping.Destination.UnicodeHexString})");
                    // TODO: '\' usage here only works on Windows, need to use '/' or path methods
                    sb.AppendLine($@"glyph.importOutlines('{GlyphSubDirectoryName}\{svgFileName}')");
                    sb.AppendLine($@"glyph.width = 1000");

                    // Only override the default FontForge name if one is provided
                    if (string.IsNullOrWhiteSpace(mapping.Destination.Name) == false)
                    {
                        sb.AppendLine($@"glyph.glyphname = '{mapping.Destination.Name}'");
                    }

                    sb.AppendLine();
                }
                else
                {
                    buildLog.Error($"Invalid mapping skipped src=0x{mapping.Source.UnicodeHexString}, dst=0x{mapping.Destination.UnicodeHexString} ({mapping.Destination.Name})");
                    continue; // Fatal error
                }
            }

            sb.AppendLine( @"# Export the newly created font");
            sb.AppendLine($@"font.generate('{fontFileName}')");
            sb.AppendLine( @"font.close()");

            return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        private MemoryStream BuildMacOSScript(string fontFileName)
        {
            var sb = new StringBuilder();
            
            // This script does not currently work
            //  1. It requires special permissions to run on modern macOS versions
            //  2. FontForge has a bug on macOS where attempting to import an SVG outline gives
            //     "I'm sorry this file is too complex for me to understand (or is erroneous)"
            //     This can be verified using [File]->[Execute Script...] in FontForge directly
            //     The same Python that works on Windows does not work on macOS
            //  3. The Python script needs to be modified to use Unix paths '/' instead of '\'
            
            sb.AppendLine(@"cd -- ""$(dirname ""$BASH_SOURCE"")""");
            sb.AppendLine();
            sb.AppendLine($@"open -a {DefaultFontForgeFilePathMacOS} --args -script ""$cd/{PythonScriptFileName}""");
            sb.AppendLine();
            sb.AppendLine($@"echo ""FontForge has finished building the {fontFileName} font.""");

            return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        private MemoryStream BuildWindowsScript(string fontFileName)
        {
            var sb = new StringBuilder();

            sb.AppendLine(@"@ECHO OFF");
            sb.AppendLine(@"SET scriptPath=%cd%");
            sb.AppendLine();
            sb.AppendLine($@"""{DefaultFontForgeFilePathWindows}"" -script ""%scriptPath%\{PythonScriptFileName}""");
            sb.AppendLine();
            sb.AppendLine($@"ECHO FontForge has finished building the {fontFileName} font.");
            sb.AppendLine();
            sb.AppendLine(@"PAUSE");

            return new MemoryStream(Encoding.UTF8.GetBytes(sb.ToString()));
        }

        /// <summary>
        /// Writes the data stream to the given file path.
        /// Existing files will be overwritten, directories will automatically be created.
        /// </summary>
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

        /***************************************************************************************
         *
         * Classes
         *
         ***************************************************************************************/

        /// <summary>
        /// Contains properties of a font.
        /// </summary>
        public class FontProperties
        {
            public FontProperties()
            {
                this.Comments   = "Released under the terms of the MIT license.";
                this.Copyright  = $@"Copyright (c) {DateTime.Now.Year}, SymbolIconManager";
                this.FamilyName = @"Symbols";
                this.Name       = @"Symbols";
            }

            /// <summary>
            /// Gets or sets any comments of the font.
            /// </summary>
            public string Comments { get; set; }

            /// <summary>
            /// Gets or sets the copyright information of the font.
            /// </summary>
            public string Copyright { get; set; }

            /// <summary>
            /// Gets or sets the name of the font's family.
            /// </summary>
            public string FamilyName { get; set; }

            /// <summary>
            /// Gets or sets the name of the font.
            /// </summary>
            public string Name { get; set; }
        }
    }
}
