using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Net;

namespace IconManager
{
    public class IconViewModel : ViewModelBase, IIcon
    {
        private Bitmap? _Glyph;
        private Uri?    _GlyphUrl;
        private IconSet _IconSet;
        private string  _Name;
        private uint    _UnicodePoint;

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public IconViewModel()
        {
            this._Glyph        = null;
            this._GlyphUrl     = null;
            this._IconSet      = IconSet.Undefined;
            this._Name         = string.Empty;
            this._UnicodePoint = 0;
        }

        public IconViewModel(IIcon icon)
        {
            this._Glyph        = null;
            this._GlyphUrl     = null;
            this._IconSet      = icon.IconSet;
            this._Name         = icon.Name;
            this._UnicodePoint = icon.UnicodePoint;

            this.UpdateProperties();
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        ///////////////////////////////////////////////////////////
        // Data
        ///////////////////////////////////////////////////////////

        /// <summary>
        /// Gets or sets the displayed glyph (graphic/symbol) of the icon.
        /// </summary>
        public Bitmap? Glyph
        {
            get => this._Glyph;
            set => this.SetField(ref this._Glyph, value);
        }

        /// <summary>
        /// Gets or sets the URL (if it exists) of the icon.
        /// This URL is usually an online address pointing to the glyph to download.
        /// </summary>
        public Uri? GlyphUrl
        {
            get => this._GlyphUrl;
            set => this.SetField(ref this._GlyphUrl, value);
        }

        /// <inheritdoc/>
        public IconSet IconSet
        {
            get => this._IconSet;
            set
            {
                if (object.Equals(this._IconSet, value) == false)
                {
                    this.SetField(ref this._IconSet, value);
                    this.UpdateProperties();
                }
            }
        }

        /// <inheritdoc/>
        public string Name
        {
            get => this._Name;
            set => this.SetField(ref this._Name, value);
        }

        /// <inheritdoc/>
        public uint UnicodePoint
        {
            get => this._UnicodePoint;
            set
            {
                if (object.Equals(this._UnicodePoint, value) == false)
                {
                    this.SetField(ref this._UnicodePoint, value);
                    this.UpdateProperties();
                }
            }
        }

        ///////////////////////////////////////////////////////////
        // Calculated
        ///////////////////////////////////////////////////////////

        /// <inheritdoc/>
        public string UnicodeString
        {
            get => Icon.ToUnicodeString(this.UnicodePoint);
        }

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        /// <summary>
        /// Converts this <see cref="IconViewModel"/> into a standard <see cref="Icon"/>.
        /// </summary>
        /// <returns>A new <see cref="Icon"/> from the view model properties.</returns>
        public Icon AsIcon()
        {
            return new Icon()
            {
                IconSet      = this.IconSet,
                Name         = this.Name,
                UnicodePoint = this.UnicodePoint
            };
        }

        private void UpdateProperties()
        {
            switch (this.IconSet)
            {
                case IconSet.SegoeFluent:
                {
                    this.GlyphUrl = new Uri(@"https://docs.microsoft.com/en-us/windows/apps/design/style/images/glyphs/segoe-fluent-icons/" + Icon.ToUnicodeString(this.UnicodePoint) + ".png");
                    this.Name     = SegoeFluent.FindName(this.UnicodePoint);

                    break;
                }
                case IconSet.SegoeMDL2Assets:
                {
                    this.GlyphUrl = new Uri(@"https://docs.microsoft.com/en-us/windows/apps/design/style/images/segoe-mdl/" + Icon.ToUnicodeString(this.UnicodePoint) + ".png");
                    this.Name     = SegoeMDL2Assets.FindName(this.UnicodePoint);

                    break;
                }
            }

            return;
        }

        public void DownloadGlyphImage()
        {
            if (this.GlyphUrl != null)
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadDataAsync(this.GlyphUrl);
                    client.DownloadDataCompleted += (sender, e) =>
                    {
                        try
                        {
                            using (Stream stream = new MemoryStream(e.Result))
                            {
                                this.Glyph = new Avalonia.Media.Imaging.Bitmap(stream);
                            }
                        }
                        catch { }
                    };
                }
            }

            return;
        }
    }
}
