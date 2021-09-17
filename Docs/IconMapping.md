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
