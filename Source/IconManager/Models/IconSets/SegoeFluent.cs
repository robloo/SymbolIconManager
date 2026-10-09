using Avalonia.Platform;
using IconManager.Core.Icons;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace IconManager
{
    /// <summary>
    /// Contains data and information for the Segoe Fluent Icons.
    /// </summary>
    public class SegoeFluent : IconSetBase
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

            using (var sourceStream = AssetLoader.Open(new Uri(IconSets.Paths.SegoeFluent)))
            using (var reader = new StreamReader(sourceStream))
            {
                string jsonString = reader.ReadToEnd();
                var rawIcons = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

                if (rawIcons is not null)
                {
                    foreach (var entry in rawIcons)
                    {
                        var icon = new Icon()
                        {
                            Name         = entry.Value,
                            UnicodePoint = Convert.ToUInt32(entry.Key, 16)
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
                if (cachedNames is null)
                {
                    RebuildCache();
                }

                cachedNames!.TryGetValue(unicodePoint, out name);
            }

            return name ?? string.Empty;
        }

        /// <summary>
        /// Gets a read-only list of all icons in the Segoe MDL2 Assets icon set.
        /// </summary>
        public static IReadOnlyList<IReadOnlyIcon> Icons
        {
            get
            {
                lock (cacheMutex)
                {
                    if (cachedIcons is null)
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
        /// Represents a single icon in Segoe Fluent Icons.
        /// </summary>
        public class Icon : IconManager.Core.Icons.Icon, IIcon
        {
            public Icon() : base()
            {
                base.IconSet = IconSet.SegoeFluent;
            }
        }
    }
}
