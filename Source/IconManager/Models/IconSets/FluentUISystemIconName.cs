using System;
using System.Globalization;
using System.Text;
using static IconManager.FluentUISystem;

namespace IconManager
{
    /// <summary>
    /// Represents a single Fluent UI System icon name with all of
    /// its individual components.
    /// </summary>
    public class FluentUISystemIconName
    {
        private string       _BaseName = string.Empty;
        private IconSize     _Size     = IconSize.Size12;
        private IconTheme    _Theme    = IconTheme.Regular;
        private NamingFormat _Format   = NamingFormat.Android;

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentUISystemIconName"/> class.
        /// </summary>
        public FluentUISystemIconName()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentUISystemIconName"/> class.
        /// </summary>
        /// <param name="name">The full name of the icon including all components.
        /// Example: ic_fluent_caret_up_24_filled.</param>
        public FluentUISystemIconName(string name)
        {
            this.SetName(name);
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

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
        /// Gets the base name in a universal format shared by both Android/iOS named formats.
        /// This may be used as a universal lookup key.
        /// </summary>
        public string BaseNameKey
        {
            get => FluentUISystemIconName.ToBaseNameKey(this._BaseName);
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
            get => (int)this.Size;
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

        /***************************************************************************************
         *
         * Public Methods
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        public override string ToString()
        {
            return this.BaseName + " | " + this.Size + " | " + this.Theme;
        }

        /// <summary>
        /// Converts the given base name into a universal format shared by both Android/iOS
        /// named formats. This may be used as a universal lookup key.
        /// </summary>
        /// <param name="baseName">The base name to get the universal formatted key for.</param>
        /// <returns>The universally formatted base name usable as a lookup key.</returns>
        public static string ToBaseNameKey(string baseName)
        {
            baseName = baseName.ToLowerInvariant();

            if (baseName.StartsWith("ic_fluent_"))
            {
                baseName = baseName.Substring("ic_fluent_".Length);
            }

            baseName = baseName
                .Replace(" ", string.Empty)
                .Replace("_", string.Empty);

            return baseName;
        }

        /// <summary>
        /// Detects the naming format of the given icon name.
        /// Only well-formed names will be detected properly; everything else will return null.
        /// </summary>
        /// <param name="name">The name to detect the format of.</param>
        /// <returns>The well-formed naming format; otherwise, null.</returns>
        public static NamingFormat? DetectFormat(string name)
        {
            string workingName = name.ToLowerInvariant().Trim();

            // iOS                  Android
            // caretUp12Filled      ic_fluent_caret_up_12_filled
            // callPark48Regular    ic_fluent_call_park_48_regular

            if (workingName.StartsWith("ic_fluent_") &&
                (workingName.EndsWith("_filled") || workingName.EndsWith("_regular")))
            {
                workingName = workingName.Substring("ic_fluent_".Length);
                workingName = workingName.Replace("_filled", string.Empty);
                workingName = workingName.Replace("_regular", string.Empty);

                if (workingName.Contains("_"))
                {
                    var sizeStr = workingName.Substring(workingName.LastIndexOf("_") + 1);
                    bool isSizeGiven = int.TryParse(sizeStr, out int size);

                    if (isSizeGiven)
                    {
                        if (Enum.IsDefined(typeof(IconSize), size))
                        {
                            return NamingFormat.Android;
                        }
                        else
                        {
                            // Currently allow even undefined icon sizes, just require a number
                            return NamingFormat.Android;
                        }
                    }
                }
            }
            else if (workingName.EndsWith("filled") || workingName.EndsWith("regular"))
            {
                // Allow anything
                return NamingFormat.iOS;
            }

            return null;
        }

        /***************************************************************************************
         *
         * Private Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Sets the icon name and extracts all components.
        /// </summary>
        /// <param name="name">The full name of the icon.</param>
        private void SetName(string name)
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
            var format = DetectFormat(workingName);
            if (format is not null)
            {
                this.Format = format.Value;
            }
            else
            {
                // Default to iOS which is harder to detect
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
        private string GetName(
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
                        sb.Append(((int)size).ToString(CultureInfo.InvariantCulture));
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
                        sb.Append(((int)size).ToString(CultureInfo.InvariantCulture));

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
    }
}
