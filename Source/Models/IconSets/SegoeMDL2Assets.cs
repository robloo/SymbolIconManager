using Avalonia;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace IconManager
{
    /// <summary>
    /// Contains data and information for the Segoe MDL2 Assets Icons.
    /// </summary>
    public class SegoeMDL2Assets
    {
        private static IReadOnlyList<Icon>? cachedIcons = null;
        private static IReadOnlyDictionary<string, string>? cachedNames = null;

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private static void RebuildCache()
        {
            List<Icon> icons = new List<Icon>();
            Dictionary<string, string> names = new Dictionary<string, string>();
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            string sourceDataPath = "avares://IconManager/Data/SegoeMDL2Assets.json";

            using (var reader = new StreamReader(assets.Open(new Uri(sourceDataPath))))
            {
                string jsonString = reader.ReadToEnd();
                var rawIcons = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

                if (rawIcons != null)
                {
                    foreach (var entry in rawIcons)
                    {
                        var icon = new Icon()
                        {
                            Name         = entry.Value,
                            UnicodePoint = entry.Key.ToUpperInvariant()
                        };

                        icons.Add(icon);
                        names.Add(icon.UnicodePoint, icon.Name);
                    }
                }
            }

            cachedIcons = icons.AsReadOnly();
            cachedNames = names;

            return;
        }

        public static string FindName(string unicodePoint)
        {
            if (cachedIcons == null || cachedNames == null)
            {
                RebuildCache();
            }

            if (cachedNames!.TryGetValue(unicodePoint, out string? name))
            {
                return name ?? string.Empty;
            }
            else
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Gets a read-only list of all icons in the Segoe MDL2 Assets icon set.
        /// </summary>
        public static IReadOnlyList<IIcon> Icons
        {
            get
            {
                if (cachedIcons == null || cachedNames == null)
                {
                    RebuildCache();
                }

                return cachedIcons!;
            }
        }

        /***************************************************************************************
         *
         * Classes
         *
         ***************************************************************************************/

        /// <summary>
        /// Represents a single icon in Segoe MDL2 Assets.
        /// </summary>
        public class Icon : IIcon
        {
            /// <inheritdoc/>
            public IconSet IconSet { get; set; } = IconSet.SegoeMDL2Assets;

            /// <inheritdoc/>
            public string Name { get; set; } = string.Empty;

            /// <inheritdoc/>
            public string UnicodePoint { get; set; } = string.Empty;
        }
    }
}
