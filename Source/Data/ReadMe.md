# Icon Set Data

This folder contains raw icon set data files used by the application. Data is commonly gathered from third party sources. A list of individual data files is given in the below table.

| Icon Set Family | Data File | Last Update | Last Version | Source |
|-----------------|-----------|-------------|--------------|--------|
| Fluent UI System   | `FluentSystemIcons-Filled.json`  | 2021-Sep-15 | 1.1.139 | [GitHub Repo](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Filled.json) |
| Fluent UI System   | `FluentSystemIcons-Filled.ttf`   | 2021-Sep-15 | 1.1.139 | [GitHub Rego](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Filled.ttf) |
| Fluent UI System   | `FluentSystemIcons-Regular.json` | 2021-Sep-15 | 1.1.139 | [GitHub Repo](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Regular.json) |
| Fluent UI System   | `FluentSystemIcons-Regular.ttf`  | 2021-Sep-15 | 1.1.139 | [GitHub Repo](https://github.com/microsoft/fluentui-system-icons/blob/master/fonts/FluentSystemIcons-Regular.ttf) |
| Line Awesome       | `la-brands-400.ttf`              | 2021-Sep-16 | 1.3.0   | [GitHub Repo](https://github.com/icons8/line-awesome/blob/master/dist/line-awesome/fonts/la-brands-400.ttf) |
| Line Awesome       | `la-regular-400.ttf`             | 2021-Sep-16 | 1.3.0   | [GitHub Repo](https://github.com/icons8/line-awesome/blob/master/dist/line-awesome/fonts/la-regular-400.ttf) |
| Line Awesome       | `la-solid-900.ttf`               | 2021-Sep-16 | 1.3.0   | [GitHub Repo](https://github.com/icons8/line-awesome/blob/master/dist/line-awesome/fonts/la-solid-900.ttf) |
| Line Awesome       | `line-awesome.css`               | 2021-Sep-16 | 1.3.0   | [GitHub Repo](https://github.com/icons8/line-awesome/blob/master/dist/line-awesome/css/line-awesome.css) |
| Segoe Fluent Icons | `SegoeFluentIcons.json`          | 2021-Sep-12 | -       | [Microsoft Website](https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-fluent-icons-font) |
| Segoe MDL2 Assets  | `SegoeMDL2Assets.json`           | 2021-Jul-18 | -       | [Microsoft Website](https://docs.microsoft.com/en-us/windows/apps/design/style/segoe-ui-symbol-font) |
| WinJS Symbols      | `Symbols.json`                   | 2021-Jul-24 | 1.05    | [GitHub PDF](https://github.com/winjs/winjs/blob/master/src/fonts/SymbolsWinJS.pdf) |
| WinJS Symbols      | `Symbols.ttf`                    | 2021-Jul-24 | 1.05    | [GitHub Repo](https://github.com/winjs/winjs/blob/master/src/fonts/Symbols.ttf) |

Note the following:
 * Where applicable, JavaScript Object Notation (JSON) is the preferred format for all textual data.
 * Where applicable, TrueType Font (TTF) is the preferred format for all font data
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

#### CSS Files

All three styles of the *Line Awesome* icons share a single CSS file that defines the Unicode points and names for all supported icons. This file requires heavier processing to extract the needed information compared to JSON formatted files in other icon sets.

 1. `line-awesome.css` : Contains a CSS formatted list of all Unicode points and names for all three icon styles.

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

# Mapping Data

There are two types of mapping files:

 1. Font Mapping Files
    * These files contain the mapping information necessary to re-build the font from other, open-source icon sets such as those found in *Fluent UI System*.
	* These mapping files prioritize glyph match quality regardless of the metaphor. How an icon is visually appears to the user should be as close as possible. This ensures a constructed font is visually as similar as possible to the original.
	* It is common to mix-and-match source icons from different icon sets when necessary.
    * Font mapping files are named with a single font name such as `SegoeFluent.json`.  
 2. Icon Set Mapping Files
    * These files contain the mapping information necessary to translate an icon from one icon set to another.
    * These mapping files prioritize metaphor match quality over glyph quality (opposite of above). It is up to heuristics to decide to use the glyph or try another icon set with a better match. Why is this done? Icon set mappings are used to remap symbol icons from one design language to another. In terms of user-interface, we want the symbols visible to the user to match a given design language. The link between design languages is metaphor, not glyph.
    * **ONLY includes a single source and destination icon set**. Do NOT mix and match icon sets in these files. There should only ever be two. This allows for algorithms to build mappings automatically using heuristics to determine fallback icons when certain icon sets are missing. These can also be automatically chained together to map icons between two icon sets that don't have their own file. 
    * These files are named similar to `FluentUISystemToSegoeMDL2Assets.json` indicating the two icon sets. Source and destination is a bit arbitrary but the first icon set name is the source, second is destination.

## Adding/Editing a Mapping

Mapping files are serialized using JSON. Each mapping entry in the file has the following properties:

 * Destination
   * `IconSet` : 
   * `UnicodePoint` : 
   * `Name` : 
 * Source
   * `IconSet` : 
   * `UnicodePoint` : 
   * `Name` : 
 * `GlyphMatchQuality` : See 'Match Quality' below
 * `MetaphorMatchQuality` : See 'Match Quality' below
 * `IsPlaceholder` : The mapping had no equivalent so a generic substitution was made. Generally it is better not to include placeholders in a mapping file. Instead, just exclude the mapping altogether. However, in some cases an icon is quite heavily used and something needs to be provided even if there is no good match.
 * `Comments` : Descriptive comments describing the mapping and any decisions or special considerations.

Each mapping file includes a name property for both the source and destination. For all icon sets except the *Fluent UI System* these names are unused and are only added to improve human readability. In fact, certain icon sets such as *Segoe MDL2 Assets* do not always have unique names (WifiCall0 - WifiCall4 and WifiCallBars are duplicated). This means they must be matched by Unicode point only.

Future Considerations: Some icons have multiple mappings that make sense. It may be necessary in the future to have an optional "Priority" to indicate which mapping take precedence when there are several options.

## Match Quality

Match quality allows for the overall quality of a mapping to be defined. This is especially useful to determine what mappings need to be re-visited or corrected in the future as new icons become available. It is also useful for calculating overall quality of a generated font.

As mentioned above, there are two properties that indicate match quality for each mapping. Each of these properties focuses only on a one aspect of the icon.

 * `GlyphMatchQuality` : Glyph match quality focuses only on the visual aspects of the icon. This is the graphic itself that is displayed to the user.
 * `MetaphorMatchQuality` : Metaphor match quality focuses on the 'purpose' of the icon. This is commonly described in an icon's name but may be another abstract concept.

Separating these two match quality properties allows for easily identifying the case where one icon set has an icon that looks visually very similar to another icon set; however, it was designed for an entirely different purpose. In this case the glyph match quality would be higher but the metaphor match quality lower.

Most matches between different icon sets should have a quality of `Medium`, `High` or `Exact` (rare). `Low` is commonly used when the mapping is a placeholder that is loosely similar, otherwise 'NoMatch' should be used. Instead of `NoMatch`, it is often better to exclude the mapping entirely (use it only for placeholders).

More details about which values to select for match quality can be found in the [source code here](https://github.com/robloo/SymbolIconManager/blob/cd3cfa4c59d0dfca2603443061ff3fb2b4b5a834/Source/Models/MatchQuality.cs#L8-L52).

## Special *Fluent UI System* Considerations

The *Fluent UI System* icons are more powerful than the other icon sets. The *Fluent UI System* has icons in many different sizes and two different themes. Additionally, there is a different naming convention for Android and iOS. This requires some standard rules to enforce consistency in the mapping files. Other icon sets do not have these rules as there is only ever one Unicode point for a given icon -- no variations.

 1. FluentUISystem must use the size 20 variant. The size 20 variant was specially designed for desktop usage. If no size 20 variant exists, use the next closest (usually size 24). It is possible within code to automatically switch between sizes so setting the size in a mapping file does not restrict the possibility of generating a size 24 for mobile usage. It just helps standardize the mapping file.
 1. FluentUISystem must use the Android names
 1. FluentUISystem names are considered unique and can be used as an ID like Unicode point. This allows for automatically re-mapping between sizes and themes in code.
