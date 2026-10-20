# User Guide

This document helps explain common workflows when using the Symbol Icon Manager.

## Build a Font from a Mapping File

Before starting, note that the font building scripts are setup for Windows right now. Building a font requires FontForge to be installed to the default path (with its own Python plugin as well). This should be very easy to get working on other systems though as the build script is in Python and the Windows script only launches the Python script.

### Method 1 : From a Known Mapping File

 1. Start the application and go to the `Font Builder` tab.
 1. Find the section for the font you wish to build and click the corresponding `[Build ... Font]` button.
 1. The application will generate a FontForge Python script and download or copy all source SVG files that can be used to build the new font. The script and glyph SVG files will be added to a new directory within the application's executable directory.
 1. The `build_win.bat` (Windows) script will be automatically invoked to build the font.
      * Note that if the script fails the first time, wait 30 seconds and try again. It's possible glyph SVGs are still being downloaded.
 1. The new font is available at `output_font.ttf`. The output directory should automatically open for you.
      * After completion, a link to this document and the [Special Font Metric Step](#special-font-metric-step) will be opened. The final font metrics must be set manually using FontForge.

### Method 2 : From an Undefined Mapping File

 1. Start the application and go to the `Icon Mapping` tab and then press `[Open]`.
 1. Select a `.json` mapping file in the file picker or drop-down to open it, all mappings should now be visible in the view.
 1. Press `[...]` > `[Build Font]` and the application will generate a FontForge Python script and download or copy all source SVG files that can be used to build a new font. The script and glyph SVG files will be added to a new directory within the application's executable directory.
 1. The `build_win.bat` (Windows) script will be automatically invoked to build the font.
      * Note that if the script fails the first time, wait 30 seconds and try again. It's possible glyph SVGs are still being downloaded.
 1. The new font is available at `output_font.ttf`. The output directory should automatically open for you.
      * After completion, a link to this document and the [Special Font Metric Step](#special-font-metric-step) will be opened. The final font metrics must be set manually using FontForge.
 1. Rename the font file to whatever you like.
      * Note that internally the font name will be 'Symbols' but can be renamed using FontForge.

### Special Font Metric Step

A special step is needed after the .ttf file is generated to ensure the OS/2 table metrics are correct. This is mandatory for some applications. Unfortunately, there is an upstream bug in FontForge that prevents the Python script from being able to do this automatically (see [here](https://github.com/robloo/SymbolIconManager/blob/2021a5b44f2ee647bbeba176fb1d596a7f1b265d/Source/IconManager.Core/Fonts/FontBuilder.cs#L399-L424)).

To set these metrics:
 1. Open the font .ttf in FontForge
 1. Navigate to the Font Information by clicking `[Element]` -> `[Font Info...]`
 1. Then click on the `[OS/2]` side tab and the `[Metrics]` tab
 1. Set all metrics as shown below. These follow the SegoeFluent standard metrics.
    <p align="left">
      <img src="https://github.com/robloo/SymbolIconManager/raw/main/Docs/Images/FontForgeOS2Metrics.png" height="350px">
    </p>
 1. Click `[Save]` to close the metrics window
 1. Finally, save the changes to the .ttf file by clicking `[File]` -> `[Generate Fonts...]` and then `[Generate]`. Use default settings and only select the destination location. It is safe to ignore any warnings or errors during this process (Click `[Generate]` again).

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
