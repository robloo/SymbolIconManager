using Avalonia.Media.Imaging;
using System;
using System.IO;
using System.Net;

namespace IconManager
{
    public class IconViewModel : ViewModelBase
    {
        private Bitmap?  _Glyph;
        private Uri?     _GlyphUrl;
        private IconSet? _IconSet;
        private string   _Name;
        private string   _UnicodePoint;

        /***************************************************************************************
         *
         * Constructor
         *
         ***************************************************************************************/

        public IconViewModel()
        {
            this._Glyph        = null;
            this._GlyphUrl     = null;
            this._IconSet      = null;
            this._Name         = string.Empty;
            this._UnicodePoint = string.Empty;
        }

        /***************************************************************************************
         *
         * Property Accessors
         *
         ***************************************************************************************/

        public Bitmap? Glyph
        {
            get => this._Glyph;
            set => this.SetField(ref this._Glyph, value);
        }

        public Uri? GlyphUrl
        {
            get => this._GlyphUrl;
            set => this.SetField(ref this._GlyphUrl, value);
        }

        public IconSet? IconSet
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

        public string Name
        {
            get => this._Name;
            set => this.SetField(ref this._Name, value);
        }

        public string UnicodePoint
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

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private void UpdateProperties()
        {
            bool propertiesSet = false;

            if (this.IconSet.HasValue &&
                this.IconSet.Value == IconManager.IconSet.SegoeMDL2Assets)
            {
                for (int i = 0; i < SegoeMDL2Assets.Icons.Count; i++)
                {
                    if (string.Equals(this.UnicodePoint, SegoeMDL2Assets.Icons[i].UnicodePoint, StringComparison.OrdinalIgnoreCase))
                    {
                        this.GlyphUrl = new Uri(@"https://docs.microsoft.com/en-us/windows/apps/design/style/images/segoe-mdl/" + this.UnicodePoint + ".png");
                        this.Name     = SegoeMDL2Assets.Icons[i].Name;

                        propertiesSet = true;
                        break;
                    }
                }
            }

            if (propertiesSet == false)
            {
                this.GlyphUrl = null;
                this.Name     = string.Empty;
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
