﻿namespace IconManager
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
        /// Family: FluentUISystem.
        /// https://github.com/microsoft/fluentui-system-icons
        /// </summary>
        FluentUISystemFilled,

        /// <summary>
        /// The cross-platform Fluent UI System icons with regular theme.
        /// Family: FluentUISystem.
        /// https://github.com/microsoft/fluentui-system-icons
        /// </summary>
        FluentUISystemRegular,

        /// <summary>
        /// The cross-platform, open-source Line Awesome icons with company brand style.
        /// Family: LineAwesome.
        /// https://github.com/icons8/line-awesome
        /// </summary>
        LineAwesomeBrand,

        /// <summary>
        /// The cross-platform, open-source Line Awesome icons with regular style.
        /// Family: LineAwesome.
        /// https://github.com/icons8/line-awesome
        /// </summary>
        LineAwesomeRegular,

        /// <summary>
        /// The cross-platform, open-source Line Awesome icons with solid fill style.
        /// Family: LineAwesome.
        /// https://github.com/icons8/line-awesome
        /// </summary>
        LineAwesomeSolid,

        /// <summary>
        /// The Windows 10/11 Segoe Fluent icons (Windows symbols version 3).
        /// https://docs.microsoft.com/en-us/windows/apps/design/signature-experiences/iconography#system-icons
        /// </summary>
        SegoeFluent,

        /// <summary>
        /// The Windows 10 Segoe MDL2 Assets icons (Windows symbols version 2).
        /// https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font
        /// </summary>
        SegoeMDL2Assets,

        /// <summary>
        /// The Windows 8 Segoe UI Symbol icons (Windows symbols version 1).
        /// </summary>
        SegoeUISymbol,

        /// <summary>
        /// An open source icon font fallback to Microsoft's <see cref="SegoeUISymbol"/> and
        /// <see cref="SegoeMDL2Assets"/> that was provided by the Microsoft WinJS team.
        /// https://github.com/microsoft/fonts
        /// https://github.com/winjs/winjs/tree/master/src/fonts
        /// </summary>
        WinJSSymbols,
    }
}
