using Avalonia.Platform;
using IconManager.Core.Icons;
using IconManager.Core.Serialization;
using System;

namespace IconManager
{
    /// <summary>
    /// Contains helper methods useful when working with <see cref="IconMappingList"/>s.
    /// </summary>
    public static class IconMappingListHelper
    {
        /// <summary>
        /// Loads a known mapping list for the given icon set.
        /// This will be a 'Font Mapping File'.
        /// </summary>
        /// <param name="iconSet">The icon set to get the mapping list for.</param>
        /// <returns>A new mapping list.</returns>
        public static IconMappingList Load(IconSet iconSet)
        {
            string resourcePath = string.Empty;

            switch (iconSet)
            {
                case IconSet.SegoeFluent:
                    resourcePath = IconSets.Paths.SegoeFluentMappings;
                    break;
            }

            return IconMappingListHelper.Load(resourcePath);
        }

        /// <summary>
        /// Loads a known mapping list for converting from the source icon set to the destination icon set.
        /// This will be an 'Icon Set Mapping File'.
        /// </summary>
        /// <param name="sourceIconSet">The source icon set to convert from.</param>
        /// <param name="destIconSet">The destination icon set to convert to.</param>
        /// <returns>A new mapping list.</returns>
        public static IconMappingList Load(
            IconSet sourceIconSet,
            IconSet destIconSet)
        {
            string resourcePath = string.Empty;

            if ((sourceIconSet == IconSet.FluentUISystemFilled ||
                 sourceIconSet == IconSet.FluentUISystemRegular) &&
                destIconSet == IconSet.SegoeMDL2Assets)
            {
                resourcePath = IconSets.Paths.FluentUISystemToSegoeMDL2AssetsMappings;
            }
            else if (sourceIconSet == IconSet.SegoeUISymbol &&
                     destIconSet == IconSet.SegoeMDL2Assets)
            {
                resourcePath = IconSets.Paths.SegoeUISymbolToSegoeMDL2AssetsMappings;
            }

            return IconMappingListHelper.Load(resourcePath);
        }

        /// <summary>
        /// Loads a mapping list from the given application resource path.
        /// </summary>
        /// <param name="jsonStream">The application resource path to load from.</param>
        /// <returns>A new mapping list.</returns>
        public static IconMappingList Load(string resourcePath)
        {
            if (string.IsNullOrWhiteSpace(resourcePath) == false)
            {
                using (var sourceStream = AssetLoader.Open(new Uri(resourcePath)))
                {
                    return IconMappingSerializer.Load(sourceStream);
                }
            }

            return new IconMappingList();
        }
    }
}
