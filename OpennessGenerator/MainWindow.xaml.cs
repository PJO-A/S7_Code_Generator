using OpennessGenerator.Constructor;
using System.Windows;

namespace OpennessGenerator
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ObjectIdentifier.ReadSettings(lbMessage);
        }

        private void btnGenHw_Click(object sender, RoutedEventArgs e)
        {

        }

        private void btnReloadCsv_Click(object sender, RoutedEventArgs e)
        {
            ObjectIdentifier.ReadSettings(lbMessage);
        }
    }
}
