# Mapping Data

There are two types of mapping files:

 1. Font Mapping Files
    * These files contain the mapping information necessary to re-build the font from other, open-source icon sets such as those found in *Fluent UI System*.
	* These mapping files prioritize glyph match quality regardless of the metaphor. How an icon is visually appears to the user should be as close as possible. This ensures a constructed font is visually as similar as possible to the original. The glyphs visible to the user should match a given design language.
	* It is common to mix-and-match source icons from different icon sets when necessary.
    * Font mapping files are named with a single font name such as `SegoeFluent.json`.  
 2. Icon Set Mapping Files
    * These files contain the mapping information necessary to translate an icon from one icon set to another.
    * These mapping files prioritize metaphor match quality over glyph quality (opposite of above). It is up to heuristics to decide to use the glyph or try another icon set with a better match. Why is this done? Icon set mappings are used to remap symbol icons from one design language to another. In terms of user-interface, we want the symbols visible to the user to match a given design language. The link between design languages is metaphor, not glyph.
    * **ONLY includes a single source and destination icon set**. Do NOT mix and match icon set families in these files. The exception to this is for icon sets in the same family such as *Fluent UI System* regular/filled. This allows for algorithms to build mappings automatically using heuristics to determine fallback icons when certain icon sets are missing. These can also be automatically chained together to map icons between two icon sets that don't have their own file. 
    * These files are named similar to `FluentUISystemToSegoeMDL2Assets.json` indicating the two icon sets. Source and destination is a bit arbitrary but the first icon set name is the source, second is destination.

## Adding/Editing a Mapping

Mapping files are serialized using JSON. Each mapping entry in the file has the following properties:

 * Destination
   * `IconSet` : Specifies the icon set that contains the destination Unicode point. For the purpose of generating fonts this value is ignored.
   * `UnicodePoint` : Specifies the exact destination Unicode point of the glyph as a 32-bit hexadecimal formatted string (i.e. `EA50` or `100A9`). This will be the Unicode point in the generated font.
   * `Name` : Specifies an icon set-specific name that is primarily for human-readability. However, any provided name will also be added to the glyph in the generated font.
 * Source
   * `IconSet` : Specifies the icon set that contains the source Unicode point. This icon set will provide the glyph (image) used in the generated font.
   * `UnicodePoint` : Specifies the exact source Unicode point of the glyph as a 32-bit hexadecimal formatted string (i.e. `EA50` or `100A9`). This represents the glyph in the icon set that will be copied into the generated font.
   * `Name` : Specifies an icon set-specific name that is primarily for human-readability. However, for `Undefined` icon sets this name will be used to find source glyphs.
 * `GlyphMatchQuality` : Defines the quality of match for the glyphs of the two icons. See 'Match Quality' below
 * `MetaphorMatchQuality` : Defines the quality of match for the metaphor (purpose) of the two icons. See 'Match Quality' below
 * `IsPlaceholder` : Set to `True` to indicate the mapping had no equivalent so a generic substitution was made. **IsPlaceholder means there is an intention to replace the icon in the future as soon as a better one exists.** Generally, it is better not to include placeholders in a mapping file and just exclude the mapping altogether. However, in some cases an icon is quite heavily used and something needs to be provided even if there is no good match.
 * `Comments` : Descriptive comments describing the mapping and any decisions or special considerations.

To identify an exact glyph it is necessary to know both `IconSet` and `UnicodePoint`. Most icon sets use the same [Private Use Area](https://en.wikipedia.org/wiki/Private_Use_Areas) of the Unicode specification. There is no standardization for glyphs in this area and Unicode points will overlap between icon sets.

Each icon/mapping includes a `Name` property for both the source and destination. For all icon sets except `Undefined`, this name property is not used for glyph lookup (internal lookup tables exist to convert `IconSet`+`UnicodePoint` into an SVG source file name). Therefore, the `Name` property is optional and used to improve human readability (including in the generated script). However, any provided destination name will be used to set the name of the glyph itself in the generated font. For `Undefined` icon sets the `Name` property is used to find a matching SVG source file name.

For most icon sets except, notably, the *Fluent UI System* names are not guaranteed to be unique and cannot be used to identify an exact glyph. In fact, certain icon sets such as *Segoe MDL2 Assets* have duplicate names (WifiCall0 - WifiCall4 and WifiCallBars). However, the *Fluent UI System* follows a unique naming convention that can be used to identify glyphs and automatically find glyphs of a different size. See *Special *Fluent UI System* Considerations* below.

Future Considerations:
 * Some icons have multiple mappings that make sense. It may be necessary to have an optional "Priority" property to indicate which mapping take precedence when there are several options.

## Match Quality

Match quality allows for the overall quality of a mapping to be defined. This is especially useful to determine what mappings need to be re-visited or corrected in the future as new icons become available. It is also useful for calculating overall quality of a generated font.

As mentioned above, there are two properties that indicate match quality for each mapping. Each of these properties focuses only on a one aspect of the icon.

 * `GlyphMatchQuality` : Glyph match quality focuses only on the visual aspects of the icon. This is the graphic itself that is displayed to the user.
 * `MetaphorMatchQuality` : Metaphor match quality focuses on the 'purpose' of the icon. This is commonly described in an icon's name but may be another abstract concept.

Separating these two match quality properties allows for easily identifying the case where one icon set has an icon that looks visually very similar to another icon set; however, it was designed for an entirely different purpose. In this case the glyph match quality would be higher but the metaphor match quality lower.

### Match Quality Values

Each match quality property can be set with the below values:

 * `NoMatch` :  There is no discernible match between two icons.
   * Generally 'NoMatch' should not be used and instead no association (mapping) between two such icons should be made.
 * `Low` : **The user may have difficulty recognizing the similarity.** Two icons are loosely similar in metaphor or glyph and there are significant differences.
   * Significant differences are common if a substitution was made. For glyphs, these may be good candidates to retrieve from another icon set instead. Low match quality metaphors that have a higher glyph match quality are generally fine.
 * `Medium` : **The user can usually recognize the similarity.** Two icons are moderately similar in metaphor or glyph.
   * Example 1: One glyph has an accent arrow facing a different direction.
   * Example 2: One glyph is flipped vertically or horizontally from the other.
 * `High` : **The user can perfectly recognize the similarity.** Two icons have a very high degree of similarity in metaphor or glyph.
   * Example 1: One icon has rounded edges and the other square, but all other characteristics in the glyph are the same.
   * Example 2: One glyph is slightly larger or smaller than the other.
 * `Exact` : **Two icons are exactly the same in metaphor or glyph.** Again, the user can perfectly recognize the similarity.
   * Exact means there are absolutely no discernible differences between two icons.
   * This value is extremely rare for glyph matches but very common for metaphor matches.

Most matches between different icon sets should have a quality of `Medium`, `High` or `Exact` (rare). `Low` is commonly used when the mapping is a placeholder that is loosely similar (or a good glyph matches with a different metaphor), otherwise 'NoMatch' should be used. Instead of `NoMatch`, it is often better to exclude the mapping entirely (use it only for placeholders).

## Special *Fluent UI System* Considerations

The *Fluent UI System* icons are more powerful than the other icon sets. The *Fluent UI System* has icons in many different sizes and two different themes. Additionally, there is a different naming convention for Android and iOS. This requires some standard rules to enforce consistency in the mapping files. Other icon sets do not have these rules as there is only ever one Unicode point for a given icon -- no variations.

 1. FluentUISystem must use the size 20 variant. The size 20 variant was specially designed for desktop usage. If no size 20 variant exists, use the next closest (usually size 24). It is possible within code to automatically switch between sizes so setting the size in a mapping file does not restrict the possibility of generating a size 24 for mobile usage. It just helps standardize the mapping file.
 1. FluentUISystem must use the Android names
 1. FluentUISystem names are considered unique and can be used as an ID like Unicode point. This allows for automatically re-mapping between sizes and themes in code.
 1. Generally, Microsoft excludes Windows/App-specific icons from the public *Fluent UI System*. Icons for the file explorer, tiles, start menu, etc. are all missing. These designs remain properietary to Microsoft.
