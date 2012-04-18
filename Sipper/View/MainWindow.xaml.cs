using System.Windows;
using Sipper.ViewModel;

namespace Sipper.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ViewModel = new MainWindowViewModel();
        }

        protected MainWindowViewModel ViewModel { get; set; }

        private void AutoMenuItemClick(object sender, RoutedEventArgs e)
        {

        }

        private void btnBrowseAndAnnotate_Click(object sender, RoutedEventArgs e)
        {

            var childWindow = new View.ManualViewingView();
            childWindow.Show();

        }

        private void btnAutoprocess_Click(object sender, RoutedEventArgs e)
        {
            var childWindow = new View.AutoprocessorWindow();
            childWindow.Show();
        }
    }
}
