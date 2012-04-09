using System.ComponentModel;
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

        private BackgroundWorker _backgroundWorker;

        public AutoprocessorWindow()
        {
            InitializeComponent();

            ViewModel = new AutoprocessorViewModel();

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

            graphUserControl.zedGraphControl1.GraphPane.Title.Text = titleString;
            graphUserControl.UpdateGraph(xvals, yvals, xvals.Min(), xvals.Max(), min, max);
            

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




        private void ExecuteProcessing()
        {
            if (!ViewModel.CanExecutorBeExecuted)
            {
                
                return;
            }


            ViewModel.Execute();

            return;
            _backgroundWorker=new BackgroundWorker();

            _backgroundWorker.WorkerReportsProgress = true;
            _backgroundWorker.WorkerSupportsCancellation = true;

            _backgroundWorker.DoWork +=new DoWorkEventHandler(_backgroundWorkerDoProcessingWork);

            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(_backgroundWorker_ProgressChanged);

            _backgroundWorker.RunWorkerAsync();

        }

        void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void _backgroundWorkerDoProcessingWork(object sender, DoWorkEventArgs e)
        {
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
