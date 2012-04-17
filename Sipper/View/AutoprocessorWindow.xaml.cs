using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using Sipper.Model;
using Sipper.ViewModel;

namespace Sipper.View
{
    /// <summary>
    /// Interaction logic for AutoprocessorWindow.xaml
    /// </summary>
    public partial class AutoprocessorWindow : Window
    {

        private BackgroundWorker _backgroundWorker;

        public AutoprocessorWindow()
        {
            InitializeComponent();

            ViewModel = new AutoprocessorViewModel(new FileInputsInfo());

            ViewModel.CurrentResultUpdated += new CurrentResultChangedHandler(ViewModel_CurrentResultUpdated); 

            DataContext = ViewModel;

            
        }

        void ViewModel_CurrentResultUpdated(object sender, System.EventArgs e)
        {
            var graphUserControl = (GWSGraphLibrary.MSGraphControl)(graphHost.Child);

            var xvals = ViewModel.CurrentResult.MassSpectrumXYData.Xvalues;

            var yvals = ViewModel.CurrentResult.MassSpectrumXYData.Yvalues;

            if (xvals==null || xvals.Length<2)
            {
                return;
            }
            
            var min = 0d;


            var max = yvals.Max();


            string titleString = ViewModel.GetInfoStringOnCurrentResult();

            graphUserControl.GraphTitle = titleString;
            graphUserControl.GenerateGraph(xvals, yvals, xvals.Min(), xvals.Max(), min, max);
            

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

                ViewModel.FileInputs.CreateFileLinkages(fileNames);
                
              
            }
        }




        private void ExecuteProcessing()
        {
            if (!ViewModel.CanExecutorBeExecuted)
            {
                return;
            }


            ViewModel.Execute();


        }

    

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            ExecuteProcessing();
        }

        private void btnCancelClick(object sender, RoutedEventArgs e)
        {
            CancelProcessing();
        }

        private void CancelProcessing()
        {
            ViewModel.CancelProcessing();
        }
    }
}
