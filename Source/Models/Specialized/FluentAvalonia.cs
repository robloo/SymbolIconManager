﻿using System;
using System.Text;

namespace IconManager.Specialized
{
    /// <summary>
    /// A highly specialized class containing methods to work with Fluent Avalonia.
    /// </summary>
    /// <remarks>
    /// This class is not general purpose and should only be used for Fluent Avalonia.
    /// See: https://github.com/amwx/FluentAvalonia.
    /// </remarks>
    public class FluentAvalonia
    {
        /// <summary>
        /// Generates Symbol enum values for the given mappings that can be copy-pasted into source code.
        /// </summary>
        public string GenerateSymbolEnumSource(IconMappingList mappings)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IconMapping mapping in mappings)
            {
                if (mapping.Source.IsValidForSource == false)
                {
                    sb.AppendLine(@"[Obsolete(""Added for compatibility with WinUI only. No glyph exists for this symbol."")]");
                }

                sb.AppendLine($"{mapping.Destination.Name} = 0x{mapping.Destination.UnicodeHexString},");
            }

            return sb.ToString();
        }

        /// <summary>
        /// Rebuilds FluentAvalonia mappings using the given mappings as a base.
        /// This will ensure rebuild mappings match with latest SegoeFluent and the WinUI Symbol enum.
        /// </summary>
        /// <param name="existingMappings">Existing FluentAvalonia.json mappings to rebuild.</param>
        /// <returns>The rebuilt mappings.</returns>
        public IconMappingList RebuildMappings(IconMappingList existingMappings)
        {
            IconMappingList mappings = existingMappings;

            // Initialize the starting value of all new symbols Unicode points here.
            // New symbols are those that do not exist by name in either SegoeFluent or the
            // WinUI Symbol enum.
            // The starting Unicode point here must be within range:
            //   U+F0000..U+FFFFD (Supplementary Private Use Area-A)
            // This ensures it is outside the range of values used by SegoeFluent/SegoeMDL2Assets.
            // Additionally, it must be high enough to almost certainly never overlay with
            // the FluentUISystem (currently ending at 0x10--- within ).
            uint newUnicodePointStart = 0xF8000;
            uint nextAvailableUnicodePoint = newUnicodePointStart;

            // Load the SegoeFluent mappings
            var segoeFluentMappings = IconMappingList.Load(IconSet.SegoeFluent);
            var segoeV1toV2 = IconMappingList.Load(IconSet.SegoeUISymbol, IconSet.SegoeMDL2Assets);

            // Ensure latest icons are used
            mappings.UpdateDeprecatedIcons();

            // Raw sources use the 'SegoeFluent' font is some cases
            // This cannot be used to construct a font as it doesn't have SVG sources by itself
            // Therefore, translate all 'SegoeFluent' to the corresponding mapped source glyph
            foreach (IconMapping mapping in mappings)
            {
                if (mapping.Source.IconSet == IconSet.SegoeFluent)
                {
                    var matchingSegoeFluentMappings = segoeFluentMappings.FindByDestinationUnicode(mapping.Source.UnicodePoint);

                    if (matchingSegoeFluentMappings.Count != 1)
                    {
                        throw new Exception("Invalid SegoeFluent Unicode point detected.");
                    }
                    else
                    {
                        mapping.Source = matchingSegoeFluentMappings[0].Source.Clone();
                        // Update other properties?
                    }
                }
            }

            // Add all entries from the WinUI symbols enum
            // Any matching Fluent Avalonia entries will be overwritten
            // The Symbol enum is considered primary for interoperability
            var enumValues = Enum.GetValues(typeof(WinUISymbols.Symbol));
            foreach (WinUISymbols.Symbol value in enumValues)
            {
                uint unicode = (uint)value;

                // Unicode points less than 0xE700 only exist in the original v1 Segoe UI Symbol font
                // These Unicode points are no longer valid in Segoe MDL2 Assets or Segoe Fluent Icons.
                // Therefore, the Unicode point must be translated to Segoe MDL2 Assets in this case
                if (unicode < 0xE700)
                {
                    var matches = segoeV1toV2.FindBySourceUnicode(unicode);

                    if (matches.Count == 1)
                    {
                        // Translate to Segoe MDL2 Assets which is compatible with V3 Segoe Fluent
                        unicode = matches[0].Destination.UnicodePoint;
                    }
                    else
                    {
                        throw new Exception("Unable to translate Segoe UI Symbol (v1) icon to Segoe MDL2 Assets (v2) equivalent.");
                    }
                }

                // Unicode matching is much more exact than a name search
                var matchingSegoeFluentMappings = segoeFluentMappings.FindByDestinationUnicode(unicode);

                if (matchingSegoeFluentMappings.Count == 1)
                {
                    // Create the exact mapping for this Symbol enum value
                    // This is primary over any mapping defined in the raw Fluent Avalonia sources
                    var symbolEnumMapping = new IconMapping()
                    {
                        Source      = matchingSegoeFluentMappings[0].Source,
                        Destination = new Icon()
                        {
                            IconSet      = IconSet.Undefined, // Fluent Avalonia is not a defined icon set
                            UnicodePoint = (uint)value,       // Do NOT use the translated Unicode value, keep original
                            Name         = value.ToString()
                        },
                        GlyphMatchQuality    = matchingSegoeFluentMappings[0].GlyphMatchQuality,
                        MetaphorMatchQuality = matchingSegoeFluentMappings[0].MetaphorMatchQuality,
                        IsPlaceholder        = matchingSegoeFluentMappings[0].IsPlaceholder,
                        Comments             = "WinUI Symbol. " + matchingSegoeFluentMappings[0].Comments
                    };

                    // Check for an existing mapping already (that should be overwritten)
                    IconMapping? existingMapping = null;
                    foreach (IconMapping mapping in mappings)
                    {
                        // Not checking by Unicode point on purpose. Names should be unique as the output will also be an enum
                        if (string.Equals(mapping.Destination.Name, symbolEnumMapping.Destination.Name, StringComparison.OrdinalIgnoreCase))
                        {
                            existingMapping = mapping;
                            break;
                        }
                    }

                    if (existingMapping != null)
                    {
                        existingMapping.Destination = symbolEnumMapping.Destination;
                        existingMapping.Source      = symbolEnumMapping.Source;
                    }
                    else
                    {
                        mappings.Add(symbolEnumMapping);
                    }
                }
                else
                {
                    if (value == WinUISymbols.Symbol.Placeholder)
                    {
                        // The Placeholder symbol 0xE18A has no equivalent in Segoe MDL2 or Segoe Fluent
                        // It is the only special case here.
                    }
                    else
                    {
                        // Should never get here, all other values should be defined
                        throw new Exception("Invalid Symbol enum icon detected.");
                    }
                }
            }

            // Sort all mappings alphabetically by destination name for easier readability of the JSON file
            // This is also important to do before new Unicode points are assigned so the Unicode points follow some order
            mappings.SortByDestinationName();

            // Assign new Unicode points for those with missing values
            foreach (IconMapping mapping in mappings)
            {
                if (mapping.Destination.UnicodePoint == 0)
                {
                    mapping.Destination.UnicodePoint = nextAvailableUnicodePoint;
                    nextAvailableUnicodePoint++;
                }
            }

            // Normalize all FluentUISystem sources to use size 20 where possible
            mappings = FluentUISystem.ConvertToSize(mappings, FluentUISystem.IconSize.Size20);
            mappings.Reprocess();

            // Double check: ensure that all Symbol enum values are in the mapping list
            foreach (WinUISymbols.Symbol value in enumValues)
            {
                bool existsByName = false;
                bool existsByUnicode = false;

                foreach (IconMapping mapping in mappings)
                {
                    if (existsByName == false &&
                        string.Equals(mapping.Destination.Name, value.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        existsByName = true;
                    }

                    if (existsByUnicode == false &&
                        mapping.Destination.UnicodePoint == (uint)value)
                    {
                        existsByUnicode = true;
                    }

                    if (existsByName && existsByUnicode)
                    {
                        break;
                    }
                }

                if (existsByName == false || existsByUnicode == false)
                {
                    throw new Exception("Missing Symbol enum mapping.");
                }
            }

            // Double check: Ensure that each destination icon name and Unicode point only occurs one time
            for (int i = 0; i < mappings.Count; i++)
            {
                for (int j = i + 1; j < mappings.Count; j++)
                {
                    if (string.Equals(
                        mappings[i].Destination.Name.Trim(),
                        mappings[j].Destination.Name.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                    {
                        throw new Exception("Duplicate destination name detected.");
                    }

                    if (mappings[i].Destination.UnicodePoint == mappings[j].Destination.UnicodePoint)
                    {
                        throw new Exception("Duplicate destination Unicode point detected.");
                    }
                }
            }

            // Double check: ensure all source icon sets have a real glyph source that can be used for font construction
            foreach (IconMapping mapping in mappings)
            {
                var possibleGlyphSources = GlyphRenderer.GetPossibleGlyphSources(
                    mapping.Source.IconSet,
                    mapping.Source.UnicodePoint);

                if (mapping.IsValidForFont == false ||
                    possibleGlyphSources.Contains(GlyphSource.RemoteSvgFile) == false)
                {
                    if (mapping.Source.IconSet == IconSet.Undefined)
                    {
                        // Allow for now
                    }
                    else
                    {
                        throw new Exception("Invalid source for building a font.");
                    }
                }
            }

            return mappings;
        }

        /// <summary>
        /// Initializes a new list of mappings to create a specialized Fluent Avalonia font.
        /// This font is inter-operable with SegoeFluent and the Symbols enum in WinUI.
        /// </summary>
        /// <remarks>
        /// This should really never be used again and serves only to document the initial build
        /// of the mapping file. It can also serve as an example for initializing future complicated
        /// mappings.
        /// 
        /// To make any changes, the mapping file itself should be edited at Mappings/FluentAvalonia.json.
        /// </remarks>
        /// <returns>A new icon mapping list.</returns>
        [Obsolete("This is out of date. Update using the FluentAvalonia.json mapping file.")]
        public IconMappingList InitNewMappings()
        {
            return this.RebuildMappings(this.LoadInitialRawSource());
        }

        [Obsolete("This is out of date. Update using the FluentAvalonia.json mapping file.")]
        private IconMappingList LoadInitialRawSource()
        {
            IconMappingList mappings = new IconMappingList();

            // Load the Fluent Avalonia raw sources
            // This contains all symbols specially added in addition to some that overlap with WinUI
            for (int i = 0; i < rawSource.GetLength(0); i++)
            {
                string name = rawSource[i, 0];
                string iconSrc = rawSource[i, 1];
                var iconSet = (IconSet)Enum.Parse(typeof(IconSet), iconSrc.Split(":")[0]);
                var unicode = Convert.ToUInt32(iconSrc.Split(":")[1].Substring(2), 16);

                var newMapping = new IconMapping()
                {
                    Source = new Icon()
                    {
                        IconSet      = iconSet,
                        UnicodePoint = unicode,
                    },
                    Destination = new Icon()
                    {
                        IconSet = IconSet.Undefined, // Fluent Avalonia is not a defined icon set
                        Name    = name,
                    },
                    // Unless the mapping is part of the Symbol enum (handled next),
                    // these mappings are additions derived from the Fluent UI System
                    // Therefore, they are an exact mapping.
                    GlyphMatchQuality    = MatchQuality.Exact,
                    MetaphorMatchQuality = MatchQuality.Exact,
                    IsPlaceholder        = false,
                    Comments             = string.Empty
                };

                mappings.Add(newMapping);
            }

            // Remove any deprecated icons (this list is known out of date)
            mappings.UpdateDeprecatedIcons();

            return mappings;
        }

        [Obsolete("This is out of date. Update using the FluentAvalonia.json mapping file.")]
        private static string[,] rawSource = new string[,]
        {
            {"Add", "SegoeFluent:0xE710"},
            {"Alert", "FluentUISystemRegular:0xF116"},
            {"AlertOff", "FluentUISystemRegular:0xF11A"},
            {"AlertOn", "FluentUISystemRegular:0xF11B"},
            {"AlertSnooze", "FluentUISystemRegular:0xF11D"},
            {"AlertUrgent", "FluentUISystemRegular:0xF11F"},
            {"Attach", "SegoeFluent:0xE723"},
            {"AlignCenter", "SegoeFluent:0xE8E3"},
            {"AlignDistributed", "FluentUISystemRegular:0xF79C"},
            {"AlignJustified", "FluentUISystemRegular:0xF79E"},
            {"AlignLeft", "SegoeFluent:0xE8E4"},
            {"AlignRight", "SegoeFluent:0xE8E2"},
            {"Back", "SegoeFluent:0xE72B"},
            {"Bookmark", "FluentUISystemRegular:0xF1F7"},
            {"Bold", "SegoeFluent:0xE8DD"},
            {"BulletList", "SegoeFluent:0xE8FD"},
            {"Calculator", "SegoeFluent:0xE8EF"},
            {"CalendarDay", "SegoeFluent:0xE8BF"},
            {"CalendarEmpty", "FluentUISystemRegular:0xF228"},
            {"CalendarMonth", "FluentUISystemRegular:0xF22C"},
            {"CalendarReply", "SegoeFluent:0xE8F5"},
            {"CalendarSync", "FluentUISystemRegular:0xF23A"},
            {"CalendarToday", "SegoeFluent:0xE8D1"},
            {"Camera", "SegoeFluent:0xE722"},
            {"Checkmark", "SegoeFluent:0xE73E"},
            {"ChevronDown", "SegoeFluent:0xE70D"},
            {"ChevronLeft", "SegoeFluent:0xE76B"},
            {"ChevronRight", "SegoeFluent:0xE76C"},
            {"ChevronUp", "SegoeFluent:0xE70E"},
            {"Clipboard", "SegoeFluent:0xE77F"},
            {"ClipboardCode", "FluentUISystemRegular:0xF2CD"},
            {"Clock", "SegoeFluent:0xED5A"},
            {"Cloud", "SegoeFluent:0xE753"},
            {"CloudBackup", "FluentUISystemRegular:0xF2E8"},
            {"CloudDownload", "SegoeFluent:0xEBD3"},
            {"CloudOff", "FluentUISystemRegular:0xF2EB"},
            {"CloudOffline", "FluentUISystemRegular:0xF2EC"},
            {"CloudSync", "FluentUISystemRegular:0xFB7E"},
            {"CloudSyncComplete", "FluentUISystemRegular:0xF2EE"},
            {"Code", "FluentUISystemRegular:0xF2F0"},
            {"ColorBackground", "FluentUISystemRegular:0xF2F8"},
            {"ColorFill", "FluentUISystemRegular:0xF2FA"},
            {"ColorLine", "FluentUISystemRegular:0xF2FC"},
            {"CommentAdd", "FluentUISystemRegular:0xF301"},
            {"CommentMention", "FluentUISystemRegular:0xF305"},
            {"CommentMultiple", "FluentUISystemRegular:0xF308"},
            {"ContactInfo", "FluentUISystemRegular:0xF320"},
            {"Copy", "SegoeFluent:0xE8C8"},
            {"Crop", "SegoeFluent:0xE7A8"},
            {"Cut", "SegoeFluent:0xE8C6"},
            {"ClearFormatting", "FluentUISystemRegular:0xF7BD"},
            {"ClosedCaption", "SegoeFluent:0xE7F0"},
            {"Comment", "SegoeFluent:0xE90A"},
            {"Calendar", "SegoeFluent:0xE787"},
            {"Download", "SegoeFluent:0xE896"},
            {"DarkTheme", "FluentUISystemRegular:0xF33C"},
            {"Delete", "SegoeFluent:0xE74D"},
            {"Directions", "SegoeFluent:0xE816"},
            {"Dismiss", "SegoeFluent:0xE711"},
            {"Document", "SegoeFluent:0xE8A5"},
            {"Dislike", "SegoeFluent:0xE8E0"},
            {"DockLeft", "SegoeFluent:0xE90C"},
            {"DockRight", "SegoeFluent:0xE90D"},
            {"Earth", "SegoeFluent:0xE909"},
            {"Edit", "SegoeFluent:0xE70F"},
            {"Emoji", "SegoeFluent:0xE76E"},
            {"Forward", "SegoeFluent:0xE72A"},
            {"Filter", "SegoeFluent:0xE71C"},
            {"Flag", "SegoeFluent:0xE7C1"},
            {"Folder", "SegoeFluent:0xE8B7"},
            {"FolderLink", "FluentUISystemRegular:0xF428"},
            {"FontDecrease", "SegoeFluent:0xE8E7"},
            {"FontIncrease", "SegoeFluent:0xE8E8"},
            {"FontColor", "FluentUISystemRegular:0xF7C0"},
            {"Font", "SegoeFluent:0xE8D2"},
            {"FontSize", "SegoeFluent:0xE8E9"},
            {"FullScreenMaximize", "FluentUISystemRegular:0xFC16"},
            {"FullScreenMinimize", "FluentUISystemRegular:0xFC17"},
            {"Games", "SegoeFluent:0xE7FC"},
            {"Globe", "SegoeFluent:0xE774"},
            {"Highlight", "SegoeFluent:0xE891"},
            {"Home", "SegoeFluent:0xE80F"},
            {"Help", "SegoeFluent:0xE897"},
            {"Import", "SegoeFluent:0xE8B5"},
            {"Icons", "FluentUISystemRegular:0xF486"},
            {"Image", "FluentUISystemRegular:0xF48B"},
            {"ImageAltText", "FluentUISystemRegular:0xF48E"},
            {"ImageCopy", "FluentUISystemRegular:0xF491"},
            {"ImageEdit", "FluentUISystemRegular:0xF494"},
            {"Important", "SegoeFluent:0xE8C9"},
            {"Italic", "SegoeFluent:0xE8DB"},
            {"Keyboard", "SegoeFluent:0xE92E"},
            {"Library", "SegoeFluent:0xE8F1"},
            {"Link", "SegoeFluent:0xE71B"},
            {"List", "SegoeFluent:0xEA37"},
            {"Like", "SegoeFluent:0xE8E1"},
            {"MailReply", "SegoeFluent:0xE97A"},
            {"MailReplyAll", "FluentUISystemRegular:0xF17D"},
            {"Mail", "SegoeFluent:0xE715"},
            {"MailReadAll", "FluentUISystemRegular:0xF50E"},
            {"MailUnreadAll", "FluentUISystemRegular:0xF50F"},
            {"MailRead", "FluentUISystemRegular:0xF524"},
            {"MailUnread", "FluentUISystemRegular:0xF529"},
            {"Map", "SegoeFluent:0xE826"},
            {"MapDrive", "SegoeFluent:0xE8CE"},
            {"MoreVertical", "FluentUISystemRegular:0xF559"},
            {"MapPin", "SegoeFluent:0xE718"},
            {"MoreHorizontal", "SegoeFluent:0xE712"},
            {"Navigation", "SegoeFluent:0xE700"},
            {"New", "FluentUISystemRegular:0xF564"},
            {"Next", "SegoeFluent:0xE893"},
            {"NewWindow", "SegoeFluent:0xE8A7"},
            {"OpenFolder", "SegoeFluent:0xE838"},
            {"Open", "SegoeFluent:0xE8A7"},
            {"Orientation", "SegoeFluent:0xE8B4"},
            {"Paste", "SegoeFluent:0xE77F"},
            {"Page", "FluentUISystemRegular:0xF378"},
            {"People", "SegoeFluent:0xE716"},
            {"Phone", "SegoeFluent:0xE8EA"},
            {"Play", "SegoeFluent:0xE768"},
            {"PreviewLink", "SegoeFluent:0xE8A1"},
            {"Previous", "SegoeFluent:0xE892"},
            {"Print", "SegoeFluent:0xE749"},
            {"Pause", "SegoeFluent:0xE769"},
            {"Refresh", "SegoeFluent:0xE72C"},
            {"Redo", "SegoeFluent:0xE7A6"},
            {"RepeatAll", "SegoeFluent:0xE8EE"},
            {"RotateClockwise", "SegoeFluent:0xE7AD"},
            {"RotateCounterClockwise", "FluentUISystemRegular:0xF188"},
            {"RadioButton", "FluentUISystemRegular:0xF654"},
            {"Restore", "SegoeFluent:0xE923"},
            {"Ruler", "SegoeFluent:0xECC6"},
            {"Sort", "SegoeFluent:0xE8CB"},
            {"Sync", "SegoeFluent:0xE895"},
            {"Save", "SegoeFluent:0xE74E"},
            {"SaveAs", "SegoeFluent:0xE792"},
            {"Scan", "FluentUISystemRegular:0xF68B"},
            {"Send", "SegoeFluent:0xE724"},
            {"Settings", "SegoeFluent:0xE713"},
            {"Share", "SegoeFluent:0xE72D"},
            {"ShareAndroid", "FluentUISystemRegular:0xF6B2"},
            {"ShareIOS", "FluentUISystemRegular:0xF6B8"},
            {"Star", "SegoeFluent:0xE734"},
            {"StarAdd", "FluentUISystemRegular:0xF714"},
            {"StarEmphasis", "FluentUISystemRegular:0xF717"},
            {"StarOff", "FluentUISystemRegular:0xF71C"},
            {"StarProhibited", "FluentUISystemRegular:0xF71F"},
            {"Stop", "SegoeFluent:0xE71A"},
            {"Speaker0", "SegoeFluent:0xE992"},
            {"Speaker1", "FluentUISystemRegular:0xFAA4"},
            {"SpeakerBluetooth", "FluentUISystemRegular:0xFAA6"},
            {"SpeakerOff", "FluentUISystemRegular:0xFAAB"},
            {"SelectAll", "SegoeFluent:0xE8B3"},
            {"ShareScreen", "SegoeFluent:0xF71C"},
            {"Speaker2", "SegoeFluent:0xE994"},
            {"SpeakerMute", "SegoeFluent:0xE74F"},
            {"Tag", "SegoeFluent:0xE8EC"},
            {"Target", "SegoeFluent:0xF272"},
            {"TargetEdit", "FluentUISystemRegular:0xF784"},
            {"Up", "SegoeFluent:0xE74A"},
            {"Upload", "SegoeFluent:0xE898"},
            {"Underline", "SegoeFluent:0xE8DC"},
            {"Undo", "SegoeFluent:0xE7A7"},
            {"Video", "SegoeFluent:0xE714"},
            {"WeatherBlowingSnow", "FluentUISystemRegular:0xF86D"},
            {"WeatherCloudy", "FluentUISystemRegular:0xF870"},
            {"WeatherDustStorm", "FluentUISystemRegular:0xF873"},
            {"WeatherFog", "FluentUISystemRegular:0xF876"},
            {"WeatherHailDay", "FluentUISystemRegular:0xF879"},
            {"WeatherHailNight", "FluentUISystemRegular:0xF87C"},
            {"WeatherMoon", "SegoeFluent:0xE708"},
            {"WeatherPartlyCloudyDay", "FluentUISystemRegular:0xF882"},
            {"WeatherPartlyCloudyNight", "FluentUISystemRegular:0xF885"},
            {"WeatherRain", "FluentUISystemRegular:0xF888"},
            {"WeatherRainShowersDay", "FluentUISystemRegular:0xF88B"},
            {"WeatherRainShowersNight", "FluentUISystemRegular:0xF88E"},
            {"WeatherRainSnow", "FluentUISystemRegular:0xF891"},
            {"WeatherSnow", "FluentUISystemRegular:0xF894"},
            {"WeatherSnowShowerDay", "FluentUISystemRegular:0xF897"},
            {"WeatherSnowShowerNight", "FluentUISystemRegular:0xF89A"},
            {"WeatherSnowflake", "FluentUISystemRegular:0xF89D"},
            {"WeatherSqualls", "FluentUISystemRegular:0xF8A0"},
            {"WeatherSunny", "SegoeFluent:0xE706"},
            {"WeatherThunderstorm", "FluentUISystemRegular:0xF8A6"},
            {"Wifi1", "SegoeFluent:0xE872"},
            {"Wifi2", "SegoeFluent:0xE873"},
            {"Wifi3", "SegoeFluent:0xE874"},
            {"Wifi4", "SegoeFluent:0xE701"},
            {"WifiProtected", "FluentUISystemRegular:0xF8B4"},
            {"WeatherDrizzle", "FluentUISystemRegular:0xFB09"},
            {"WeatherHaze", "FluentUISystemRegular:0xFB0C"},
            {"WeatherSunnyHigh", "FluentUISystemRegular:0xFB16"},
            {"WeatherSunnyLow", "FluentUISystemRegular:0xFB19"},
            {"WifiWarning", "FluentUISystemRegular:0xFB69"},
            {"XboxConsole", "FluentUISystemRegular:0xF8C3"},
            {"ZipFolder", "SegoeFluent:0xF012"},
            {"ZoomIn", "SegoeFluent:0xE8A3"},
            {"ZoomOut", "SegoeFluent:0xE71F"},
            {"AddFilled", "SegoeFluent:0xF8AA"},
            {"AlertFilled", "FluentUISystemFilled:0xF116"},
            {"AlertOffFilled", "FluentUISystemFilled:0xF11A"},
            {"AlertOnFilled", "FluentUISystemFilled:0xF11B"},
            {"AlertSnoozeFilled", "FluentUISystemFilled:0xF11D"},
            {"AlertUrgentFilled", "FluentUISystemFilled:0xF11F"},
            {"AttachFilled", "FluentUISystemFilled:0xF1A9"},
            {"AlignCenterFilled", "FluentUISystemFilled:0xF7B2"},
            {"AlignDistributedFilled", "FluentUISystemFilled:0xF7B4"},
            {"AlignJustifiedFilled", "FluentUISystemFilled:0xF7B6"},
            {"AlignLeftFilled", "FluentUISystemFilled:0xF7B8"},
            {"AlignRightFilled", "FluentUISystemFilled:0xF7BA"},
            {"BackFilled", "SegoeFluent:0xF0D5"},
            {"BookmarkFilled", "FluentUISystemFilled:0xF1F7"},
            {"BoldFilled", "FluentUISystemFilled:0xF7BD"},
            {"BulletListFilled", "FluentUISystemFilled:0xFD8A"},
            {"CalculatorFilled", "FluentUISystemFilled:0xF20A"},
            {"CalendarDayFilled", "FluentUISystemFilled:0xF222"},
            {"CalendarEmptyFilled", "FluentUISystemFilled:0xF228"},
            {"CalendarMonthFilled", "FluentUISystemFilled:0xF22C"},
            {"CalendarReplyFilled", "FluentUISystemFilled:0xF232"},
            {"CalendarSyncFilled", "FluentUISystemFilled:0xF23A"},
            {"CalendarTodayFilled", "FluentUISystemFilled:0xF23C"},
            {"CameraFilled", "FluentUISystemFilled:0xF254"},
            {"CheckmarkFilled", "FluentUISystemFilled:0xF294"},
            {"ChevronDownFilled", "FluentUISystemFilled:0xF2A3"},
            {"ChevronLeftFilled", "FluentUISystemFilled:0xF2AA"},
            {"ChevronRightFilled", "FluentUISystemFilled:0xF2B0"},
            {"ChevronUpFilled", "FluentUISystemFilled:0xF2B6"},
            {"ClipboardFilled", "FluentUISystemFilled:0xF2C9"},
            {"ClipboardCodeFilled", "FluentUISystemFilled:0xF2CD"},
            {"ClockFilled", "FluentUISystemFilled:0xF2DD"},
            {"CloudFilled", "FluentUISystemFilled:0xF2E4"},
            {"CloudBackupFilled", "FluentUISystemFilled:0xF2E8"},
            {"CloudDownloadFilled", "FluentUISystemFilled:0xFE5B"},
            {"CloudOffFilled", "FluentUISystemFilled:0xF2EB"},
            {"CloudOfflineFilled", "FluentUISystemFilled:0xF2EC"},
            {"CloudSyncFilled", "FluentUISystemFilled:0xFB86"},
            {"CloudSyncCompleteFilled", "FluentUISystemFilled:0xF2EE"},
            {"CodeFilled", "FluentUISystemFilled:0xF2F0"},
            {"ColorBackgroundFilled", "FluentUISystemFilled:0xF2F8"},
            {"ColorFillFilled", "FluentUISystemFilled:0xF2FA"},
            {"ColorLineFilled", "FluentUISystemFilled:0xF2FC"},
            {"CommentAddFilled", "FluentUISystemFilled:0xF301"},
            {"CommentMentionFilled", "FluentUISystemFilled:0xF305"},
            {"CommentMultipleFilled", "FluentUISystemFilled:0xF308"},
            {"ContactInfoFilled", "FluentUISystemFilled:0xF320"},
            {"CopyFilled", "FluentUISystemFilled:0xF32B"},
            {"CropFilled", "FluentUISystemFilled:0xF9B7"},
            {"CutFilled", "FluentUISystemFilled:0xF33A"},
            {"ClearFormattingFilled", "FluentUISystemFilled:0xF7D5"},
            {"ClosedCaptionFilled", "FluentUISystemFilled:0xF98B"},
            {"CommentFilled", "FluentUISystemFilled:0xF991"},
            {"CalendarFilled", "SegoeFluent:0xEA89"},
            {"DownloadFilled", "FluentUISystemFilled:0xF150"},
            {"DarkThemeFilled", "FluentUISystemFilled:0xF33C"},
            {"DeleteFilled", "FluentUISystemFilled:0xF34C"},
            {"DirectionsFilled", "FluentUISystemFilled:0xF365"},
            {"DismissFilled", "SegoeFluent:0xEF2C"},
            {"DocumentFilled", "SegoeFluent:0xE729"},
            {"DislikeFilled", "FluentUISystemFilled:0xF837"},
            {"DockLeftFilled", "FluentUISystemFilled:0xFC03"},
            {"DockRightFilled", "FluentUISystemFilled:0xFC08"},
            {"EarthFilled", "FluentUISystemFilled:0xF3DA"},
            {"EditFilled", "FluentUISystemFilled:0xF9F9"},
            {"EmojiFilled", "FluentUISystemFilled:0xF3E0"},
            {"ForwardFilled", "FluentUISystemFilled:0xF181"},
            {"FilterFilled", "FluentUISystemFilled:0xF407"},
            {"FlagFilled", "SegoeFluent:0xEB4B"},
            {"FolderFilled", "SegoeFluent:0xE8D5"},
            {"FolderLinkFilled", "FluentUISystemFilled:0xF42C"},
            {"FontDecreaseFilled", "FluentUISystemFilled:0xF43C"},
            {"FontIncreaseFilled", "FluentUISystemFilled:0xF43E"},
            {"FontColorFilled", "FluentUISystemFilled:0xF7D8"},
            {"FontFilled", "FluentUISystemFilled:0xF7FD"},
            {"FontSizeFilled", "FluentUISystemFilled:0xF7FF"},
            {"FullScreenMaximizeFilled", "FluentUISystemFilled:0xFC1F"},
            {"FullScreenMinimizeFilled", "FluentUISystemFilled:0xFC20"},
            {"GamesFilled", "FluentUISystemFilled:0xF455"},
            {"GlobeFilled", "FluentUISystemFilled:0xFDC7"},
            {"HighlightFilled", "SegoeFluent:0xE891"},
            {"HomeFilled", "SegoeFluent:0xEA8A"},
            {"HelpFilled", "FluentUISystemFilled:0xF645"},
            {"ImportFilled", "FluentUISystemFilled:0xF159"},
            {"IconsFilled", "FluentUISystemFilled:0xF48D"},
            {"ImageFilled", "FluentUISystemFilled:0xF492"},
            {"ImageAltTextFilled", "FluentUISystemFilled:0xF495"},
            {"ImageCopyFilled", "FluentUISystemFilled:0xF498"},
            {"ImageEditFilled", "FluentUISystemFilled:0xF49B"},
            {"ImportantFilled", "FluentUISystemFilled:0xF4A7"},
            {"ItalicFilled", "FluentUISystemFilled:0xF80D"},
            {"KeyboardFilled", "SegoeFluent:0xEC31"},
            {"LibraryFilled", "FluentUISystemFilled:0xF4DE"},
            {"LinkFilled", "FluentUISystemFilled:0xF4F1"},
            {"ListFilled", "FluentUISystemFilled:0xF4F9"},
            {"LikeFilled", "FluentUISystemFilled:0xF839"},
            {"MailReplyFilled", "FluentUISystemFilled:0xF177"},
            {"MailReplyAllFilled", "FluentUISystemFilled:0xF17D"},
            {"MailFilled", "SegoeFluent:0xE8A8"},
            {"MailReadAllFilled", "FluentUISystemFilled:0xF518"},
            {"MailUnreadAllFilled", "FluentUISystemFilled:0xF519"},
            {"MailReadFilled", "FluentUISystemFilled:0xF52E"},
            {"MailUnreadFilled", "FluentUISystemFilled:0xF533"},
            {"MapFilled", "FluentUISystemFilled:0xF538"},
            {"MapDriveFilled", "FluentUISystemFilled:0xF53B"},
            {"MoreVerticalFilled", "FluentUISystemFilled:0xF563"},
            {"MapPinFilled", "SegoeFluent:0xE841"},
            {"MoreHorizontalFilled", "FluentUISystemFilled:0xFC39"},
            {"NavigationFilled", "FluentUISystemFilled:0xF56B"},
            {"NewFilled", "FluentUISystemFilled:0xF56E"},
            {"NextFilled", "SegoeFluent:0xF8AD"},
            {"NewWindowFilled", "FluentUISystemFilled:0xFB24"},
            {"OpenFolderFilled", "FluentUISystemFilled:0xF433"},
            {"OpenFilled", "FluentUISystemFilled:0xFA68"},
            {"OrientationFilled", "FluentUISystemFilled:0xFCF4"},
            {"PasteFilled", "FluentUISystemFilled:0xF2D5"},
            {"PageFilled", "FluentUISystemFilled:0xF378"},
            {"PeopleFilled", "FluentUISystemFilled:0xFCF5"},
            {"PhoneFilled", "FluentUISystemFilled:0xF5EB"},
            {"PlayFilled", "SegoeFluent:0xF5B0"},
            {"PreviewLinkFilled", "FluentUISystemFilled:0xF630"},
            {"PreviousFilled", "SegoeFluent:0xF8AC"},
            {"PrintFilled", "FluentUISystemFilled:0xF636"},
            {"PauseFilled", "SegoeFluent:0xF8AE"},
            {"RefreshFilled", "FluentUISystemFilled:0xF13D"},
            {"RedoFilled", "FluentUISystemFilled:0xF13F"},
            {"RepeatAllFilled", "FluentUISystemFilled:0xF172"},
            {"RotateClockwiseFilled", "SegoeFluent:0xE7AD"},
            {"RotateCounterClockwiseFilled", "FluentUISystemFilled:0xF188"},
            {"RadioButtonFilled", "FluentUISystemFilled:0xF64F"},
            {"RestoreFilled", "SegoeFluent:0xEF2F"},
            {"RulerFilled", "FluentUISystemFilled:0xF687"},
            {"SortFilled", "FluentUISystemFilled:0xF18A"},
            {"SyncFilled", "FluentUISystemFilled:0xF190"},
            {"SaveFilled", "FluentUISystemFilled:0xF68A"},
            {"SaveAsFilled", "FluentUISystemFilled:0xFC5B"},
            {"ScanFilled", "FluentUISystemFilled:0xF695"},
            {"SendFilled", "SegoeFluent:0xE725"},
            {"SettingsFilled", "SegoeFluent:0xE713"},
            {"ShareFilled", "FluentUISystemFilled:0xF6B9"},
            {"ShareAndroidFilled", "FluentUISystemFilled:0xF6BB"},
            {"ShareIOSFilled", "FluentUISystemFilled:0xF6C1"},
            {"StarFilled", "SegoeFluent:0xE735"},
            {"StarAddFilled", "FluentUISystemFilled:0xF71D"},
            {"StarEmphasisFilled", "FluentUISystemFilled:0xFD10"},
            {"StarOffFilled", "SegoeFluent:0xE8D9"},
            {"StarProhibitedFilled", "FluentUISystemFilled:0xF732"},
            {"StopFilled", "FluentUISystemFilled:0xF743"},
            {"Speaker0Filled", "SegoeFluent:0xE992"},
            {"Speaker1Filled", "FluentUISystemFilled:0xFAAF"},
            {"SpeakerBluetoothFilled", "FluentUISystemFilled:0xFAB1"},
            {"SpeakerOffFilled", "FluentUISystemFilled:0xFAB6"},
            {"SelectAllFilled", "FluentUISystemFilled:0xFC60"},
            {"ShareScreenFilled", "FluentUISystemFilled:0xFC63"},
            {"Speaker2Filled", "FluentUISystemFilled:0xFC70"},
            {"SpeakerMuteFilled", "FluentUISystemFilled:0xFC75"},
            {"TagFilled", "FluentUISystemFilled:0xF795"},
            {"TargetFilled", "FluentUISystemFilled:0xFD14"},
            {"TargetEditFilled", "FluentUISystemFilled:0xF79D"},
            {"UpFilled", "FluentUISystemFilled:0xF19B"},
            {"UploadFilled", "FluentUISystemFilled:0xF1A4"},
            {"UnderlineFilled", "FluentUISystemFilled:0xF824"},
            {"UndoFilled", "FluentUISystemFilled:0xFBD4"},
            {"VideoFilled", "SegoeFluent:0xEA0C"},
            {"WeatherBlowingSnowFilled", "FluentUISystemFilled:0xF885"},
            {"WeatherCloudyFilled", "FluentUISystemFilled:0xF888"},
            {"WeatherDustStormFilled", "FluentUISystemFilled:0xF88B"},
            {"WeatherFogFilled", "FluentUISystemFilled:0xF88E"},
            {"WeatherHailDayFilled", "FluentUISystemFilled:0xF891"},
            {"WeatherHailNightFilled", "FluentUISystemFilled:0xF894"},
            {"WeatherMoonFilled", "FluentUISystemFilled:0xF897"},
            {"WeatherPartlyCloudyDayFilled", "FluentUISystemFilled:0xF89A"},
            {"WeatherPartlyCloudyNightFilled", "FluentUISystemFilled:0xF89D"},
            {"WeatherRainFilled", "FluentUISystemFilled:0xF8A0"},
            {"WeatherRainShowersDayFilled", "FluentUISystemFilled:0xF8A3"},
            {"WeatherRainShowersNightFilled", "FluentUISystemFilled:0xF8A6"},
            {"WeatherRainSnowFilled", "FluentUISystemFilled:0xF8A9"},
            {"WeatherSnowFilled", "FluentUISystemFilled:0xF8AC"},
            {"WeatherSnowShowerDayFilled", "FluentUISystemFilled:0xF8AF"},
            {"WeatherSnowShowerNightFilled", "FluentUISystemFilled:0xF8B2"},
            {"WeatherSnowflakeFilled", "FluentUISystemFilled:0xF8B5"},
            {"WeatherSquallsFilled", "FluentUISystemFilled:0xF8B8"},
            {"WeatherSunnyFilled", "FluentUISystemFilled:0xF8BB"},
            {"WeatherThunderstormFilled", "FluentUISystemFilled:0xF8BE"},
            {"Wifi1Filled", "FluentUISystemFilled:0xF8C5"},
            {"Wifi2Filled", "FluentUISystemFilled:0xF8C7"},
            {"Wifi3Filled", "FluentUISystemFilled:0xF8C9"},
            {"Wifi4Filled", "FluentUISystemFilled:0xF8CB"},
            {"WifiProtectedFilled", "FluentUISystemFilled:0xF8CC"},
            {"WeatherDrizzleFilled", "FluentUISystemFilled:0xFB11"},
            {"WeatherHazeFilled", "FluentUISystemFilled:0xFB14"},
            {"WeatherSunnyHighFilled", "FluentUISystemFilled:0xFB1E"},
            {"WeatherSunnyLowFilled", "FluentUISystemFilled:0xFB21"},
            {"WifiWarningFilled", "FluentUISystemFilled:0xFB71"},
            {"XboxConsoleFilled", "FluentUISystemFilled:0xF8DB"},
            {"ZipFolderFilled", "FluentUISystemFilled:0xF43A"},
            {"ZoomInFilled", "FluentUISystemFilled:0xF8DD"},
            {"ZoomOutFilled", "FluentUISystemFilled:0xF8DF"}
        };
    }
}
