using System;
using System.Collections.Generic;
using System.Linq;

namespace IconManager
{
    public static class SegoeMDL2AssetsToFluentUISystem
    {
        public static IList<IconMapping> BuildMapping(FluentUISystem.IconSize desiredSize)
        {
            int nonExactMappings = 0;
            int missingMappings = 0;
            FluentUISystem.IconName[] fluentUISystemIconNames;
            var mappingTable = new List<IconMapping>();

            // Process the initial base table extracting Fluent UI System icon details
            // iconNames will be indexed matching the base mapping table
            fluentUISystemIconNames = new FluentUISystem.IconName[BaseMappingTable.GetLength(0)];
            for (int i = 0; i < BaseMappingTable.GetLength(0); i++)
            {
                fluentUISystemIconNames[i] = new FluentUISystem.IconName(BaseMappingTable[i, 3]);
            }

            // Rebuild the mapping table using the desired icon size (or closest available)
            for (int i = 0; i < BaseMappingTable.GetLength(0); i++)
            {
                var sourceIcon = new SegoeMDL2Assets.Icon()
                {
                    Name         = BaseMappingTable[i, 1],
                    UnicodePoint = BaseMappingTable[i, 0]
                };
                var destIcon = new FluentUISystem.Icon();

                if (fluentUISystemIconNames[i].Size == desiredSize)
                {
                    // Mapping is already correct
                    destIcon.Name         = BaseMappingTable[i, 3];
                    destIcon.UnicodePoint = BaseMappingTable[i, 2];
                }
                else
                {
                    // Attempt an exact size match
                    var match = FluentUISystem.FindIcon(
                        fluentUISystemIconNames[i].BaseName,
                        desiredSize,
                        fluentUISystemIconNames[i].Theme);

                    if (match != null)
                    {
                        destIcon = match;
                    }
                    else
                    {
                        // Use the nearest numerical size instead
                        var matches = FluentUISystem.FindIcons(
                            fluentUISystemIconNames[i].BaseName,
                            fluentUISystemIconNames[i].Theme);

                        if (matches != null &&
                            matches.Count > 0)
                        {
                            // To find the numerically closest match in size simply find the difference from the desired size
                            // to actual size for each item, sort from smallest to largest, then take the first item
                            var closestMatch = matches.OrderBy(icon => Math.Abs(FluentUISystem.ToNumericalSize(desiredSize) - icon.NumericalSize)).First();
                            destIcon = closestMatch;

                            nonExactMappings++;
                        }
                        else
                        {
                            // Nothing was found, use the original base mapping
                            destIcon.Name         = BaseMappingTable[i, 3];
                            destIcon.UnicodePoint = BaseMappingTable[i, 2];

                            missingMappings++;
                        }
                    }
                }

                mappingTable.Add(new IconMapping(sourceIcon, destIcon));
            }

            return mappingTable;
        }

        /// <summary>
        /// The default/base SegoeMDL2Assets to FluentUISystem glyph mapping table.
        /// Size 20 is used by default.
        /// </summary>
        private static readonly string[,] BaseMappingTable = new string[,]
        {
            // Sequence goes as follows:
            //  1. Use an icon of the same exact metaphor (even if it visually differs, unless totally different)
            //      Add -> ic_fluent_add_20_regular
            //      Import -> ic_fluent_arrow_import_20_regular (even though arrow direction is reversed 'Export')
            //  2. Use an icon of a similar design but more generic purpose/metaphor
            //      Forward -> ic_fluent_arrow_right_20_regular
            //  2. Use an icon of a similar design but different metaphor
            //
            // Columns:
            //  1. SegoeMDL2Assets Unicode point in hexadecimal
            //  2. SegoeMDL2Assets Description
            //  3. FluentUISystem Unicode point in hexadecimal
            //  4. FluentUISystem name
            {"E707", "MapPin",                      "F4F8", "ic_fluent_location_20_regular"},       // Visually different and 'MapPin = Location'
            {"E70D", "ChevronDown",                 "F2A3", "ic_fluent_chevron_down_20_regular"},
            {"E70E", "ChevronUp",                   "F2B6", "ic_fluent_chevron_up_20_regular"},
            {"E70F", "Edit",                        "F3DD", "ic_fluent_edit_20_regular"},
            {"E710", "Add",                         "F109", "ic_fluent_add_20_regular"},
            {"E711", "Cancel",                      "F369", "ic_fluent_dismiss_20_regular"},
            {"E712", "More",                        "FC2D", "ic_fluent_more_horizontal_20_regular"},
            {"E713", "Setting",                     "F6A9", "ic_fluent_settings_20_regular"},
            {"E71B", "Link",                        "FE26", "ic_fluent_link_square_20_regular"},
            {"E71F", "ZoomOut",                     "F8C6", "ic_fluent_zoom_out_20_regular"},
            {"E721", "Search",                      "F68F", "ic_fluent_search_20_regular"},
            {"E72A", "Forward",                     "F181", "ic_fluent_arrow_right_20_regular"},    // Doesn't exist, same as right arrow
            {"E72B", "Back",                        "F15B", "ic_fluent_arrow_left_20_regular"},     // Doesn't exist, same as left arrow
            {"E72E", "Lock",                        "FC22", "ic_fluent_lock_closed_20_regular"},
            {"E738", "Remove",                      "FC72", "ic_fluent_subtract_20_regular"},
            {"E739", "Checkbox",                    "F291", "ic_fluent_checkbox_unchecked_20_regular"},
            {"E73A", "CheckboxComposite",           "F28D", "ic_fluent_checkbox_checked_20_regular"},
            {"E73E", "CheckMark",                   "F294", "ic_fluent_checkmark_20_regular"},
            {"E74D", "Delete",                      "F34C", "ic_fluent_delete_20_regular"},
            {"E74E", "Save",                        "F67F", "ic_fluent_save_20_regular"},
            {"E751", "ReturnKey",                   "FBC6", "ic_fluent_arrow_enter_left_20_regular"},
            {"E76B", "ChevronLeft",                 "F2AA", "ic_fluent_chevron_left_20_regular"},
            {"E76C", "ChevronRight",                "F2B0", "ic_fluent_chevron_right_20_regular"},
            {"E76D", "InkingTool",                  "F4A7", "ic_fluent_inking_tool_20_regular"},
            {"E76F", "GripperBarHorizontal",        "F64A", "ic_fluent_re_order_24_regular"},       // Size 20 missing
            {"E787", "Calendar",                    "FD1D", "ic_fluent_calendar_ltr_20_regular"},
            {"E790", "Color",                       "F2F5", "ic_fluent_color_20_regular"},
            {"E7B3", "RedEye",                      "F3FB", "ic_fluent_eye_show_20_regular"},
            {"E7BA", "Warning",                     "F869", "ic_fluent_warning_20_regular"},
            {"E7C3", "Page",                        "F378", "ic_fluent_document_20_regular"},
            {"E7EA", "ResizeTouchNarrower",         "F15B", "ic_fluent_arrow_left_20_regular"},     // Doesn't exist, same as left arrow
            {"E80F", "Home",                        "F480", "ic_fluent_home_20_regular"},
            {"E814", "IncidentTriangle",            "F881", "ic_fluent_warning_20_filled"},         // Maps to filled variant
            {"E81D", "Location",                    "F4F8", "ic_fluent_location_20_regular"},
            {"E825", "Bank",                        "F925", "ic_fluent_building_bank_20_regular"},
            {"E83B", "DirectAccess",                "F769", "ic_fluent_server_20_regular"},
            {"E894", "Clear",                       "F369", "ic_fluent_dismiss_20_regular"},        // Doesn't exist, same as dismiss (cancel)
            {"E895", "Sync",                        "F190", "ic_fluent_arrow_sync_20_regular"},
            {"E896", "Download",                    "F150", "ic_fluent_arrow_download_20_regular"},
            {"E897", "Help",                        "F638", "ic_fluent_question_20_regular"},
            {"E898", "Upload",                      "F1A4", "ic_fluent_arrow_upload_20_regular"},
            {"E8A3", "ZoomIn",                      "F8C4", "ic_fluent_zoom_in_20_regular"},
            {"E8A9", "ViewAll",                     "100B0", "ic_fluent_border_all_20_regular"},
            {"E8AB", "Switch",                      "F18D", "ic_fluent_arrow_swap_20_regular"},
            {"E8AC", "Rename",                      "F669", "ic_fluent_rename_20_regular"},
            {"E8B3", "SelectAll",                   "FC56", "ic_fluent_select_all_on_24_regular"},  // Doesn't exist, visually similar doesn't exist, using similar purpose icon. Size 20 missing
            {"E8B5", "Import",                      "F159", "ic_fluent_arrow_import_20_regular"},   // Arrow direction is reversed
            {"E8BC", "ShowResults",                 "FD80", "ic_fluent_text_bullet_list_ltr_20_regular"}, // Doesn't exist, using visually similar icon (purpose not the same), ic_fluent_multiselect_20_regular alternate
            {"E8C8", "Copy",                        "F380", "ic_fluent_document_copy_20_regular"},  // Document copy matches icon, ic_fluent_copy_20_regular alternate
            {"E8D1", "GotoToday",                   "F23C", "ic_fluent_calendar_today_20_regular"},
            {"E8E5", "OpenFile",                    "10008", "ic_fluent_document_arrow_up_20_regular"},
            {"E8E6", "ClearSelection",              "F696", "ic_fluent_select_all_off_24_regular"}, // Doesn't exist, visually similar doesn't exist, using similar purpose icon. Size 20 missing
            {"E8E7", "FontDecrease",                "F437", "ic_fluent_font_decrease_20_regular"},
            {"E8E8", "FontIncrease",                "F439", "ic_fluent_font_increase_20_regular"},
            {"E8F1", "Library",                     "FE78", "ic_fluent_library_20_regular"},
            {"E8F4", "NewFolder",                   "F41C", "ic_fluent_folder_add_20_regular"},     // No vertical variant exists
            {"E8FB", "Accept",                      "F294", "ic_fluent_checkmark_20_regular"},
            {"E904", "ZeroBars",                    "FBE4", "ic_fluent_cellular_data_cellular_off_24_regular"}, // Designed in FluentUISystem off-by-one different
            {"E905", "OneBar",                      "F279", "ic_fluent_cellular_data_5_20_regular"}, // Designed in FluentUISystem off-by-one different
            {"E906", "TwoBars",                     "F277", "ic_fluent_cellular_data_4_20_regular"}, // Designed in FluentUISystem off-by-one different
            {"E907", "ThreeBars",                   "F275", "ic_fluent_cellular_data_3_20_regular"}, // Designed in FluentUISystem off-by-one different
            {"E908", "FourBars",                    "F273", "ic_fluent_cellular_data_2_20_regular"}, // Designed in FluentUISystem off-by-one different
            {"E909", "World",                       "F3DA", "ic_fluent_earth_20_regular"},
            {"E916", "Stopwatch",                   "FAE9", "ic_fluent_timer_20_regular"},
            {"E946", "Info",                        "F4A3", "ic_fluent_info_20_regular"},
            {"E96D", "ChevronUpSmall",              "F2B6", "ic_fluent_chevron_up_20_regular"},     // No smaller chevron exists, only regular
            {"E96E", "ChevronDownSmall",            "F2A3", "ic_fluent_chevron_down_20_regular"},   // No smaller chevron exists, only regular
            {"E96F", "ChevronLeftSmall",            "F2AA", "ic_fluent_chevron_left_20_regular"},   // No smaller chevron exists, only regular
            {"E970", "ChevronRightSmall",           "F2B0", "ic_fluent_chevron_right_20_regular"},  // No smaller chevron exists, only regular
            {"E97A", "Reply",                       "F177", "ic_fluent_arrow_reply_20_regular"},    // ic_fluent_arrow_hook_up_left_20_regular alternate
            {"E9A6", "FitPage",                     "F58F", "ic_fluent_page_fit_20_regular"},
            {"E9D2", "AreaChart",                   "F33D", "ic_fluent_data_area_24_regular"},      // Size 20 missing
            {"E9D9", "Diagnostic",                  "FBA0", "ic_fluent_pulse_square_24_regular"},   // Size 20 missing
            {"E9E9", "Equalizer",                   "F587", "ic_fluent_options_20_regular"},
            {"E9F5", "Processing",                  "F6A9", "ic_fluent_settings_20_regular"},       // Doesn't exist, closest is a single settings gear
            {"EA37", "List",                        "F4ED", "ic_fluent_list_20_regular"},
            {"EA42", "BulletedListMirrored",        "FD82", "ic_fluent_text_bullet_list_rtl_20_regular"},
            {"EA4E", "ExpandTileMirrored",          "100A9", "ic_fluent_arrow_step_in_right_16_regular"}, // Doesn't exist, using visually similar icon (purpose not the same), Size 20 missing
            {"EA55", "ListMirrored",                "F4ED", "ic_fluent_list_20_regular"},           // No mirrored or RTL variant exists, using regular
            {"EA62", "ResizeTouchNarrowerMirrored", "F181", "ic_fluent_arrow_right_20_regular"},    // Doesn't exist, same as right arrow
            {"EB05", "PieSingle",                   "F344", "ic_fluent_data_pie_20_regular"},
            {"EC50", "FileExplorer",                "F418", "ic_fluent_folder_20_regular"},         // Doesn't exist, generic folder is used instead
            {"EC59", "CashDrawer",                  "F54F", "ic_fluent_money_20_regular"},          // Doesn't exist, no visually similar exists either. Used conceptually similar 'money'
            {"EC92", "DateTime",                    "F21D", "ic_fluent_calendar_clock_20_regular"},
            {"ECA5", "Tiles",                       "100D9", "ic_fluent_glance_horizontal_20_regular"},
            {"ECC8", "AddTo",                       "F10C", "ic_fluent_add_circle_20_regular"},
            {"ECC9", "RemoveFrom",                  "F7B0", "ic_fluent_subtract_circle_20_regular"},
            {"ED41", "TreeFolderFolder",            "F418", "ic_fluent_folder_20_regular"},         // No closed vertical or tree variant exists, ic_fluent_folder_open_vertical_20_regular alternate
            {"ED5B", "EmojiSwatch",                 "10030", "ic_fluent_square_20_regular"},        // Doesn't exist, generic square is used instead
            {"EDE1", "Export",                      "FBC8", "ic_fluent_arrow_export_ltr_20_regular"}, // Arrow direction is reversed
            {"EE93", "DateTimeMirrored",            "F21D", "ic_fluent_calendar_clock_20_regular"},
            {"F003", "Relationship",                "F6B1", "ic_fluent_share_android_20_regular"},  // Doesn't exist, using visually similar icon (purpose not the same)
            {"F0E2", "GridView",                    "F462", "ic_fluent_grid_20_regular"},
            {"F0E3", "ClipboardList",               "FF53", "ic_fluent_clipboard_bullet_list_ltr_20_regular"},
            {"F108", "LeftStick",                   "F108", ""}, // Missing
            {"F109", "RightStick",                  "F109", ""}, // Missing
            {"F164", "ExploreContentSingle",        "F8CA", "ic_fluent_add_square_24_regular"},     // Size 20 missing
            {"F168", "GroupList",                   "F467", "ic_fluent_group_list_24_regular"},     // Size 20 missing
            {"F413", "CopyTo",                      "FBF1", "ic_fluent_copy_arrow_right_24_regular"}, // Doesn't exist, copy with arrow has similar visual meaning. Size 20 missing
            {"F584", "DuplexPortraitOneSided",      "F399", "ic_fluent_document_one_page_20_regular"},// Doesn't exist, using visually similar document (purpose not the same)
            {"F5ED", "Set",                         "F792", "ic_fluent_stack_20_regular"},
        };
    }
}
