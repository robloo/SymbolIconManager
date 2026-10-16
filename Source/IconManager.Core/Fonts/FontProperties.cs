using System;

namespace IconManager.Core.Fonts
{
    /// <summary>
    /// Contains properties of a generated font.
    /// </summary>
    public class FontProperties
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        /// <summary>
        /// Initializes a new instance of the <see cref="FontProperties"/> class.
        /// </summary>
        public FontProperties()
        {
            this.Comments   = "Released under the terms of the MIT license.";
            this.Copyright  = $@"Copyright (c) {DateTime.Now.Year}, SymbolIconManager";
            this.FamilyName = @"Symbols";
            this.Name       = @"Symbols";
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        /// <summary>
        /// Gets or sets any comments of the font.
        /// </summary>
        public string Comments { get; set; }

        /// <summary>
        /// Gets or sets the copyright information of the font.
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// Gets or sets the name of the font's family.
        /// </summary>
        public string FamilyName { get; set; }

        /// <summary>
        /// Gets or sets the name of the font.
        /// </summary>
        public string Name { get; set; }
    }
}
