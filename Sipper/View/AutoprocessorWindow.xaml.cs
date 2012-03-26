using System.Linq;
using System.Text;
using System.Windows;
using Sipper.ViewModel;

namespace Sipper.View
{
    /// <summary>
    /// Interaction logic for AutoprocessorWindow.xaml
    /// </summary>
    public partial class AutoprocessorWindow : Window
    {
        public AutoprocessorWindow()
        {
            InitializeComponent();

            ViewModel = new AutoprocessorViewModel();
            DataContext = ViewModel;
        }

        public AutoprocessorViewModel ViewModel { get; set; }

        private void FileDropHandler(object sender, DragEventArgs e)
        {
            DataObject dataObject = e.Data as DataObject;

            if (dataObject.ContainsFileDropList())
            {
               
                
                var fileNamesStringCollection = dataObject.GetFileDropList();
                StringBuilder bd = new StringBuilder();


                var fileNames = fileNamesStringCollection.Cast<string>().ToList();

                ViewModel.CreateFileLinkages(fileNames);
                
              
            }
        }
    }
}
