using IconManager.Core.Icons;
using System;
using System.Collections.Generic;

namespace IconManager
{
    /// <summary>
    /// Contains a list of icon mappings along with methods to process them.
    /// </summary>
    public class IconMappingList : List<IconMapping>
    {
        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Merges any mappings from this list into the destination.
        /// Any matching mappings in the destination will be replaced by information in the source.
        /// Any mappings that exist in the source but not the destination will be added to the destination.
        /// </summary>
        /// <param name="destMappings">The destination mappings list to merge into.</param>
        public void MergeInto(IconMappingList destMappings)
        {
            foreach (IconMapping sourceMapping in this)
            {
                bool matchFound = false;
                foreach (IconMapping destMapping in destMappings)
                {
                    if (sourceMapping.Destination.IsUnicodeMatch(destMapping.Destination))
                    {
                        // Only copy over the source if it is valid
                        if (sourceMapping.Source.IsValidForSource)
                        {
                            destMapping.Source               = sourceMapping.Source.Clone();
                            destMapping.GlyphMatchQuality    = sourceMapping.GlyphMatchQuality;
                            destMapping.MetaphorMatchQuality = sourceMapping.MetaphorMatchQuality;
                            destMapping.IsPlaceholder        = sourceMapping.IsPlaceholder;
                            // Exclude comments intentionally
                        }

                        matchFound = true;
                        // Do not break, update all duplicates (Yes, this is slow)
                    }
                }

                if (matchFound == false)
                {
                    destMappings.Add(sourceMapping.Clone());
                }
            }

            return;
        }

        /// <summary>
        /// Finds all mappings in the list that match the given destination name.
        /// </summary>
        /// <param name="name">The name of the destination icon to search for.</param>
        /// <param name="ignoreCase">Whether to ignore character case of the name.</param>
        /// <returns>A list of all matching icons.</returns>
        public List<IconMapping> FindByDestinationName(string name, bool ignoreCase = true)
        {
            List<IconMapping> result = new List<IconMapping>();

            foreach (IconMapping mapping in this)
            {
                if (ignoreCase &&
                    string.Equals(mapping.Destination.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(mapping);
                }
                else if (string.Equals(mapping.Destination.Name, name, StringComparison.Ordinal))
                {
                    result.Add(mapping);
                }
            }

            return result;
        }

        /// <summary>
        /// Finds all mappings in the list that match the given destination Unicode point.
        /// </summary>
        /// <param name="unicodePoint">The Unicode point of the destination icon to search for..</param>
        /// <returns>A list of all matching icons.</returns>
        public List<IconMapping> FindByDestinationUnicode(uint unicodePoint)
        {
            List<IconMapping> result = new List<IconMapping>();

            foreach (IconMapping mapping in this)
            {
                if (mapping.Destination.UnicodePoint == unicodePoint)
                {
                    result.Add(mapping);
                }
            }

            return result;
        }

        /// <summary>
        /// Finds all mappings in the list that match the given source name.
        /// </summary>
        /// <param name="name">The name of the source icon to search for.</param>
        /// <param name="ignoreCase">Whether to ignore character case of the name.</param>
        /// <returns>A list of all matching icons.</returns>
        public List<IconMapping> FindBySourceName(string name, bool ignoreCase = true)
        {
            List<IconMapping> result = new List<IconMapping>();

            foreach (IconMapping mapping in this)
            {
                if (ignoreCase &&
                    string.Equals(mapping.Source.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    result.Add(mapping);
                }
                else if (string.Equals(mapping.Source.Name, name, StringComparison.Ordinal))
                {
                    result.Add(mapping);
                }
            }

            return result;
        }

        /// <summary>
        /// Finds all mappings in the list that match the given source Unicode point.
        /// </summary>
        /// <param name="unicodePoint">The Unicode point of the source icon to search for..</param>
        /// <returns>A list of all matching icons.</returns>
        public List<IconMapping> FindBySourceUnicode(uint unicodePoint)
        {
            List<IconMapping> result = new List<IconMapping>();

            foreach (IconMapping mapping in this)
            {
                if (mapping.Source.UnicodePoint == unicodePoint)
                {
                    result.Add(mapping);
                }
            }

            return result;
        }

        /// <summary>
        /// Sort each mapping alphabetically by source name.
        /// </summary>
        public void SortBySourceName()
        {
            this.Sort((x, y) =>
            {
                return string.Compare(x.Source.Name, y.Source.Name);
            });

            return;
        }

        /// <summary>
        /// Sort each mapping by source icon set and Unicode point.
        /// </summary>
        public void SortBySourceUnicode()
        {
            this.Sort((x, y) =>
            {
                if (x.Source.IconSet == y.Source.IconSet)
                {
                    return x.Source.UnicodePoint.CompareTo(y.Source.UnicodePoint);
                }
                else
                {
                    return x.Source.IconSet.CompareTo(y.Source.IconSet);
                }
            });

            return;
        }

        /// <summary>
        /// Sort each mapping alphabetically by destination name.
        /// </summary>
        public void SortByDestinationName()
        {
            this.Sort((x, y) =>
            {
                return string.Compare(x.Destination.Name, y.Destination.Name);
            });

            return;
        }

        /// <summary>
        /// Sort each mapping by destination icon set and Unicode point.
        /// </summary>
        public void SortByDestinationUnicode()
        {
            this.Sort((x, y) =>
            {
                if (x.Destination.IconSet == y.Destination.IconSet)
                {
                    return x.Destination.UnicodePoint.CompareTo(y.Destination.UnicodePoint);
                }
                else
                {
                    return x.Destination.IconSet.CompareTo(y.Destination.IconSet);
                }
            });

            return;
        }
    }
}
