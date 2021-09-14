using Avalonia;
using Avalonia.Platform;
using System;
using System.Linq;

namespace IconManager
{
    public static class SegoeMDL2AssetsToFluentUISystem
    {
        public static IconMappingList BuildMapping(FluentUISystem.IconSize desiredSize)
        {
            int nonExactMappings = 0;
            int missingMappings = 0;
            FluentUISystem.IconName[] fluentUISystemIconNames;
            IconMappingList mappings;
            IconMappingList finalMappings = new IconMappingList();

            // Load the mappings table
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            string sourceDataPath = "avares://IconManager/Data/Mappings/FluentUISystemToSegoeMDL2Assets.json";

            using (var sourceStream = assets.Open(new Uri(sourceDataPath)))
            {
                mappings = IconMappingList.Load(sourceStream);
            }

            // Process the initial mappings extracting Fluent UI System icon details
            // iconNames will be indexed matching the base mapping table
            fluentUISystemIconNames = new FluentUISystem.IconName[mappings.Count];
            for (int i = 0; i < mappings.Count; i++)
            {
                fluentUISystemIconNames[i] = new FluentUISystem.IconName(mappings[i].Source.Name);
            }

            // Rebuild the mappings using the desired icon size (or closest available)
            for (int i = 0; i < mappings.Count; i++)
            {
                var segoeMDL2AssetsIcon = new SegoeMDL2Assets.Icon()
                {
                    Name         = mappings[i].Destination.Name,
                    UnicodePoint = mappings[i].Destination.UnicodePoint
                };
                var fluentUISystemIcon = new FluentUISystem.Icon();

                if (fluentUISystemIconNames[i].Size == desiredSize)
                {
                    // Mapping is already correct
                    fluentUISystemIcon.Name         = mappings[i].Source.Name;
                    fluentUISystemIcon.UnicodePoint = mappings[i].Source.UnicodePoint;
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
                        fluentUISystemIcon = match;
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
                            fluentUISystemIcon = closestMatch;

                            nonExactMappings++;
                        }
                        else
                        {
                            // Nothing was found, use the original base mapping
                            fluentUISystemIcon.Name         = mappings[i].Source.Name;
                            fluentUISystemIcon.UnicodePoint = mappings[i].Source.UnicodePoint;

                            missingMappings++;
                        }
                    }
                }

                // Note: The mapping order is reversed here
                finalMappings.Add(new IconMapping(segoeMDL2AssetsIcon, fluentUISystemIcon.AsIcon()));
            }

            return finalMappings;
        }

        // TODO: Move a version of this to the data ReadMe
        // Sequence goes as follows:
        //  1. Use an icon of the same exact metaphor (even if it visually differs, unless totally different)
        //      Add -> ic_fluent_add_20_regular
        //      Import -> ic_fluent_arrow_import_20_regular (even though arrow direction is reversed 'Export')
        //  2. Use an icon of a similar design but more generic purpose/metaphor
        //      Forward -> ic_fluent_arrow_right_20_regular
        //  2. Use an icon of a similar design but different metaphor
    }
}
