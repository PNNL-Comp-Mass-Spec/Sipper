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

        private void btnBrowseAndAnnotate_Click(object sender, RoutedEventArgs e)
        {

            var childWindow = new View.ManualViewingView(ViewModel.SipperProject);

            childWindow.ShowDialog();

            ViewModel.SipperProject.Run = childWindow.ViewModel.Run;


        }

        private void btnAutoprocess_Click(object sender, RoutedEventArgs e)
        {
            var childWindow = new View.AutoprocessorWindow(ViewModel.SipperProject);
            childWindow.Show();

            
        }

        private void btnStaticModeAnnotation_Click(object sender, RoutedEventArgs e)
        {
            var childWindow = new View.ManualAnnotationResultImageView(ViewModel.SipperProject);
            childWindow.ShowDialog();
        }
    }
}
