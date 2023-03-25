using Avalonia;
using Avalonia.Platform;
using System;

namespace IconManager
{
    public static class SegoeMDL2AssetsToFluentUISystem
    {
        public static IconMappingList BuildMapping(FluentUISystem.IconSize desiredSize)
        {
            IconMappingList mappings;
            IconMappingList finalMappings = new IconMappingList();

            // Load the mappings table
            var assets = AvaloniaLocator.Current.GetRequiredService<IAssetLoader>();
            string sourceDataPath = "avares://IconManager/Data/Mappings/FluentUISystemToSegoeMDL2Assets.json";

            using (var sourceStream = assets.Open(new Uri(sourceDataPath)))
            {
                mappings = IconMappingList.Load(sourceStream);
            }

            var adjustedMappings = FluentUISystem.ConvertToSize(mappings, desiredSize);

            for (int i = 0; i < adjustedMappings.Count; i++)
            {
                // Note: The mapping order is reversed here
                finalMappings.Add(new IconMapping(adjustedMappings[i].Destination, adjustedMappings[i].Source));
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
