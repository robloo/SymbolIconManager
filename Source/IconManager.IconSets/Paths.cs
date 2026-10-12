using System;
using System.IO;

namespace IconManager.IconSets
{
    public static class Paths
    {
        /// <summary>
        /// Contains the directory to the root of the icon manager cache.
        /// This is commonly used for saving glyph images and source files.
        /// </summary>
        public static readonly string IconManagerCache = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "IconManagerCache");

        public const string BasePath = "avares://IconManager.IconSets/";

        public const string FluentUISystemGlyphSources = $"{BasePath}FluentUISystem/DataSources/FluentUISystemGlyphSources.json";

        public const string FluentUISystemRenamedIcons = $"{BasePath}FluentUISystem/DataSources/FluentUISystemRenamedIcons.txt";

        public const string FluentUISystemFilled       = $"{BasePath}FluentUISystem/DataSources/FluentSystemIcons-Filled.json";

        public const string FluentUISystemRegular      = $"{BasePath}FluentUISystem/DataSources/FluentSystemIcons-Regular.json";

        public const string LineAwesomeGlyphSources    = $"{BasePath}LineAwesome/DataSources/LineAwesomeGlyphSources.json";

        public const string LineAwesomeBrands          = $"{BasePath}LineAwesome/DataSources/la-brands-400.json";

        public const string LineAwesomeRegular         = $"{BasePath}LineAwesome/DataSources/la-regular-400.json";

        public const string LineAwesomeSolid           = $"{BasePath}LineAwesome/DataSources/la-solid-900.json";

        public const string SegoeFluent                = $"{BasePath}SegoeFluent/DataSources/SegoeFluentIcons.json";

        public const string SegoeMDL2Assets            = $"{BasePath}SegoeMDL2Assets/DataSources/SegoeMDL2Assets.json";

        public const string SegoeUISymbol              = $"{BasePath}SegoeUISymbol/DataSources/SegoeUISymbol.json";

        public const string WinJSSymbols               = $"{BasePath}WinJSSymbols/DataSources/Symbols.json";

        ///////////////////////////////////////////////////////////
        // Fonts
        ///////////////////////////////////////////////////////////

        public const string FluentUISystemFilledFont  = $"{BasePath}FluentUISystem/DataSources/FluentSystemIcons-Filled.ttf";

        public const string FluentUISystemRegularFont = $"{BasePath}FluentUISystem/DataSources/FluentSystemIcons-Regular.ttf";

        public const string LineAwesomeBrandsFont     = $"{BasePath}LineAwesome/DataSources/la-brands-400.ttf";

        public const string LineAwesomeRegularFont    = $"{BasePath}LineAwesome/DataSources/la-regular-400.ttf";

        public const string LineAwesomeSolidFont      = $"{BasePath}LineAwesome/DataSources/la-solid-900.ttf";

        public const string WinJSSymbolsFont          = $"{BasePath}WinJSSymbols/DataSources/Symbols.ttf";

        ///////////////////////////////////////////////////////////
        // Mappings
        ///////////////////////////////////////////////////////////

        public const string FluentAvaloniaMappings                  = $"{BasePath}Mappings/FluentAvalonia.json";

        public const string FluentUISystemToSegoeMDL2AssetsMappings = $"{BasePath}Mappings/FluentUISystemToSegoeMDL2Assets.json";

        public const string SegoeFluentMappings                     = $"{BasePath}Mappings/SegoeFluent.json";

        public const string SegoeUISymbolToSegoeMDL2AssetsMappings  = $"{BasePath}Mappings/SegoeUISymbolToSegoeMDL2Assets.json";
    }
}
