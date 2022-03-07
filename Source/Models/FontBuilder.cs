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

        /// <summary>
        /// Authors the python script that is used by FontForge to build the font.
        /// </summary>
        /// <returns>The python script stream that builds the font.</returns>
        private MemoryStream BuildPythonFontScript(
            IconMappingList mappings,
            string outputDirectory,
            string fontFileName,
            Log buildLog)
        {
            var sb = new StringBuilder();
            var props = new FontProperties(); // May be passed in the future
            TextInfo textInfo = CultureInfo.InvariantCulture.TextInfo;

            // Most of the instructions and notes for building the font are embedded in the script itself.
            // However, it is also useful to mention two existing fonts that were used as a guideline.
            // Properties for these fonts are listed below:
            //
            // FluentSystemIcons-Regular.ttf
            //        Ascent: 500
            //       Descent: 0
            //       Em-Size: 500
            // Underline Pos: 5
            //        Height: 0
            //         Notes: Each glyph has a margin and isn't stretched to fill the entire bounding box.
            //                This makes symbols appear smaller in usage.
            //
            // Segoe Fluent Icons.ttf
            //        Ascent: 2048
            //       Descent: 0
            //       Em-Size: 2048
            // Underline Pos: -100
            //        Height: 50
            //         Notes: Almost every glyph is stretched to fill the entire bounding box.
            //                This makes symbols appear larger in usage.
            //
            // Also, according to FontForge, with the "FluentSystemIcons-Regular.ttf" font:
            //
            //    "The convention is that TrueType fonts should have an
            //     Em-Size which is a power of 2. But this font has a size of
            //     500. This is not an error, but you might consider altering
            //     the Em-Size with the Element->Font Info->General dialog."
            //
            // This means "Segoe Fluent Icons.ttf" is better authored.

            sb.AppendLine(@"import fontforge");
            sb.AppendLine(@"import psMat");
            sb.AppendLine();
            sb.AppendLine( @"# Create a new, empty font");
            sb.AppendLine( @"font = fontforge.font()");
            sb.AppendLine($@"font.copyright  = '{props.Copyright}'");
            sb.AppendLine($@"font.familyname = '{props.FamilyName}'");
            sb.AppendLine($@"font.fontname   = '{props.Name}'");
            sb.AppendLine($@"font.fullname   = '{props.Name}' # Name for humans");
            sb.AppendLine();
            sb.AppendLine(@"# Each character's glyph is created and added to the font below.");
            sb.AppendLine(@"# Glyphs are created by automatically importing from an SVG source when possible.");
            sb.AppendLine(@"# FontForge uses the following metrics automatically when importing from SVG:");
            sb.AppendLine(@"#   - Assumes the SVG is 1000-by-1000 EM and will scale an SVG of a different size");
            sb.AppendLine(@"#   - Sets the font ascent to 800 and descent to 200");
            sb.AppendLine(@"#   - Sets the baseline at 200 above the descent");
            sb.AppendLine(@"#   - Following 1000-by-1000 glyphs, the font EM-Size is set to 1000");
            sb.AppendLine(@"#");
            sb.AppendLine(@"# The width of each glyph is not automatically set when importing an SVG.");
            sb.AppendLine(@"# This is true even though the SVG width is scaled to 1000 EM.");
            sb.AppendLine(@"# To work-around this, the glyph width is set for all characters after importing SVGs.");
            sb.AppendLine(@"#");
            sb.AppendLine(@"# After each glyph is imported into the font, the font must be scaled correctly.");
            sb.AppendLine(@"# This means the final step is to move the baseline to the botton and then set the");
            sb.AppendLine(@"# Em-Size to a power of two matching Window's Symbols Fonts.");
            sb.AppendLine(@"# This is done by making the following adjustments:");
            sb.AppendLine(@"#   - Ascent changed to 1000");
            sb.AppendLine(@"#   - Descent changed to 0");
            sb.AppendLine(@"#   - Glyphs translated up by 200 to account for new baseline");
            sb.AppendLine(@"#   - Em-Size changed to 2048 (which then changes ascent to 2048)");
            sb.AppendLine(@"#     FontForge will automatically scale the glyphs to fit the new size.");
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

            sb.AppendLine(@"# Adjust each glyph's width to match the import default 1000x1000.");
            sb.AppendLine(@"for glyph in font.glyphs():");
            sb.AppendLine(@"    glyph.width = 1000");
            sb.AppendLine();

            sb.AppendLine(@"# Move the baseline from the default 200 to 0.");
            sb.AppendLine(@"# This is done indirectly by setting both the ascent and descent.");
            sb.AppendLine(@"# Remember FontForge will by default import with size 1000x1000 and 200 baseline.");
            sb.AppendLine(@"font.ascent = 1000 # From 800");
            sb.AppendLine(@"font.descent = 0   # From 200");
            sb.AppendLine();

            sb.AppendLine(@"# Translate each glyph's position after moving the baseline.");
            sb.AppendLine(@"translate_matrix = psMat.translate(0, 200)");
            sb.AppendLine();
            sb.AppendLine(@"for glyph in font.glyphs():");
            sb.AppendLine(@"    glyph.transform(translate_matrix)");
            sb.AppendLine();

            sb.AppendLine(@"# Change the Em-Size of the font to match other symbol fonts.");
            sb.AppendLine(@"# The convention is that TrueType fonts should have an Em-Size which is a power of 2.");
            sb.AppendLine(@"# Setting this will scale the entire font (each glyph) to the new size.");
            sb.AppendLine(@"font.em = 2048 ");
            sb.AppendLine();

            /* This code may be enabled in the future
            sb.AppendLine(@"# Set remaining font properties");
            sb.AppendLine(@"font.upos = -100 # Underline position");
            sb.AppendLine();
            */

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

                string? directoryName = Path.GetDirectoryName(filePath);
                if (directoryName != null &&
                    Directory.Exists(directoryName) == false)
                {
                    Directory.CreateDirectory(directoryName);
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
