using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IconManager
{
    /// <summary>
    /// Contains data and information for the Icons8 Line Awesome icons.
    /// </summary>
    public class LineAwesome : IconSetBase
    {
        /// <summary>
        /// Defines the style of a <see cref="LineAwesome"/> icon.
        /// </summary>
        public enum IconStyle
        {
            /// <summary>
            /// Company branding icons.
            /// </summary>
            /// <remarks>
            /// CSS class: 'lab'.
            /// </remarks>
            Brand,

            /// <summary>
            /// Regular styled icons.
            /// </summary>
            /// <remarks>
            /// CSS class: 'lar'.
            /// </remarks>
            Regular,

            /// <summary>
            /// Solid fill styled icons.
            /// </summary>
            /// <remarks>
            /// CSS class: 'las'.
            /// </remarks>
            Solid
        }

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Builds the full list of glyph sources for Line Awesome icons.
        /// Source files must already be downloaded and present in the cache.
        /// </summary>
        /// <remarks>
        /// This method is NOT intended for general-purpose use.
        /// It should only be used by those who know what they are doing to re-build the glyph sources.
        /// </remarks>
        public static void BuildGlyphSources()
        {
            List<string> glyphSources = new List<string>();
            string lineAwesomeCacheDirectory = @"line-awesome";

            string searchDirectory = Path.Combine(App.IconManagerCache, lineAwesomeCacheDirectory, "svg");

            foreach (string filePath in Directory.EnumerateFiles(searchDirectory, "*.*", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(filePath).ToUpperInvariant() == ".SVG")
                {
                    glyphSources.Add(filePath.Replace(searchDirectory, string.Empty));
                }
            }

            glyphSources.Sort();

            var jsonString = JsonSerializer.Serialize(
                glyphSources.ToArray(),
                new JsonSerializerOptions()
                {
                    WriteIndented = true
                });

            using (var fileStream = File.OpenWrite(Path.Combine(App.IconManagerCache, "LineAwesomeGlyphSources.json")))
            {
                fileStream.Write(Encoding.UTF8.GetBytes(jsonString));
            }

            return;
        }

        /***************************************************************************************
         *
         * Classes
         *
         ***************************************************************************************/

        /// <summary>
        /// Represents a single icon in the Line Awesome icon set.
        /// </summary>
        public class Icon : IIcon
        {
            /***************************************************************************************
             *
             * Property Accessors
             *
             ***************************************************************************************/

            ///////////////////////////////////////////////////////////
            // Data
            ///////////////////////////////////////////////////////////

            /// <inheritdoc/>
            public IconSet IconSet
            {
                get
                {
                    if (this.Style == IconStyle.Brand)
                    {
                        return IconSet.LineAwesomeBrand;
                    }
                    else if (this.Style == IconStyle.Solid)
                    {
                        return IconSet.LineAwesomeSolid;
                    }
                    else
                    {
                        return IconSet.LineAwesomeRegular;
                    }
                }
                set { /* Do nothing */ }
            }

            /// <inheritdoc/>
            public string Name { get; set; } = string.Empty;

            /// <summary>
            /// Gets or sets the Line Awesome style of the icon.
            /// </summary>
            public IconStyle Style { get; set; } = IconStyle.Regular;

            /// <inheritdoc/>
            public uint UnicodePoint { get; set; } = 0;

            ///////////////////////////////////////////////////////////
            // Calculated
            ///////////////////////////////////////////////////////////

            /// <inheritdoc/>
            public string UnicodeHexString
            {
                get => IconManager.Icon.ToUnicodeHexString(this.UnicodePoint);
            }

            /***************************************************************************************
             *
             * Methods
             *
             ***************************************************************************************/

            /// <summary>
            /// Creates a new <see cref="Icon"/> instance from this instance's values.
            /// </summary>
            /// <returns>The cloned <see cref="Icon"/>.</returns>
            public Icon Clone()
            {
                var clone = new Icon()
                {
                    Name         = this.Name,
                    Style        = this.Style,
                    UnicodePoint = this.UnicodePoint
                };

                return clone;
            }

            /// <summary>
            /// Converts this <see cref="LineAwesome.Icon"/> into a standard <see cref="IconManager.Icon"/>.
            /// This is sometimes needed because <see cref="LineAwesome.Icon"/> does not derive from
            /// <see cref="IconManager.Icon"/> like most other icons do. It only implements the interface.
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
    }
}
