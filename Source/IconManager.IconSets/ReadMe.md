# Icon Set Data

For information on icon mapping files see [Icon Mapping](https://github.com/robloo/SymbolIconManager/blob/main/Docs/IconMapping.md).

Note that the most important glyph sources (Fluent UI System and Line Awesome) are copied directly into this repository. This was not always the case; however, it became necessary due to the high number of breaking changes upstream. It was only possible to rebuild fonts for a short period of time until the build process would break and all source glyph data links and mappings would have to be updated to build fonts again.

This was not only unstable, but unmaintanable long-term. Therefore, all important glyph sources (SVG files) are now part of this repository. This ensures this repository alone can be cloned and fonts built exactly as they were before using local files reguardless of changes upstream. In sort, the font build process is now stable and fonts can still be built even if the source repositories go dark.

---

This folder contains raw icon set data files used by the application. Data is commonly gathered from third party sources. A list of individual data files is given in the below table.

| Icon Set Family | Data File | Last Update | Last Version | Source |
|-----------------|-----------|-------------|--------------|--------|
| Fluent UI System   | `FluentSystemIcons-Filled.json`  | 2023-Feb-05 | 1.1.193 | [GitHub Repo](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Filled.json) |
| Fluent UI System   | `FluentSystemIcons-Filled.ttf`   | 2023-Feb-05 | 1.1.193 | [GitHub Rego](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Filled.ttf) |
| Fluent UI System   | `FluentSystemIcons-Regular.json` | 2023-Feb-05 | 1.1.193 | [GitHub Repo](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Regular.json) |
| Fluent UI System   | `FluentSystemIcons-Regular.ttf`  | 2023-Feb-05 | 1.1.193 | [GitHub Repo](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Regular.ttf) |
| Line Awesome       | `la-brands-400.ttf`              | 2021-Sep-16 | 1.3.0   | [GitHub Repo](https://github.com/icons8/line-awesome/blob/master/dist/line-awesome/fonts/la-brands-400.ttf) |
| Line Awesome       | `la-regular-400.ttf`             | 2021-Sep-16 | 1.3.0   | [GitHub Repo](https://github.com/icons8/line-awesome/blob/master/dist/line-awesome/fonts/la-regular-400.ttf) |
| Line Awesome       | `la-solid-900.ttf`               | 2021-Sep-16 | 1.3.0   | [GitHub Repo](https://github.com/icons8/line-awesome/blob/master/dist/line-awesome/fonts/la-solid-900.ttf) |
| Line Awesome       | `line-awesome.css`               | 2021-Sep-16 | 1.3.0   | [GitHub Repo](https://github.com/icons8/line-awesome/blob/master/dist/line-awesome/css/line-awesome.css) |
| Segoe Fluent       | `SegoeFluentIcons.json`          | 2021-Sep-12 | -       | [Microsoft Website](https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-fluent-icons-font) |
| Segoe MDL2 Assets  | `SegoeMDL2Assets.json`           | 2021-Jul-18 | -       | [Microsoft Website](https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font) |
| WinJS Symbols      | `Symbols.json`                   | 2021-Jul-24 | 1.05    | [GitHub PDF](https://github.com/winjs/winjs/blob/master/src/fonts/SymbolsWinJS.pdf) |
| WinJS Symbols      | `Symbols.ttf`                    | 2021-Jul-24 | 1.05    | [GitHub Repo](https://github.com/winjs/winjs/blob/master/src/fonts/Symbols.ttf) |

Note the following:
 * Where applicable, JavaScript Object Notation (JSON) is the preferred format for all textual data.
 * Where applicable, TrueType Font (TTF) is the preferred format for all font data.
 * When possible, data file names should match the source -- including capitalization rules.
 * Data files do not support comments; therefore, file information (including last update) is tracked here.
 * Data (except glyph source images) is copied even if it is publicly available elsewhere. This is a failsafe if ever the source goes offline; however, this requires maintenance to keep in sync.
 * Glyph source image files are not currently copied into this project due to file size.

## *Fluent UI System* Icon Data | Microsoft | MIT License

 * [GitHub Repo](https://github.com/microsoft/fluentui-system-icons)
 * [Website](https://developer.microsoft.com/en-us/fluentui#/)

#### JSON Files

The *Fluent UI System* icons come in two themes: regular and filled. Each of these themes has their own font and JSON file:

 1. `FluentSystemIcons-Filled.json` : Contains a list of all filled theme Unicode points and names (key).
 2. `FluentSystemIcons-Regular.json` : Contains a list of all regular theme Unicode points and names (key).

The first column (key) is the icon name and the second is the Unicode point. For the purpose of data processing these two files are commonly merged together.

#### Unicode Points

This application uses the *Fluent UI System* Unicode points defined in JSON files. These files are updated externally (on GitHub) and copied verbatim here for further use.

#### Glyphs

*Fluent UI System* icon glyphs (SVG files) are available in GitHub under the MIT license. Due to file size, this application does not distribute these source glyphs. Glyphs are downloaded on demand directly from the GitHub repository. The open source (MIT licensed) .ttf files are used to render glyphs in text.

## *Line Awesome* Icon Data | Icons8 | MIT License

 * [GitHub Repo](https://github.com/icons8/line-awesome)
 * [Website](https://icons8.com/line-awesome)

 > Line Awesome is a free alternative for Font Awesome 5.11.2. It consists of ~1380 flat icons that offer complete coverage of the main Font Awesome icon set.
 >
 > This icon-font is based off of the Icons8 Windows 10 style...

#### JSON/CSS Files

All three styles of the *Line Awesome* icons share a single CSS file that defines the Unicode points and names for all supported icons. This file requires heavier processing to extract the needed information compared to JSON formatted files in other icon sets. Additionally, the CSS file processing is prone to error and mismatches with other data sources.

For this reason, a Python script is available that uses FontForge to build the list of all Unicode points and names from the TTF source itself. These files are much higher quality (as they are generated from the font) and are what is used in processing throughout the application. Always update the TTF font files then run the script to update these JSON files.

 1. `line-awesome.css` : Contains a CSS formatted list of all Unicode points and names for all three icon styles.
 2. `la-brands-400.json` : Contains a list of all Unicode points (key) and their corresponding name for the brands style.
 3. `la-regular-400.json` : Contains a list of all Unicode points (key) and their corresponding name for the regular style.
 4. `la-solid-900.json` : Contains a list of all Unicode points (key) and their corresponding name for the solid style.

In all JSON files the first column (key) is the Unicode point and the second is the name.

#### Unicode Points

This application uses the *Line Awesome* Unicode points defined in the CSS file. This file is updated externally (on GitHub) and copied verbatim here for further use.

#### Glyphs

*Line Awesome* icon glyphs (SVG files) are available in GitHub under the MIT license. Due to file size, this application does not distribute these source glyphs. Glyphs are downloaded on demand directly from the GitHub repository. The open source (MIT licensed) .ttf files are used to render glyphs in text.

## *Segoe Fluent Icons* Icon Data | Microsoft | Proprietary (Windows-only)

 * [Website](https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-fluent-icons-font)

*Segoe Fluent Icons* are proprietary and Windows-only according to the EULA. Propriety data such as icon glyphs are NOT redistributed here. Only Unicode points and names are used for the purposes of interoperability.

#### JSON File

 1. `SegoeFluentIcons.json` : Contains a list of all Unicode points (key) and their description/name.

The first column (key) is the Unicode point and the second is the description.

#### Unicode Points

Use of Unicode points and descriptions is for the purpose of interoperability only. The Unicode points publicly available by Microsoft are manually entered into the JSON file.

#### Glyphs

Icon glyphs are not open source and can only be used on Microsoft's Windows operating systems. Glyphs are not available from or distributed by this application and are the exclusive property of Microsoft. Glyphs are; however, hosted by Microsoft online which is what is used when displayed in this application.
 * Glyph URL example: https://docs.microsoft.com/en-us/windows/apps/design/style/images/glyphs/segoe-fluent-icons/{UnicodePoint}.png

Note that *Segoe Fluent Icons* are very close to *Fluent UI System* icons in glyph design. This similarity allows glyphs from the *Fluent UI System* to be used to rebuild a font very similar to *Segoe Fluent Icons*. 
 
## *Segoe MDL2 Assets* Icon Data | Microsoft | Proprietary (Windows-only)

 * [Website](https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font)

*Segoe MDL2 Assets* are proprietary and Windows-only. Propriety data such as icon glyphs are NOT redistributed here. Only Unicode points and names are used for the purposes of interoperability.

#### JSON File

 1. `SegoeMDL2Assets.json` : Contains a list of all Unicode points (key) and their description/name.

The first column (key) is the Unicode point and the second is the description.

#### Unicode Points

Use of Unicode points and descriptions is for the purpose of interoperability only. The Unicode points publicly available by Microsoft are manually entered into the JSON file.

#### Glyphs

Icon glyphs are not open source and can only be used on Microsoft's Windows operating systems. Glyphs are not available from or distributed by this application and are the exclusive property of Microsoft. Glyphs are; however, hosted by Microsoft online which is what is used when displayed in this application.
 * Glyph URL example: https://docs.microsoft.com/en-us/windows/apps/design/style/images/segoe-mdl/{UnicodePoint}.png

## WinJS *Symbols* Icon Data | Microsoft | MIT License

 * [GitHub Repo](https://github.com/winjs/winjs/tree/master/src/fonts)

The source data is in maintenance mode and hasn't been changed in seven years (as of 2021). WinJS itself has been deprecated. Therefore, this data is not expected to need updates in the future.

#### JSON File

 1. `Symbols.json` : Contains a list of all Unicode points (key) and their description/name

The first column (key) is the Unicode point and the second is the description.

#### Unicode Points

This application uses the *WinJS Symbols* Unicode points defined in the source PDF hosted on GitHub. The PDF is converted into a table and then manually merged to create the source JSON data.

#### Glyphs

The open source (MIT licensed) .ttf file is used to render glyphs in text.
