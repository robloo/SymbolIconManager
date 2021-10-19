# User Guide

This document helps explain common workflows when using the Symbol Icon Manager.

## Build a Font from the Icon Mapping View

Before starting, note that the font building scripts are setup for Windows right now. Building a font requires FontForge to be installed to the default path (with its own Python plugin as well). This should be very easy to get working on other systems though as the build script is in Python and the Windows script only launches the Python script.

 1. Go to the `Icon Mapping` tab and then press [Load]
 1. Select a `.json` mapping file and open it, all mappings should now be visible in the view
 1. Press [Build Font] and the application will generate a FontForge Python script and download all source SVG files that can be used to build a new font. The script and glyph SVG files will be added to a new directory within the application's executable directory. This output directory should automatically open for you.
 1. Run `build_win.bat` (on Windows) to build the font and watch `output_font.ttf` be created automatically -- that's it!
      * Note that if the script fails the first time, wait 30 seconds and try again. It's possible glyph SVGs are still being downloaded.
 3. Rename the font file to whatever you like.
      * Note that internally the font name will be 'Symbols' but can be renamed using FontForge.

## Font Usage in XAML

Font's built with the Symbol Icon Manager are usable in all XAML-based projects. These fonts are standard TrueType (.ttf) files.

### Avalonia

 * The font must be added to the project file and Build Action set to `AvaloniaResource`
   * Usually font files should be added to an /Assets/Fonts directory within your project.
 * Reference the font in XAML:
   * Within a `TextBlock`: `FontFamily="avares://<app_name>/Assets/Fonts/output_font.ttf#Symbols"`

### WinUI/UWP

 * The font must be added to the project file and `Build Action` set to `Content`.
   * Usually font files should be added to an /Assets/Fonts directory within your project.
 * Reference the font in XAML: ``
   * Within a `TextBlock`: `FontFamily="ms-appx:///Assets/Fonts/output_font.ttf#Symbols" `
   * As a new resource: `<FontFamily x:Key="SymbolThemeFontFamily">ms-appx:///Assets/Fonts/output_font.ttf#Symbols</FontFamily>`

### Uno Platform

Follow the process for WinUI/UWP. For further details see [this blog](https://blog.mzikmund.com/2020/01/custom-fonts-in-uno-platform/).
