# Symbol Icon Manager

The Symbol Icon Manager is an application written to facilitate working with Microsoft's various symbol icon fonts. It's most widely-useful tool is a font builder that allows mapping glyphs from a source icon set into a destination icon set (by Unicode point). In practice this means it is possible to build a free and open source variant of Microsoft's proprietary fonts that can be used as a drop-in replacement for apps that need it cross-platform (especially those that already use Microsoft's symbol icons). Using Microsoft's fonts in these projects directly is not possible due to licensing.

In other words, the primary purpose of this tool is to build alternatives for Microsoft's symbol icon fonts for both the Avalonia and Uno Platform cross-platform UI frameworks.

## App Functionality

This application has the following planned functionality:

 1. **Font Builder**
     * [Planned] Provides a tool to build cross-platform, open-source variants of Microsoft's Symbol fonts
 3. **Icon Mapping**
     * [Planned] Provides tools to facilitate making and reviewing icon set mapping files (by Unicode point/Glyph).
 5. **Application Tools**
     * Provides a tool to list all symbol icons used in an app's source code (C#/XAML files).
     * Provides a tool to remap icon Unicode points from one symbol icon set to another. This is done by replacing all occurances of one Unicode point with another (usually in a FontIcon's Glyph property). For example, this can be used to switch an application's source code from using *Segoe MDL2 Assets* to *Fluent UI System Icons*.

Note that application was originally an internal tool written to migrate a C#/UWP codebase from *Segoe MDL2 Assets* to the cross-platform *Fluent UI System Icons*.

## Third-Party Resources

This application is possible thanks to the work of others:

 * [Avalonia UI](https://www.avaloniaui.net/) and [Avalonia.NameGenerator](https://github.com/AvaloniaUI/Avalonia.NameGenerator)
 * Also see the [Data ReadMe](https://github.com/robloo/SymbolIconManager/tree/main/Source/Data#readme) for a list of third-party data used by the app

Significant credit goes to Microsoft for open sourcing the icon fonts that make most of this possible. This includes the older WinJS *Symbols* font as well as the newer *Fluent UI System Icons* which share a design language with current *Segoe Fluent Icons*.
