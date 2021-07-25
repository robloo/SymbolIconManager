# Icon Set Data

This folder contains raw icon set data files used by the application. Data is commonly gathered from third party sources. A individual data files is given in the below table.

Note the following:
 * Where applicable, JSON is the preferred format for all data.
 * JSON does not support comments; therefore, file information (including last update) is tracked here.
 * Data is copied even if it is publicly available elsewhere. This is a failsafe if ever the source goes offline; however, this requires maintenence to keep in sync.

| Icon Set | Data File | Last Update | Last Version | Source |
|----------|-----------|-------------|--------------|--------|
| Fluent UI System  | `FluentSystemIcons-Filled.json`  | 2021-Jul-20 | 1.1.135 | [GitHub Repo](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Filled.json) |
| Fluent UI System  | `FluentSystemIcons-Filled.ttf`   | 2021-Jul-24 | 1.1.135 | [GitHub Rego](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Filled.ttf) |
| Fluent UI System  | `FluentSystemIcons-Regular.json` | 2021-Jul-20 | 1.1.135 | [GitHub Repo](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Regular.json) |
| Fluent UI System  | `FluentSystemIcons-Regular.ttf`  | 2021-Jul-24 | 1.1.135 | [GitHub Repo](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Regular.ttf) |
| Segoe MDL2 Assets | `SegoeMDL2Assets.json`           | 2021-Jul-18 | -       | [Microsoft Website](https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font) |
| WinJS Symbols     | `Symbols.json`                   | 2021-Jul-24 | 1.05    | [GitHub PDF](https://github.com/winjs/winjs/blob/master/src/fonts/SymbolsWinJS.pdf) |
| WinJS Symbols     | `Symbols.json`                   | 2021-Jul-24 | 1.05    | [GitHub Repo](https://github.com/winjs/winjs/blob/master/src/fonts/Symbols.ttf) |


## *Fluent UI System* Icon Data | Microsoft | MIT License

 * [GitHub Repo](https://github.com/microsoft/fluentui-system-icons)
 * [Website](https://developer.microsoft.com/en-us/fluentui#/)

#### JSON Files

The Fluent UI System icons come in two themes: regular and filled. Each of these themes has their own font and JSON file:

 1. `FluentSystemIcons-Filled.json` : Contains a list of all filled theme Unicode points and names (key).
 2. `FluentSystemIcons-Regular.json` : Contains a list of all regular theme Unicode points and names (key).

The first column (key) is the icon name and the second is the Unicode point. For the purpose of data processing these two files are commonly merged together.

#### Unicode Points

This application uses the *Fluent UI System* Unicode points defined in JSON files. These files are updated externally (on GitHub) and copied verbatim here for further use.

#### Glyphs

*Fluent UI System* icon glyphs (SVG files) are available in GitHub under the MIT license. Due to file size, this application does not distribute these source glyphs. Glyphs are downloaded on demand directly from the GitHub repository. The open source (MIT licensed) .ttf files are used to render glyphs in text.

## *Segoe MDL2 Assets* Icon Data | Microsoft | Proprietary (Windows-only)

 * [Website](https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font)

#### JSON File

 1. `SegoeMDL2Assets.json` : Contains a list of all Unicode points (key) and their description/name.

The first column (key) is the Unicode point and the second is the description.

#### Unicode Points

Use of Unicode points and descriptions is for the purpose of interoperability only. The Unicode points publicly available by Microsoft are manually entered into the `SegoeMDL2Assets.json` file.

#### Glyphs

Icon glyphs are not open source and can only be used on Microsoft's Windows operating systems. Glyphs are not available from or distributed by this application and are the exclusive property of Microsoft. Glyphs are; however, hosted by Microsoft online which is what is used when displayed in this application.
 * Glyph URL example: https://docs.microsoft.com/en-us/windows/apps/design/style/images/segoe-mdl/{UnicodePoint}.png

## WinJS *Symbols* Icon Data | Microsoft | MIT License

 * [GitHub Repo](https://github.com/winjs/winjs/tree/master/src/fonts)

The source data is in maintence mode and hasn't been changed in seven years (as of 2021). WinJS itself has been deprecated. Therefore, this data is not expected to need updates in the future.

#### JSON File

 1. `Symbols.json` : Contains a list of all Unicode points (key) and their description/name

The first column (key) is the Unicode point and the second is the description.

#### Unicode Points

This application uses the *WinJS Symbols* Unicode points defined in the source PDF hosted on GitHub. The PDF is converted into a table and then manually merged to create the source JSON data.

#### Glyphs

The open source (MIT licensed) .ttf file is used to render glyphs in text.

# Mapping Data

TODO
