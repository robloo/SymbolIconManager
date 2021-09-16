# Symbol Icon Manager

The Symbol Icon Manager is an application written to facilitate working with Microsoft's various symbol icon fonts. It's most widely-useful tool is a font builder that allows mapping glyphs from a source icon set into a destination icon set (by Unicode point). In practice this means it is possible to build a free and open source variant of Microsoft's proprietary fonts (*Segoe MDL2 Assets* and *Segoe Fluent Icons*) that can be used as a drop-in replacement for apps that need it cross-platform. Using Microsoft's fonts in cross-platform projects directly is not possible due to licensing.

In other words, the primary purpose of this tool is to build alternatives for Microsoft's symbol icon fonts for both the Avalonia and Uno Platform cross-platform UI frameworks.

**Scope**

This project is currently focusing on Microsoft's symbol icon fonts including *Segoe MDL2 Assets* and *Segoe Fluent Icons*. Other symbol icon sets are used only to provide equivalent glyphs for Microsoft's already defined Unicode points. Symbol icons for other frameworks or platforms are currently out of scope.

## App Functionality

This application has the following planned functionality:

 1. **Icon Mapping & Font Builder**
     * Provides tools to facilitate making and reviewing icon set mapping files (by Unicode point/Glyph).
     * Provides a tool to build cross-platform, open-source variants of Microsoft's Symbol fonts using icon mapping files.
 1. **Application Tools**
     * Provides a tool to list all symbol icons used in an app's source code (C#/XAML files).
     * Provides a tool to remap icon Unicode points from one symbol icon set to another. This is done by replacing all occurances of one Unicode point with another (usually in a FontIcon's Glyph property). For example, this can be used to switch an application's source code from using *Segoe MDL2 Assets* to *Fluent UI System Icons*. 
     * [Planned] Provides a tool to build an application-specific custom symbol font containing only the glyphs actually needed by the application's source code (C#/XAML). Fonts are quite large (commonly around 1MB) and some apps may not want to package the whole thing. This tool significantly reduces the font file size.

Note that this application was originally an internal tool written to migrate a C#/UWP codebase from *Segoe MDL2 Assets* to the cross-platform *Fluent UI System Icons*.

Ideas for future consideration:
 * Work with XAML path data for icons directly instead of through icon fonts. This can simplify usage (and file size) in some applications and scenarios. Think of this as a feature to convert FontIcons into PathIcons.

## Contributing

Anyone can contribute by submitting a PR or opening an issue (especially for missing mappings). There are well over a thousand icons in any given symbol icon set (font) so everyone is encouraged to add the mappings that are important for their applications. Contributions are very welcome!

For more instructions on how to modify icon mapping files see [Icon Mapping](https://github.com/robloo/SymbolIconManager/blob/main/Docs/IconMapping.md).

## Third-Party Resources

This application is possible thanks to the work of others:

 * [Avalonia UI](https://www.avaloniaui.net/) and [Avalonia.NameGenerator](https://github.com/AvaloniaUI/Avalonia.NameGenerator) provides the UI framework
 * [SkiaSharp](https://github.com/mono/SkiaSharp) provides the library used to render glyphs to images
 * [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) which is still the best C# JSON serialization library doing things `System.Text.Json` can't
 * Also see the [Data ReadMe](https://github.com/robloo/SymbolIconManager/tree/main/Source/Data#readme) for a list of third-party data used by the app

Significant credit goes to Microsoft for open sourcing the symbol icon fonts and glyph sources that make most of this possible. This includes the older WinJS *Symbols* font as well as the newer *Fluent UI System Icons* which share a design language with current *Segoe Fluent Icons*.

Special thanks to the Uno Platform for making a great cross-platform UI framework. Use of the Uno Platform drove the creation of this repository due to the need for a cross-platform symbol font variant matching the new *Segoe Fluent Icons* style.
