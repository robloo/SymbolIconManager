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

        private static void RebuildCache()
        {
            List<Icon> icons = new List<Icon>();
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
                        icons.Add(new Icon()
                        {
                            Name         = entry.Value,
                            UnicodePoint = entry.Key.ToUpperInvariant()
                        });
                    }
                }
            }

            cachedIcons = icons.AsReadOnly();

            return;
        }

        /// <summary>
        /// Gets a read-only list of all icons in the Segoe MDL2 Assets icon set.
        /// </summary>
        public static IReadOnlyList<IIcon> Icons
        {
            get
            {
                if (cachedIcons == null)
                {
                    RebuildCache();
                }

                return cachedIcons!;
            }
        }

        /// <summary>
        /// Represents a single icon in Segoe MDL2 Assets.
        /// </summary>
        public class Icon : IIcon
        {
            /// <inheritdoc/>
            public string Name { get; set; } = string.Empty;

            /// <inheritdoc/>
            public string UnicodePoint { get; set; } = string.Empty;
        }
    }
}
