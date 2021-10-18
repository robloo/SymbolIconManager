using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace IconManager
{
    public partial class FontsView : UserControl
    {
        private const string FluentAvaloniaMappingsPath = "avares://IconManager/Data/Mappings/FluentAvalonia.json";

        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public FontsView()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        /***************************************************************************************
         *
         * Methods
         *
         ***************************************************************************************/

        private async Task<string> GetSaveFilePath()
        {
            var dialog = new SaveFileDialog();
            var path = await dialog.ShowAsync(App.MainWindow);

            if (path != null)
            {
                if (File.Exists(path))
                {
                    // Delete the existing file, it will be replaced
                    File.Delete(path);
                }

                if (Directory.Exists(Path.GetDirectoryName(path)) == false)
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(path));
                }
            }

            return path ?? string.Empty;
        }

        private void BuildWinSymbols3Font()
        {
            var segoeFluentMappings = IconMappingList.Load(IconSet.SegoeFluent);

            var fontBuilder = new FontBuilder();
            fontBuilder.BuildFont(segoeFluentMappings, "WinSymbols3.ttf");

            return;
        }

        private async void RebuildFluentAvaloniaMappings()
        {
            string path = await this.GetSaveFilePath();

            if (string.IsNullOrEmpty(path) == false)
            {
                var fluentAvalonia = new Specialized.FluentAvalonia();
                var mappings = fluentAvalonia.RebuildMappings(IconMappingList.Load(FluentAvaloniaMappingsPath));

                using (var fileStream = File.OpenWrite(path))
                {
                    IconMappingList.Save(mappings, fileStream);
                }

                if (this.IncludeFluentAvaloniaEnumCheckBox.IsChecked ?? false)
                {
                    using (var fileStream = File.OpenWrite(Path.Combine(Path.GetDirectoryName(path)!, "enum.cs")))
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        writer.Write(fluentAvalonia.GenerateSymbolEnumSource(mappings));
                    }
                }
            }

            return;
        }

        private void BuildFluentAvaloniaFont()
        {
            var fluentAvaloniaMappings = IconMappingList.Load(FluentAvaloniaMappingsPath);
            var segoeFluentMappings = IconMappingList.Load(IconSet.SegoeFluent);

            // The Fluent Avalonia font must meet 3 requirements:
            //  1. Include all entries from the WinUI Symbol enum (even if no glyph is available)
            //     These icons are the base for the FluentAvalonia symbols enum.
            //  2. Include new, custom icons that exist in the FluentUISystem but not SegoeFluent.
            //     The custom icons are also part of the FluentAvalonia symbols enum.
            //  3. Be a drop-in replacement for the SegoeFluent font (WinSymbols3)
            //
            // The first two requirements are taken care of by the FluentAvalonia.json mapping file itself.
            // This file is specially built to ensure it is compatible with the WinUI symbol enum.
            // It also defines all custom icons for FluentAvalonia itself.
            //
            // To meet the last requirement, the SegoeFluent mappings must be merged in before the
            // font is built. These will take precedence over any overlapping entries in FluentAvalonia.
            segoeFluentMappings.MergeInto(fluentAvaloniaMappings);

            var fontBuilder = new FontBuilder();
            fontBuilder.BuildFont(fluentAvaloniaMappings, "FluentAvalonia.ttf");

            return;
        }

        /***************************************************************************************
         *
         * Event Handling
         *
         ***************************************************************************************/
    }
}
