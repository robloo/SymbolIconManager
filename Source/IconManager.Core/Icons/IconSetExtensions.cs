namespace IconManager.Core.Icons
{
    /// <summary>
    /// Contains extension methods useful when working with <see cref="IconSet"/>s.
    /// </summary>
    public static class IconSetExtensions
    {
        /// <summary>
        /// Gets the <see cref="IconSetFamily"/> corresponding to the <see cref="IconSet"/>.
        /// </summary>
        /// <param name="iconSet">The <see cref="IconSet"/> to get the family for.</param>
        /// <returns>
        /// The <see cref="IconSetFamily"/> corresponding to the <see cref="IconSet"/>;
        /// otherwise, <see cref="IconSetFamily.Undefined"/>.
        /// </returns>
        public static IconSetFamily GetFamily(this IconSet iconSet)
        {
            switch (iconSet)
            {
                case IconSet.FluentUISystemFilled:
                case IconSet.FluentUISystemRegular:
                    return IconSetFamily.FluentUISystem;
                case IconSet.LineAwesomeBrand:
                case IconSet.LineAwesomeRegular:
                case IconSet.LineAwesomeSolid:
                    return IconSetFamily.LineAwesome;
                case IconSet.SegoeFluent:
                    return IconSetFamily.SegoeFluent;
                case IconSet.SegoeMDL2Assets:
                    return IconSetFamily.SegoeMDL2Assets;
                case IconSet.SegoeUISymbol:
                    return IconSetFamily.SegoeUISymbol;
                case IconSet.WinJSSymbols:
                    return IconSetFamily.WinJSSymbols;
                default:
                    return IconSetFamily.Undefined;
            }
        }
    }
}
