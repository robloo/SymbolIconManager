using Avalonia;
using Avalonia.Controls;

namespace IconManager
{
    public partial class MainWindow : Window
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
