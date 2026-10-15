using IconManager.Core.Icons;
using System;
using System.Collections.Generic;
using System.Linq;

namespace IconManager.Core.Glyphs
{
    /// <summary>
    /// Contains a collection of <see cref="IGlyphSourceProvider"/>s registered by their supported <see cref="IconSet"/>s.
    /// The specific <see cref="IGlyphSourceProvider"/> will be automatically resolved internally for a specified <see cref="IconSet"/>.
    /// </summary>
    public class GlyphSourceProviderCollection : IGlyphSourceProvider
    {
        private Dictionary<IconSet, IGlyphSourceProvider> _glyphSourceProvider = new();

        /***************************************************************************************
         *
         * Public Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Registers the given <see cref="IGlyphSourceProvider"/> in the collection.
        /// The given provider will override any existing ones based on supported <see cref="IconSet"/>s.
        /// </summary>
        /// <param name="glyphSourceProvider">The <see cref="IGlyphSourceProvider"/> to register.</param>
        public void Register(IGlyphSourceProvider glyphSourceProvider)
        {
            if (glyphSourceProvider is null)
            {
                return;
            }

            var iconSets = glyphSourceProvider.GetIconSets();

            foreach (IconSet iconSet in iconSets)
            {
                this._glyphSourceProvider[iconSet] = glyphSourceProvider;
            }

            return;
        }

        /// <inheritdoc/>
        public List<IconSet> GetIconSets()
        {
            return this._glyphSourceProvider.Keys.ToList();
        }

        /// <inheritdoc/>
        public List<GlyphSource> GetGlyphSources(IconSet iconSet, uint unicodePoint)
        {
            return this.FindProvider(iconSet)?.GetGlyphSources(iconSet, unicodePoint) ?? new List<GlyphSource>();
        }

        /// <inheritdoc/>
        public Uri? GetGlyphSourceUri(IconSet iconSet, uint unicodePoint, GlyphSource glyphSource)
        {
            return this.FindProvider(iconSet)?.GetGlyphSourceUri(iconSet, unicodePoint, glyphSource);
        }

        /// <inheritdoc/>
        public Uri? GetFontSourceUri(IconSet iconSet)
        {
            return this.FindProvider(iconSet)?.GetFontSourceUri(iconSet);
        }

        /***************************************************************************************
         *
         * Private Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Finds any internally registered <see cref="IGlyphSourceProvider"/> for the given <see cref="IconSet"/>.
        /// </summary>
        /// <param name="iconSet">The <see cref="IconSet"/> to find the registered glyph source provider for.</param>
        /// <returns>The registered <see cref="IGlyphSourceProvider"/>; otherwise, null.</returns>
        private IGlyphSourceProvider? FindProvider(IconSet iconSet)
        {
            if (this._glyphSourceProvider.TryGetValue(iconSet, out IGlyphSourceProvider? provider))
            {
                return provider;
            }

            return null;
        }
    }
}
