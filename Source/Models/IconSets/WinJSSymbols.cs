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
    public class WinJSSymbols : IconSetBase
    {
        private static IReadOnlyList<Icon>?               cachedIcons = null;
        private static IReadOnlyDictionary<uint, string>? cachedNames = null;

        private static object cacheMutex = new object();

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private static void RebuildCache()
        {
            var icons = new List<Icon>();
            var names = new Dictionary<uint, string>();
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
                        var icon = new Icon()
                        {
                            Name         = entry.Value,
                            UnicodePoint = Convert.ToUInt32(entry.Key.Substring(2), 16) // Remove 'U+'
                        };

                        icons.Add(icon);
                        names.Add(icon.UnicodePoint, icon.Name);
                    }
                }
            }

            lock (cacheMutex)
            {
                cachedIcons = icons.AsReadOnly();
                cachedNames = names;
            }

            return;
        }

        public static string FindName(uint unicodePoint)
        {
            string? name = null;

            lock (cacheMutex)
            {
                if (cachedNames == null)
                {
                    RebuildCache();
                }

                cachedNames!.TryGetValue(unicodePoint, out name);
            }

            return name ?? string.Empty;
        }

        /// <summary>
        /// Gets a read-only list of all icons in the WinJS Symbols icon set.
        /// </summary>
        public static IReadOnlyList<IReadOnlyIcon> Icons
        {
            get
            {
                lock (cacheMutex)
                {
                    if (cachedIcons == null)
                    {
                        RebuildCache();
                    }
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
