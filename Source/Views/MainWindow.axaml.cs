using Avalonia;
using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using FluentAvalonia.UI.Windowing;

namespace IconManager
{
    public partial class MainWindow : AppWindow
    {
        /***************************************************************************************
         *
         * Constructors
         *
         ***************************************************************************************/

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.DataContext = this;
        }
    }
}
