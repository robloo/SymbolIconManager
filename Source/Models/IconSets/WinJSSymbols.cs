using Avalonia;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace IconManager
{
    /// <summary>
    /// Contains data and information for WinJS Symbols icons.
    /// </summary>
    public class WinJSSymbols
    {
        private static IReadOnlyList<Icon>? cachedIcons = null;

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private static void RebuildCache()
        {
            var icons = new List<Icon>();
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            string sourceDataPath = "avares://IconManager/Data/Symbols.json";

            using (var sourceStream = assets.Open(new Uri(sourceDataPath)))
            using (var reader = new StreamReader(sourceStream))
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
                            UnicodePoint = Convert.ToUInt32(entry.Key.Substring(2), 16) // Remove 'U+'
                        });
                    }
                }
            }

            cachedIcons = icons.AsReadOnly();

            return;
        }

        /// <summary>
        /// Gets a read-only list of all icons in the WinJS Symbols icon set.
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

        /***************************************************************************************
         *
         * Classes
         *
         ***************************************************************************************/

        /// <summary>
        /// Represents a single icon in WinJS Symbols.
        /// </summary>
        public class Icon : IconManager.Icon, IIcon
        {
            public Icon() : base()
            {
                base.IconSet = IconSet.WinJSSymbols;
            }
        }
    }
}
