using Avalonia;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;

namespace IconManager
{
    /// <summary>
    /// Contains data and information for the Fluent UI System Icons.
    /// </summary>
    public class FluentUISystem
    {
        /// <summary>
        /// Defines an icon size within the <see cref="FluentUISystem"/>.
        /// </summary>
        public enum IconSize
        {
            Size12,
            Size16,
            /// <summary>
            /// Designed specifically for desktop.
            /// </summary>
            Size20,
            /// <remarks>
            /// The 24-pixel art board uses a 1.5 pixel stroke.
            /// This will not align with the pixel grid and may appear blurry.
            /// </remarks>
            Size24,
            Size28,
            Size32,
            Size48
        }

        /// <summary>
        /// Defines an icon theme within the <see cref="FluentUISystem"/>.
        /// </summary>
        public enum IconTheme
        {
            /// <summary>
            /// Used when weight is needed.
            /// </summary>
            Filled,
            /// <summary>
            /// Standard theme for use. These have a nice, friendly weight to them.
            /// </summary>
            Regular
        }

        /// <summary>
        /// Defines an icon naming format within the <see cref="FluentUISystem"/>.
        /// </summary>
        public enum NamingFormat
        {
            Android,
            iOS
        }

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
            string[] sourceDataPaths = new string[]
            {
                "avares://IconManager/Data/FluentSystemIcons-Filled.json",
                "avares://IconManager/Data/FluentSystemIcons-Regular.json"
            };

            // Load all data from JSON source files
            foreach (string path in sourceDataPaths)
            {
                using (var sourceStream = assets.Open(new Uri(path)))
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
                                RawName      = entry.Key,
                                Name         = entry.Key, // Automatically parses into components
                                UnicodePoint = Convert.ToUInt32(entry.Value.Substring(2), 16) // Remove '0x'
                            });
                        }
                    }
                }
            }

            cachedIcons = icons.AsReadOnly();

            return;
        }

        /// <summary>
        /// Gets a read-only list of all icons in the Fluent UI System icon set.
        /// This includes BOTH the regular and filled themes.
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
        /// Gets all icons of the desired theme.
        /// </summary>
        public static IReadOnlyList<Icon> GetIcons(IconTheme desiredTheme)
        {
            var matchingIcons = new List<Icon>();

            if (cachedIcons == null)
            {
                RebuildCache();
            }

            foreach (Icon icon in cachedIcons!)
            {
                if (icon.Theme == desiredTheme)
                {
                    matchingIcons.Add(icon);
                }
            }

            return matchingIcons.AsReadOnly();
        }

        public static Icon? FindIcon(
            string baseName,
            IconSize desiredSize,
            IconTheme desiredTheme)
        {
            if (cachedIcons == null)
            {
                RebuildCache();
            }

            foreach (Icon icon in cachedIcons!)
            {
                if (string.Equals(icon.BaseName, baseName, StringComparison.OrdinalIgnoreCase) &&
                    icon.Size == desiredSize &&
                    icon.Theme == desiredTheme)
                {
                    return icon;
                }
            }

            return null;
        }

        /// <summary>
        /// Finds all sizes of icons matching the base name and desired theme.
        /// </summary>
        public static IList<Icon> FindIcons(
            string baseName,
            IconTheme desiredTheme)
        {
            var matchingIcons = new List<Icon>();

            if (cachedIcons == null)
            {
                RebuildCache();
            }

            foreach (Icon icon in cachedIcons!)
            {
                if (string.Equals(icon.BaseName, baseName, StringComparison.OrdinalIgnoreCase) &&
                    icon.Theme == desiredTheme)
                {
                    matchingIcons.Add(icon);
                }
            }

            return matchingIcons;
        }

        /// <summary>
        /// Finds all themes of icons matching the base name and desired size.
        /// </summary>
        public static IList<Icon> FindIcons(
            string baseName,
            IconSize desiredSize)
        {
            var matchingIcons = new List<Icon>();

            if (cachedIcons == null)
            {
                RebuildCache();
            }

            foreach (Icon icon in cachedIcons!)
            {
                if (string.Equals(icon.BaseName, baseName, StringComparison.OrdinalIgnoreCase) &&
                    icon.Size == desiredSize)
                {
                    matchingIcons.Add(icon);
                }
            }

            return matchingIcons;
        }

        public static Icon? FindNearestSize(
            Icon icon,
            IconSize desiredSize)
        {
            // TODO

            return null;
        }

        public static int ToNumericalSize(IconSize size) => size switch
        {
            IconSize.Size12 => 12,
            IconSize.Size16 => 16,
            IconSize.Size20 => 20,
            IconSize.Size24 => 24,
            IconSize.Size28 => 28,
            IconSize.Size32 => 32,
            IconSize.Size48 => 48,
            _ => 0,
        };

        /***************************************************************************************
         *
         * Classes
         *
         ***************************************************************************************/

        /// <summary>
        /// Represents a single icon in the Fluent UI System.
        /// </summary>
        public class Icon : IconName, IIcon
        {
            /// <inheritdoc/>
            public IconSet IconSet
            {
                get
                {
                    if (this.Theme == IconTheme.Filled)
                    {
                        return IconSet.FluentUISystemFilled;
                    }
                    else
                    {
                        return IconSet.FluentUISystemRegular;
                    }
                }
                set { /* Do nothing */ }
            }

            /// <summary>
            /// Gets or sets the raw, unparsed name or description of the icon.
            /// </summary>
            public string RawName { get; set; } = string.Empty;

            /// <inheritdoc/>
            public uint UnicodePoint { get; set; } = 0;

            /// <inheritdoc/>
            public string UnicodeString
            {
                get => IconManager.Icon.ToUnicodeString(this.UnicodePoint);
            }

            /// <summary>
            /// Converts this <see cref="FluentUISystem.Icon"/> into a standard <see cref="Icon"/>.
            /// This is sometimes needed because <see cref="FluentUISystem.Icon"/> does not derive from
            /// <see cref="Icon"/> like most other icons do. It only implements the interface.
            /// </summary>
            /// <returns>A new <see cref="IconManager.Icon"/>.</returns>
            public IconManager.Icon AsIcon()
            {
                return new IconManager.Icon()
                {
                    IconSet      = this.IconSet,
                    Name         = this.Name,
                    UnicodePoint = this.UnicodePoint
                };
            }
        }

        /// <summary>
        /// Represents a single Fluent UI System icon name with all of
        /// its individual components.
        /// </summary>
        public class IconName
        {
            private string       _BaseName = string.Empty;
            private IconSize     _Size     = IconSize.Size12;
            private IconTheme    _Theme    = IconTheme.Regular;
            private NamingFormat _Format   = NamingFormat.Android;

            public IconName()
            {
            }

            public IconName(string name)
            {
                this.SetName(name);
            }

            /// <summary>
            /// Gets or sets the full name of the icon including all components.
            /// Example: ic_fluent_caret_up_24_filled.
            /// </summary>
            public string Name
            {
                get => this.GetName(
                    this.BaseName,
                    this.Size,
                    this.Theme,
                    this.Format);
                set => this.SetName(value);
            }

            /// <summary>
            /// Gets or sets the base name or description component corresponding to the
            /// metaphor the icon represents.
            /// Example: caret_up.
            /// </summary>
            public string BaseName
            {
                get => this._BaseName;
                set => this._BaseName = value;
            }

            /// <summary>
            /// Gets or sets the size component of the icon name.
            /// </summary>
            public IconSize Size
            {
                get => this._Size;
                set => this._Size = value;
            }

            /// <summary>
            /// Gets the size component of the icon name returned as an integer.
            /// </summary>
            public int NumericalSize
            {
                get => ToNumericalSize(this.Size);
            }

            /// <summary>
            /// Gets or sets the theme component of the icon name.
            /// </summary>
            public IconTheme Theme
            {
                get => this._Theme;
                set => this._Theme = value;
            }

            /// <summary>
            /// Gets or sets the format of the icon name.
            /// </summary>
            public NamingFormat Format
            {
                get => this._Format;
                set => this._Format = value;
            }

            /// <summary>
            /// Sets the icon name and extracts all components.
            /// </summary>
            /// <param name="name">The full name of the icon.</param>
            public void SetName(string name)
            {
                string workingName = name;

                // Must process formats as below:
                //
                // iOS                  Android
                // caretUp12Filled      ic_fluent_caret_up_12_filled
                // caretUp16Filled      ic_fluent_caret_up_16_filled
                // caretUp20Filled      ic_fluent_caret_up_20_filled
                // caretUp24Filled      ic_fluent_caret_up_24_filled
                //
                // callPark16Regular    ic_fluent_call_park_16_regular
                // callPark20Regular    ic_fluent_call_park_20_regular
                // callPark24Regular    ic_fluent_call_park_24_regular
                // callPark28Regular    ic_fluent_call_park_28_regular
                // callPark32Regular    ic_fluent_call_park_32_regular
                // callPark48Regular    ic_fluent_call_park_48_regular

                // Extract format
                if (workingName.StartsWith("ic_fluent_"))
                {
                    this.Format = NamingFormat.Android;
                    workingName = workingName.Substring("ic_fluent_".Length);
                }
                else
                {
                    this.Format = NamingFormat.iOS;
                }

                string filledPattern1 = "_filled";
                string filledPattern2 = "Filled";
                string regularPattern1 = "_regular";
                string regularPattern2 = "Regular";

                // Extract theme
                if (workingName.EndsWith(filledPattern1, StringComparison.OrdinalIgnoreCase))
                {
                    this.Theme = IconTheme.Filled;
                    workingName = workingName.Substring(0, workingName.Length - filledPattern1.Length);
                }
                else if (workingName.EndsWith(filledPattern2, StringComparison.OrdinalIgnoreCase))
                {
                    this.Theme = IconTheme.Filled;
                    workingName = workingName.Substring(0, workingName.Length - filledPattern2.Length);
                }
                else if (workingName.EndsWith(regularPattern1, StringComparison.OrdinalIgnoreCase))
                {
                    this.Theme = IconTheme.Regular;
                    workingName = workingName.Substring(0, workingName.Length - regularPattern1.Length);
                }
                else if (workingName.EndsWith(regularPattern2, StringComparison.OrdinalIgnoreCase))
                {
                    this.Theme = IconTheme.Regular;
                    workingName = workingName.Substring(0, workingName.Length - regularPattern2.Length);
                }
                else
                {
                    // Default
                    this.Theme = IconTheme.Regular;
                }

                // Trim underscores
                if (workingName.EndsWith("_"))
                {
                    workingName = workingName.Substring(0, workingName.Length - 1);
                }

                // Extract size
                if (workingName.EndsWith("12", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size12;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("16", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size16;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("20", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size20;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("24", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size24;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("28", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size28;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("32", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size32;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else if (workingName.EndsWith("48", StringComparison.OrdinalIgnoreCase))
                {
                    this.Size = IconSize.Size48;
                    workingName = workingName.Substring(0, workingName.Length - 2);
                }
                else
                {
                    // Default
                    this.Size = IconSize.Size12;
                }

                // Trim underscores
                if (workingName.StartsWith("_"))
                {
                    workingName = workingName.Substring(1);
                }

                if (workingName.EndsWith("_"))
                {
                    workingName = workingName.Substring(0, workingName.Length - 1);
                }

                // Whatever is left is the base name
                this.BaseName = workingName;

                return;
            }

            /// <summary>
            /// Gets a full icon name built from individual components.
            /// </summary>
            /// <returns>The full name of the icon.</returns>
            public string GetName(
                string baseName,
                IconSize size,
                IconTheme theme,
                NamingFormat format)
            {
                StringBuilder sb = new StringBuilder();

                switch (format)
                {
                    case NamingFormat.Android:
                        {
                            if (baseName.StartsWith("ic_fluent_") == false)
                            {
                                sb.Append("ic_fluent_");
                            }

                            sb.Append(baseName);
                            sb.Append("_");
                            sb.Append(FluentUISystem.ToNumericalSize(size).ToString(CultureInfo.InvariantCulture));
                            sb.Append("_");

                            switch (theme)
                            {
                                case IconTheme.Filled:
                                    sb.Append("filled");
                                    break;
                                case IconTheme.Regular:
                                    sb.Append("regular");
                                    break;
                            }

                            // Failsafe
                            sb.Replace("__", "_");

                            return sb.ToString();
                        }
                    case NamingFormat.iOS:
                        {
                            sb.Append(baseName);
                            sb.Append(FluentUISystem.ToNumericalSize(size).ToString(CultureInfo.InvariantCulture));

                            switch (theme)
                            {
                                case IconTheme.Filled:
                                    sb.Append("Filled");
                                    break;
                                case IconTheme.Regular:
                                    sb.Append("Regular");
                                    break;
                            }

                            return sb.ToString();
                        }
                }

                return string.Empty;
            }

            /// <inheritdoc/>
            public override string ToString()
            {
                return this.BaseName + " | " + this.Size + " | " + this.Theme;
            }
        }
    }
}
