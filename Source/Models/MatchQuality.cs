namespace IconManager
{
    /// <summary>
    /// Defines the quality of a metaphor or glyph match between two icons.
    /// </summary>
    public enum MatchQuality
    {
        /// <summary>
        /// There is no discernible match between two icons.
        /// </summary>
        NoMatch = 0,

        /// <summary>
        /// Two icons are loosely similar in metaphor or glyph and there are significant
        /// differences. The user may have difficulty recognizing the similarity.
        /// </summary>
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
        High,

        /// <summary>
        /// Two icons are exactly the same in metaphor or glyph.
        /// The user can perfectly recognize the similarity.
        /// </summary>
        Exact
    }
}
