using Avalonia.Controls;
using System.IO;
using System.Threading.Tasks;

namespace IconManager
{
    public partial class FontsView : UserControl
    {
        private const string FluentAvaloniaMappingsPath = "avares://IconManager/Data/Mappings/FluentAvalonia.json";

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public FontsView()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private async Task<string> GetSaveFilePath()
        {
            var dialog = new SaveFileDialog();
            var path = await dialog.ShowAsync(App.MainWindow);

            if (path != null)
            {
                if (File.Exists(path))
                {
                    // Delete the existing file, it will be replaced
                    File.Delete(path);
                }

                if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                }
            }

            return path ?? string.Empty;
        }

        private IconMappingList GetWinSymbols1Mappings()
        {
            var log = new Log();
            var segoeV3Mappings = IconMappingList.Load(IconSet.SegoeFluent);
            var segoeV1toV2Mappings = IconMappingList.Load(
                IconSet.SegoeUISymbol,
                IconSet.SegoeMDL2Assets);

            // WinSymbols1 is an equivalent font for SegoeUISymbol icons.
            //
            // This font does not have its own mapping file suitable for building a font.
            // Instead, the build process translates to SegoeMDL2Assets Unicode points and
            // then uses the same, backwards-compatible mapping file for SegoeFluent
            // ('SegoeFluent.json').
            //
            // This is possible because:
            //  1. SegoeMDL2Assets contains all the icons that were in the older
            //     SegoeUISymbol just at a different Unicode point (and name).
            //     A mapping file is provided for this translation.
            //  2. Microsoft has retained backwards compatibility in each subsequent
            //     symbol font (even for deprecated glyphs). This means a mapping file
            //     for WinSymbols2 or WinSymbols3 will also contain the Unicode points
            //     and mappings used to construct equivalent, earlier fonts.
            //
            // Translation from SegoeUISymbol->SegoeMDL2Assets is accomplished using the
            // mapping file 'SegoeUISymbolToSegoeMDL2Assets.json'. This is an
            // `Icon Set Mapping' not a 'Font Mapping' and cannot be used to construct a font.
            //
            // Warning: Doing this means the graphical design language is not maintained.
            // SegoeUISymbol/SegoeMDL2Assets glyphs visually differ from SegoeFluent.
            // This is considered acceptable for now.

            // Initialize a brand-new, empty mapping list that will eventually be used to build the font
            var segoeV1Mappings = IconMappingList.InitNewMappings(IconSet.SegoeUISymbol);

            // Add source information for each mapping
            foreach (IconMapping mapping in segoeV1Mappings)
            {
                Icon? translatedDestination = null;

                // Translate from SegoeUISymbol (V1) to SegoeMDL2Assets (V2)
                var translationMatches = segoeV1toV2Mappings.FindBySourceUnicode(mapping.Destination.UnicodePoint);

                // Only allow a translation if there was a single match
                // This is a failsafe and should never be encountered
                if (translationMatches.Count == 1)
                {
                    translatedDestination = translationMatches[0].Destination.Clone();
                }
                else
                {
                    log.Error($"Unable to translate 0x{mapping.Destination.UnicodeHexString} ({mapping.Destination.Name}) into SegoeMDL2Assets");
                }

                // Look for a mapping to the translated icon in SegoeFluent (V3) mappings
                // Remember, this is backwards compatible with SegoeMDL2Assets (V2)
                if (translatedDestination != null)
                {
                    var matches = segoeV3Mappings.FindByDestinationUnicode(translatedDestination.UnicodePoint);

                    // Again, as a failsafe, only continue if exactly one match was found
                    if (matches.Count == 1 &&
                        matches[0].Source.IsValidForSource)
                    {
                        mapping.Source = matches[0].Source.Clone();
                    }
                    else
                    {
                        // This is not an error since many mappings do not exist yet
                        log.Message($"No mapping source found for 0x{mapping.Destination.UnicodeHexString} ({mapping.Destination.Name})");
                    }
                }
            }

            return segoeV1Mappings;
        }

        private IconMappingList GetWinSymbols2Mappings(bool includeLegacyGlyphs = true)
        {
            var log = new Log();
            var segoeV3Mappings = IconMappingList.Load(IconSet.SegoeFluent);

            // WinSymbols2 is an equivalent font for SegoeMDL2Assets icons.
            //
            // This font does not have its own mapping file suitable for building a font.
            // Instead, the build process uses the same, backwards-compatible mapping file
            // for SegoeFluent ('SegoeFluent.json').
            //
            // This is possible because Microsoft has retained backwards compatibility in
            // each subsequent symbol font (even for deprecated glyphs). This means a
            // mapping file for WinSymbols3 will also contain the Unicode points and
            // mappings used to construct equivalent, earlier fonts.
            //
            // Warning: Doing this means the graphical design language is not maintained.
            // SegoeMDL2Assets glyphs visually differ from SegoeFluent.
            // This is considered acceptable for now.
            //
            // In addition, the initial/default mapping file does not include legacy
            // (obsolete) code-points from earlier SegoeUISymbol (V1) which are still used in
            // the Symbols enum and some in-box controls.
            //
            // In order to ensure complete compatibility with the SegoeMDL2Assets font provided
            // by Microsoft, and ensure full backwards-compatibility, mappings are also
            // automatically added for earlier SegoeUISymbol (V1) glyphs.

            // Initialize a brand-new, empty mapping list that will eventually be used to build the font
            var segoeV2Mappings = IconMappingList.InitNewMappings(IconSet.SegoeMDL2Assets);

            // Add source information for each mapping
            foreach (IconMapping mapping in segoeV2Mappings)
            {
                var matches = segoeV3Mappings.FindByDestinationUnicode(mapping.Destination.UnicodePoint);

                // As a failsafe, only continue if exactly one match was found
                if (matches.Count == 1 &&
                    matches[0].Source.IsValidForSource)
                {
                    mapping.Source = matches[0].Source.Clone();
                }
                else
                {
                    // This is not an error since many mappings do not exist yet
                    log.Message($"No mapping source found for 0x{mapping.Destination.UnicodeHexString} ({mapping.Destination.Name})");
                }
            }

            if (includeLegacyGlyphs)
            {
                var segoeV1Mappings = this.GetWinSymbols1Mappings();

                // Merge order ensures V2 overwrites any conflict with V1
                var finalSegoeV2Mappings = segoeV1Mappings;
                segoeV2Mappings.MergeInto(finalSegoeV2Mappings);

                return finalSegoeV2Mappings;
            }
            else
            {
                return segoeV2Mappings;
            }
        }

        private IconMappingList GetWinSymbols3Mappings(bool includeLegacyGlyphs = true)
        {
            // WinSymbols3 is an equivalent font for SegoeFluent icons.
            //
            // This font has it's own mapping file 'SegoeFluent.json'.
            // However, this mapping file does not include legacy (obsolete) code-points
            // from earlier SegoeUISymbol (V1) and SegoeMDL2Assets (V2) which are still used
            // in the Symbols enum and some in-box controls.
            //
            //   https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-fluent-icons-font#icon-list
            //   Glyphs with prefixes ranging from E0- to E5- (e.g. E001, E5B1) are
            //   currently marked as legacy and are therefore deprecated.
            //
            // This mapping file DOES include the Unicode points starting at 0xE700
            // and later which makes it a drop-in replacement for SegoeMDL2Assets (V2) as well
            // (the glyph names are the same).
            //
            // In order to ensure complete compatibility with the SegoeFluent font provided
            // by Microsoft, and ensure full backwards-compatibility, mappings are also
            // automatically added for earlier SegoeUISymbol (V1) glyphs.

            var segoeV3Mappings = IconMappingList.Load(IconSet.SegoeFluent);

            if (includeLegacyGlyphs)
            {
                var segoeV1Mappings = this.GetWinSymbols1Mappings();

                // Merge order ensures V3 overwrites any conflict with V1
                var finalSegoeV3Mappings = segoeV1Mappings;
                segoeV3Mappings.MergeInto(finalSegoeV3Mappings);

                return finalSegoeV3Mappings;
            }
            else
            {
                return segoeV3Mappings;
            }
        }

        private void BuildWinSymbols1Font()
        {
            var fontBuilder = new FontBuilder();
            fontBuilder.BuildFont(this.GetWinSymbols1Mappings(), "WinSymbols1.ttf");

            return;
        }

        private void BuildWinSymbols2Font()
        {
            var fontBuilder = new FontBuilder();
            fontBuilder.BuildFont(this.GetWinSymbols2Mappings(), "WinSymbols2.ttf");

            return;
        }

        private void BuildWinSymbols3Font()
        {
            var fontBuilder = new FontBuilder();
            fontBuilder.BuildFont(this.GetWinSymbols3Mappings(), "WinSymbols3.ttf");

            return;
        }

        private void BuildFluentAvaloniaFont()
        {
            var fluentAvaloniaMappings = IconMappingList.Load(FluentAvaloniaMappingsPath);
            var segoeFluentMappings = IconMappingList.Load(IconSet.SegoeFluent);

            // The Fluent Avalonia font must meet 3 requirements:
            //  1. Include all entries from the WinUI Symbol enum (even if no glyph is available)
            //     These icons are the base for the FluentAvalonia symbols enum.
            //  2. Include new, custom icons that exist in the FluentUISystem but not SegoeFluent.
            //     The custom icons are also part of the FluentAvalonia symbols enum.
            //  3. Be a drop-in replacement for the SegoeFluent font (WinSymbols3)
            //
            // The first two requirements are taken care of by the FluentAvalonia.json mapping file itself.
            // This file is specially built to ensure it is compatible with the WinUI symbol enum.
            // It also defines all custom icons for FluentAvalonia itself.
            //
            // To meet the last requirement, the SegoeFluent mappings must be merged in before the
            // font is built. These will take precedence over any overlapping entries in FluentAvalonia.
            segoeFluentMappings.MergeInto(fluentAvaloniaMappings);

            var fontBuilder = new FontBuilder();
            fontBuilder.BuildFont(fluentAvaloniaMappings, "FluentAvalonia.ttf");

            return;
        }

        private async void RebuildFluentAvaloniaMappings()
        {
            string path = await this.GetSaveFilePath();

            if (string.IsNullOrEmpty(path) == false)
            {
                var fluentAvalonia = new Specialized.FluentAvalonia();
                var mappings = fluentAvalonia.RebuildMappings(IconMappingList.Load(FluentAvaloniaMappingsPath));

                using (var fileStream = File.OpenWrite(path))
                {
                    IconMappingList.Save(mappings, fileStream);
                }

                if (this.IncludeFluentAvaloniaEnumCheckBox.IsChecked ?? false)
                {
                    using (var fileStream = File.OpenWrite(Path.Combine(Path.GetDirectoryName(path)!, "enum.cs")))
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        writer.Write(fluentAvalonia.GenerateSymbolEnumSource(mappings));
                    }
                }
            }

            return;
        }

        private void BuildFluentUISystemRegularFont()
        {
            // This is an identity mapping
            var mappings = IconMappingList.InitNewMappings(IconSet.FluentUISystemRegular);
            foreach (IconMapping mapping in mappings)
            {
                mapping.Source = mapping.Destination.Clone();
            }

            var fontBuilder = new FontBuilder();
            fontBuilder.BuildFont(mappings, "FluentUISystemRegular.ttf");

            return;
        }

        private void BuildFluentUISystemFilledFont()
        {
            // This is an identity mapping
            var mappings = IconMappingList.InitNewMappings(IconSet.FluentUISystemFilled);
            foreach (IconMapping mapping in mappings)
            {
                mapping.Source = mapping.Destination.Clone();
            }

            var fontBuilder = new FontBuilder();
            fontBuilder.BuildFont(mappings, "FluentUISystemFilled.ttf");

            return;
        }

        /***************************************************************************************
         *
         * Event Handling
         *
         ***************************************************************************************/

        // Section available
    }
}
