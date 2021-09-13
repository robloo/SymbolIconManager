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
                fluentUISystemIconNames[i] = new FluentUISystem.IconName((string)BaseMappingTable[i, 3]);
            }

            // Rebuild the mapping table using the desired icon size (or closest available)
            for (int i = 0; i < BaseMappingTable.GetLength(0); i++)
            {
                var sourceIcon = new SegoeMDL2Assets.Icon()
                {
                    Name         = (string)BaseMappingTable[i, 1],
                    UnicodePoint = (uint)BaseMappingTable[i, 0]
                };
                var destIcon = new FluentUISystem.Icon();

                if (fluentUISystemIconNames[i].Size == desiredSize)
                {
                    // Mapping is already correct
                    destIcon.Name         = (string)BaseMappingTable[i, 3];
                    destIcon.UnicodePoint = (uint)BaseMappingTable[i, 2];
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
                            destIcon.Name         = (string)BaseMappingTable[i, 3];
                            destIcon.UnicodePoint = (uint)BaseMappingTable[i, 2];

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
        private static readonly object[,] BaseMappingTable = new object[,]
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
            //  0. SegoeMDL2Assets Unicode point in hexadecimal
            //  1. SegoeMDL2Assets Description
            //  2. FluentUISystem Unicode point in hexadecimal
            //  3. FluentUISystem name
            {0xE707, "MapPin",                      0xF4F8, "ic_fluent_location_20_regular"},       // Visually different and 'MapPin = Location'
            {0xE70D, "ChevronDown",                 0xF2A3, "ic_fluent_chevron_down_20_regular"},
            {0xE70E, "ChevronUp",                   0xF2B6, "ic_fluent_chevron_up_20_regular"},
            {0xE70F, "Edit",                        0xF3DD, "ic_fluent_edit_20_regular"},
            {0xE710, "Add",                         0xF109, "ic_fluent_add_20_regular"},
            {0xE711, "Cancel",                      0xF369, "ic_fluent_dismiss_20_regular"},
            {0xE712, "More",                        0xFC2D, "ic_fluent_more_horizontal_20_regular"},
            {0xE713, "Setting",                     0xF6A9, "ic_fluent_settings_20_regular"},
            {0xE71B, "Link",                        0xFE26, "ic_fluent_link_square_20_regular"},
            {0xE71F, "ZoomOut",                     0xF8C6, "ic_fluent_zoom_out_20_regular"},
            {0xE721, "Search",                      0xF68F, "ic_fluent_search_20_regular"},
            {0xE72A, "Forward",                     0xF181, "ic_fluent_arrow_right_20_regular"},    // Doesn't exist, same as right arrow
            {0xE72B, "Back",                        0xF15B, "ic_fluent_arrow_left_20_regular"},     // Doesn't exist, same as left arrow
            {0xE72E, "Lock",                        0xFC22, "ic_fluent_lock_closed_20_regular"},
            {0xE738, "Remove",                      0xFC72, "ic_fluent_subtract_20_regular"},
            {0xE739, "Checkbox",                    0xF291, "ic_fluent_checkbox_unchecked_20_regular"},
            {0xE73A, "CheckboxComposite",           0xF28D, "ic_fluent_checkbox_checked_20_regular"},
            {0xE73E, "CheckMark",                   0xF294, "ic_fluent_checkmark_20_regular"},
            {0xE74D, "Delete",                      0xF34C, "ic_fluent_delete_20_regular"},
            {0xE74E, "Save",                        0xF67F, "ic_fluent_save_20_regular"},
            {0xE751, "ReturnKey",                   0xFBC6, "ic_fluent_arrow_enter_left_20_regular"},
            {0xE76B, "ChevronLeft",                 0xF2AA, "ic_fluent_chevron_left_20_regular"},
            {0xE76C, "ChevronRight",                0xF2B0, "ic_fluent_chevron_right_20_regular"},
            {0xE76D, "InkingTool",                  0xF4A7, "ic_fluent_inking_tool_20_regular"},
            {0xE76F, "GripperBarHorizontal",        0xF64A, "ic_fluent_re_order_24_regular"},       // Size 20 missing
            {0xE787, "Calendar",                    0xFD1D, "ic_fluent_calendar_ltr_20_regular"},
            {0xE790, "Color",                       0xF2F5, "ic_fluent_color_20_regular"},
            {0xE7B3, "RedEye",                      0xF3FB, "ic_fluent_eye_show_20_regular"},
            {0xE7BA, "Warning",                     0xF869, "ic_fluent_warning_20_regular"},
            {0xE7C3, "Page",                        0xF378, "ic_fluent_document_20_regular"},
            {0xE7EA, "ResizeTouchNarrower",         0xF15B, "ic_fluent_arrow_left_20_regular"},     // Doesn't exist, same as left arrow
            {0xE80F, "Home",                        0xF480, "ic_fluent_home_20_regular"},
            {0xE814, "IncidentTriangle",            0xF881, "ic_fluent_warning_20_filled"},         // Maps to filled variant
            {0xE81D, "Location",                    0xF4F8, "ic_fluent_location_20_regular"},
            {0xE825, "Bank",                        0xF925, "ic_fluent_building_bank_20_regular"},
            {0xE83B, "DirectAccess",                0xF769, "ic_fluent_server_20_regular"},
            {0xE894, "Clear",                       0xF369, "ic_fluent_dismiss_20_regular"},        // Doesn't exist, same as dismiss (cancel)
            {0xE895, "Sync",                        0xF190, "ic_fluent_arrow_sync_20_regular"},
            {0xE896, "Download",                    0xF150, "ic_fluent_arrow_download_20_regular"},
            {0xE897, "Help",                        0xF638, "ic_fluent_question_20_regular"},
            {0xE898, "Upload",                      0xF1A4, "ic_fluent_arrow_upload_20_regular"},
            {0xE8A3, "ZoomIn",                      0xF8C4, "ic_fluent_zoom_in_20_regular"},
            {0xE8A9, "ViewAll",                     0x100B0, "ic_fluent_border_all_20_regular"},
            {0xE8AB, "Switch",                      0xF18D, "ic_fluent_arrow_swap_20_regular"},
            {0xE8AC, "Rename",                      0xF669, "ic_fluent_rename_20_regular"},
            {0xE8B3, "SelectAll",                   0xFC56, "ic_fluent_select_all_on_24_regular"},  // Doesn't exist, visually similar doesn't exist, using similar purpose icon. Size 20 missing
            {0xE8B5, "Import",                      0xF159, "ic_fluent_arrow_import_20_regular"},   // Arrow direction is reversed
            {0xE8BC, "ShowResults",                 0xFD80, "ic_fluent_text_bullet_list_ltr_20_regular"}, // Doesn't exist, using visually similar icon (purpose not the same), ic_fluent_multiselect_20_regular alternate
            {0xE8C8, "Copy",                        0xF380, "ic_fluent_document_copy_20_regular"},  // Document copy matches icon, ic_fluent_copy_20_regular alternate
            {0xE8D1, "GotoToday",                   0xF23C, "ic_fluent_calendar_today_20_regular"},
            {0xE8E5, "OpenFile",                    0x10008, "ic_fluent_document_arrow_up_20_regular"},
            {0xE8E6, "ClearSelection",              0xF696, "ic_fluent_select_all_off_24_regular"}, // Doesn't exist, visually similar doesn't exist, using similar purpose icon. Size 20 missing
            {0xE8E7, "FontDecrease",                0xF437, "ic_fluent_font_decrease_20_regular"},
            {0xE8E8, "FontIncrease",                0xF439, "ic_fluent_font_increase_20_regular"},
            {0xE8F1, "Library",                     0xFE78, "ic_fluent_library_20_regular"},
            {0xE8F4, "NewFolder",                   0xF41C, "ic_fluent_folder_add_20_regular"},     // No vertical variant exists
            {0xE8FB, "Accept",                      0xF294, "ic_fluent_checkmark_20_regular"},
            {0xE904, "ZeroBars",                    0xFBE4, "ic_fluent_cellular_data_cellular_off_24_regular"}, // Designed in FluentUISystem off-by-one different
            {0xE905, "OneBar",                      0xF279, "ic_fluent_cellular_data_5_20_regular"}, // Designed in FluentUISystem off-by-one different
            {0xE906, "TwoBars",                     0xF277, "ic_fluent_cellular_data_4_20_regular"}, // Designed in FluentUISystem off-by-one different
            {0xE907, "ThreeBars",                   0xF275, "ic_fluent_cellular_data_3_20_regular"}, // Designed in FluentUISystem off-by-one different
            {0xE908, "FourBars",                    0xF273, "ic_fluent_cellular_data_2_20_regular"}, // Designed in FluentUISystem off-by-one different
            {0xE909, "World",                       0xF3DA, "ic_fluent_earth_20_regular"},
            {0xE916, "Stopwatch",                   0xFAE9, "ic_fluent_timer_20_regular"},
            {0xE946, "Info",                        0xF4A3, "ic_fluent_info_20_regular"},
            {0xE96D, "ChevronUpSmall",              0xF2B6, "ic_fluent_chevron_up_20_regular"},     // No smaller chevron exists, only regular
            {0xE96E, "ChevronDownSmall",            0xF2A3, "ic_fluent_chevron_down_20_regular"},   // No smaller chevron exists, only regular
            {0xE96F, "ChevronLeftSmall",            0xF2AA, "ic_fluent_chevron_left_20_regular"},   // No smaller chevron exists, only regular
            {0xE970, "ChevronRightSmall",           0xF2B0, "ic_fluent_chevron_right_20_regular"},  // No smaller chevron exists, only regular
            {0xE97A, "Reply",                       0xF177, "ic_fluent_arrow_reply_20_regular"},    // ic_fluent_arrow_hook_up_left_20_regular alternate
            {0xE9A6, "FitPage",                     0xF58F, "ic_fluent_page_fit_20_regular"},
            {0xE9D2, "AreaChart",                   0xF33D, "ic_fluent_data_area_24_regular"},      // Size 20 missing
            {0xE9D9, "Diagnostic",                  0xFBA0, "ic_fluent_pulse_square_24_regular"},   // Size 20 missing
            {0xE9E9, "Equalizer",                   0xF587, "ic_fluent_options_20_regular"},
            {0xE9F5, "Processing",                  0xF6A9, "ic_fluent_settings_20_regular"},       // Doesn't exist, closest is a single settings gear
            {0xEA37, "List",                        0xF4ED, "ic_fluent_list_20_regular"},
            {0xEA42, "BulletedListMirrored",        0xFD82, "ic_fluent_text_bullet_list_rtl_20_regular"},
            {0xEA4E, "ExpandTileMirrored",          0x100A9, "ic_fluent_arrow_step_in_right_16_regular"}, // Doesn't exist, using visually similar icon (purpose not the same), Size 20 missing
            {0xEA55, "ListMirrored",                0xF4ED, "ic_fluent_list_20_regular"},           // No mirrored or RTL variant exists, using regular
            {0xEA62, "ResizeTouchNarrowerMirrored", 0xF181, "ic_fluent_arrow_right_20_regular"},    // Doesn't exist, same as right arrow
            {0xEB05, "PieSingle",                   0xF344, "ic_fluent_data_pie_20_regular"},
            {0xEC50, "FileExplorer",                0xF418, "ic_fluent_folder_20_regular"},         // Doesn't exist, generic folder is used instead
            {0xEC59, "CashDrawer",                  0xF54F, "ic_fluent_money_20_regular"},          // Doesn't exist, no visually similar exists either. Used conceptually similar 'money'
            {0xEC92, "DateTime",                    0xF21D, "ic_fluent_calendar_clock_20_regular"},
            {0xECA5, "Tiles",                       0x100D9, "ic_fluent_glance_horizontal_20_regular"},
            {0xECC8, "AddTo",                       0xF10C, "ic_fluent_add_circle_20_regular"},
            {0xECC9, "RemoveFrom",                  0xF7B0, "ic_fluent_subtract_circle_20_regular"},
            {0xED41, "TreeFolderFolder",            0xF418, "ic_fluent_folder_20_regular"},         // No closed vertical or tree variant exists, ic_fluent_folder_open_vertical_20_regular alternate
            {0xED5B, "EmojiSwatch",                 0x10030, "ic_fluent_square_20_regular"},        // Doesn't exist, generic square is used instead
            {0xEDE1, "Export",                      0xFBC8, "ic_fluent_arrow_export_ltr_20_regular"}, // Arrow direction is reversed
            {0xEE93, "DateTimeMirrored",            0xF21D, "ic_fluent_calendar_clock_20_regular"},
            {0xF003, "Relationship",                0xF6B1, "ic_fluent_share_android_20_regular"},  // Doesn't exist, using visually similar icon (purpose not the same)
            {0xF0E2, "GridView",                    0xF462, "ic_fluent_grid_20_regular"},
            {0xF0E3, "ClipboardList",               0xFF53, "ic_fluent_clipboard_bullet_list_ltr_20_regular"},
            {0xF108, "LeftStick",                   0xF108, ""}, // Missing
            {0xF109, "RightStick",                  0xF109, ""}, // Missing
            {0xF164, "ExploreContentSingle",        0xF8CA, "ic_fluent_add_square_24_regular"},     // Size 20 missing
            {0xF168, "GroupList",                   0xF467, "ic_fluent_group_list_24_regular"},     // Size 20 missing
            {0xF413, "CopyTo",                      0xFBF1, "ic_fluent_copy_arrow_right_24_regular"}, // Doesn't exist, copy with arrow has similar visual meaning. Size 20 missing
            {0xF584, "DuplexPortraitOneSided",      0xF399, "ic_fluent_document_one_page_20_regular"},// Doesn't exist, using visually similar document (purpose not the same)
            {0xF5ED, "Set",                         0xF792, "ic_fluent_stack_20_regular"},
        };
    }
}
