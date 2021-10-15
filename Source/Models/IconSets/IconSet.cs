namespace IconManager
{
    /// <summary>
    /// Defines a named set of icons.
    /// Each icon within an icon set should have a unique Unicode point.
    /// </summary>
    /// <remarks>
    /// This is more generically worded as 'Set' instead of 'Font' as not all icon sets may be
    /// provided in a font.
    /// </remarks>
    public enum IconSet
    {
        /// <summary>
        /// No icon set is defined.
        /// This should only be used as a default value.
        /// </summary>
        Undefined = 0,

        /// <summary>
        /// The cross-platform Fluent UI System icons with filled theme.
        /// </summary>
        /// <remarks>
        /// Family: <see cref="IconSetFamily.FluentUISystem"/>.
        /// </remarks>
        FluentUISystemFilled,

        /// <summary>
        /// The cross-platform Fluent UI System icons with regular theme.
        /// </summary>
        /// <remarks>
        /// Family: <see cref="IconSetFamily.FluentUISystem"/>.
        /// </remarks>
        FluentUISystemRegular,

        /// <summary>
        /// The cross-platform, open-source Line Awesome icons with company brand style.
        /// </summary>
        /// <remarks>
        /// Family: <see cref="IconSetFamily.LineAwesome"/>.
        /// </remarks>
        LineAwesomeBrand,

        /// <summary>
        /// The cross-platform, open-source Line Awesome icons with regular style.
        /// </summary>
        /// <remarks>
        /// Family: <see cref="IconSetFamily.LineAwesome"/>.
        /// </remarks>
        LineAwesomeRegular,

        /// <summary>
        /// The cross-platform, open-source Line Awesome icons with solid fill style.
        /// </summary>
        /// <remarks>
        /// Family: <see cref="IconSetFamily.LineAwesome"/>.
        /// </remarks>
        LineAwesomeSolid,

        /// <summary>
        /// The Windows 10/11 Segoe Fluent icons (Windows symbols version 3).
        /// </summary>
        /// <remarks>
        /// Family: <see cref="IconSetFamily.SegoeFluent"/>.
        /// </remarks>
        SegoeFluent,

        /// <summary>
        /// The Windows 10 Segoe MDL2 Assets icons (Windows symbols version 2).
        /// </summary>
        /// <remarks>
        /// Family: <see cref="IconSetFamily.SegoeMDL2Assets"/>.
        /// </remarks>
        SegoeMDL2Assets,

        /// <summary>
        /// The Windows 8 Segoe UI Symbol icons (Windows symbols version 1).
        /// </summary>
        /// <remarks>
        /// Family: <see cref="IconSetFamily.SegoeUISymbol"/>.
        /// </remarks>
        SegoeUISymbol,

        /// <summary>
        /// An open source icon font fallback to Microsoft's <see cref="SegoeUISymbol"/> and
        /// <see cref="SegoeMDL2Assets"/> that was provided by the Microsoft WinJS team.
        /// </summary>
        /// <remarks>
        /// Family: <see cref="IconSetFamily.WinJSSymbols"/>.
        /// </remarks>
        WinJSSymbols,
    }
}
