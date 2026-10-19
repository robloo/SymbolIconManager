using Avalonia.Markup.Xaml;
using System;
using Strings = IconManager.Resources.Strings;

namespace IconManager
{
    /// <summary>
    /// Markup extension to provide a translated, localized text string for the given key.
    /// </summary>
    public class LocalizeExtension : MarkupExtension
    {
        /***************************************************************************************
         *
         * Constructor
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizeExtension"/> class.
        /// </summary>
        public LocalizeExtension()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizeExtension"/> class.
        /// </summary>
        /// <param name="key">The key used to lookup the translated/localized string.</param>
        public LocalizeExtension(string key)
        {
            this.Key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizeExtension"/> class.
        /// </summary>
        /// <param name="key">The key used to lookup the translated/localized string.</param>
        /// <param name="fallbackValue">The fallback value to return when the key isn't found.</param>
        public LocalizeExtension(
            string key,
            string fallbackValue)
        {
            this.Key = key;
            this.FallbackValue = fallbackValue;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets or sets the key used to lookup the translated/localized string.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the optional fallback value to return when no
        /// translated/localized string is found for the given key.
        /// </summary>
        public string? FallbackValue { get; set; } = null;

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <inheritdoc/>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            // In release mode be forgiving of lookup failures and return the fallback value
            // In debug mode we want to throw an exception to find bad keys
#if !DEBUG
            try
            {
#endif
                string? text = Strings.Resources.ResourceManager.GetString(this.Key, Strings.Resources.Culture);

                return string.IsNullOrEmpty(text)
                    ? this.GetFallbackValue() ?? string.Empty
                    : text;
#if !DEBUG
            }
            catch
            {
                return this.FallbackValue ?? string.Empty;
            }
#endif
        }

        /// <summary>
        /// Gets the fallback value to return when no localized value was found matching the given key.
        /// </summary>
        /// <returns>The fallback value.</returns>
        private string? GetFallbackValue()
        {
            // Note: Only null is checked here
            // Consumers are allowed to provide an empty string to avoid the default fallback values
            if (this.FallbackValue is not null)
            {
                return this.FallbackValue;
            }

            if (!string.IsNullOrEmpty(this.Key))
            {
                return $"<{this.Key}>";
            }

            return "<missing>";
        }
    }
}
