namespace IconManager
{
    /// <summary>
    /// Defines a named family of icon sets.
    /// </summary>
    public enum IconSetFamily
    {
        /// <inheritdoc cref="IconSet.Undefined"/>
        Undefined = 0,

        /// <summary>
        /// The cross-platform Fluent UI System icons.
        /// </summary>
        /// <remarks>
        /// https://github.com/microsoft/fluentui-system-icons.
        /// Includes both <see cref="IconSet.FluentUISystemRegular"/> and
        /// <see cref="IconSet.FluentUISystemFilled"/>.
        /// </remarks>
        FluentUISystem,

        /// <summary>
        /// The cross-platform, open-source Line Awesome icons.
        /// </summary>
        /// <remarks>
        /// https://github.com/icons8/line-awesome.
        /// </remarks>
        LineAwesome,

        /// <summary>
        /// The Windows 10/11 Segoe Fluent icons (Windows symbols version 3).
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-fluent-icons-font.
        /// </remarks>
        SegoeFluent,

        /// <summary>
        /// The Windows 10 Segoe MDL2 Assets icons (Windows symbols version 2).
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font.
        /// </remarks>
        SegoeMDL2Assets,

        /// <summary>
        /// The Windows 8 Segoe UI Symbol icons (Windows symbols version 1).
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/previous-versions/windows/apps/jj841127(v=win.10).
        /// </remarks>
        SegoeUISymbol,

        /// <summary>
        /// An open source icon font fallback to Microsoft's <see cref="SegoeUISymbol"/> and
        /// <see cref="SegoeMDL2Assets"/> that was provided by the Microsoft WinJS team.
        /// </summary>
        /// <remarks>
        /// https://github.com/microsoft/fonts;
        /// https://github.com/winjs/winjs/tree/master/src/fonts.
        /// </remarks>
        WinJSSymbols,
    }
}
