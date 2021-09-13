namespace IconManager
{
    /// <summary>
    /// Defines the quality of a metaphor or glyph match between two icons.
    /// </summary>
    public enum MatchQuality
    {
        /// <summary>
        /// There is no match between two icons.
        /// </summary>
        /// <remarks>
        /// Generally 'NoMatch' should not be used and instead no association (mapping)
        /// between two such icons should be made.
        /// </remarks>
        NoMatch = 0,

        /// <summary>
        /// Two icons are loosely similar in metaphor or glyph and there are significant
        /// differences. The user may have difficulty recognizing the similarity.
        /// </summary>
        /// <remarks>
        /// Significant differences are common if a substitution was made.
        /// These may be good candidates to retrieve from another icon set instead.
        /// </remarks>
        Low,

        /// <summary>
        /// Two icons are moderately similar in metaphor or glyph.
        /// The user can usually recognize the similarity.
        /// </summary>
        Medium,

        /// <summary>
        /// Two icons have a very high degree of similarity in metaphor or glyph.
        /// The user can perfectly recognize the similarity.
        /// </summary>
        /// <remarks>
        /// Example 1: One icon has rounded edges and the other square, but all
        ///            other characteristics in the glyph are the same.
        /// Example 2: One glyph is slightly larger or smaller than the other.
        /// </remarks>
        High,

        /// <summary>
        /// Two icons are exactly the same in metaphor or glyph.
        /// The user can perfectly recognize the similarity.
        /// </summary>
        /// <remarks>
        /// Exact means there are absolutely no discernible differences between two icons.
        /// This value is very rarely used.
        /// </remarks>
        Exact
    }
}
