using System.Linq;
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
        public AutoprocessorWindow(Project sipperProject = null)
        {
            InitializeComponent();

            if (sipperProject == null)
            {
                sipperProject = new Project();
            }


            ViewModel = new AutoprocessorViewModel(sipperProject.ResultRepository, sipperProject.FileInputs);

            ViewModel.CurrentResultUpdated += ViewModel_CurrentResultUpdated;

            DataContext = ViewModel;
        }

        void ViewModel_CurrentResultUpdated(object sender, System.EventArgs e)
        {

            var xvals = ViewModel.CurrentResultInfo.MassSpectrumXYData.Xvalues;
            var yvals = ViewModel.CurrentResultInfo.MassSpectrumXYData.Yvalues;

            if (xvals==null || xvals.Length<2)
            {
                return;
            }

            var min = 0d;


            var max = yvals.Max();


            var titleString = ViewModel.GetInfoStringOnCurrentResult();


        }

        public AutoprocessorViewModel ViewModel { get; set; }

        private void FileDropHandler(object sender, DragEventArgs e)
        {
            if (!(e.Data is DataObject dataObject))
                return;

            if (dataObject.ContainsFileDropList())
            {
                var fileNamesStringCollection = dataObject.GetFileDropList();

                var fileNames = fileNamesStringCollection.Cast<string>().ToList();

                ViewModel.FileInputs.CreateFileLinkages(fileNames);
            }
        }

        private void txtResultsFilePath_DragOver(object sender, DragEventArgs e)
        {
            var dropEnabled = true;

            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                if (!(e.Data.GetData(DataFormats.FileDrop, true) is string[] fileNames))
                {
                    dropEnabled = false;
                }
                else
                {
                    foreach (var filename in fileNames)
                    {
                        if (System.IO.Path.GetExtension(filename)?.ToUpper() != ".TXT")
                        {
                            dropEnabled = false;
                            break;
                        }
                    }
                }
            }
            else
            {
                dropEnabled = false;
            }


            if (!dropEnabled)
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;
        }




        private void ExecuteProcessing()
        {
            if (!ViewModel.CanExecutorBeExecuted)
            {
                return;
            }

            // Make sure the file paths are up-to-date
            // Note that settings are defined based on the file extension (.raw for raw data, .xml for the parameter file, and .txt for targets)
            ViewModel.FileInputs.CreateFileLinkage(txtRawDataFilepath.Text);
            ViewModel.FileInputs.CreateFileLinkage(txtTargetsFilePath.Text);
            ViewModel.FileInputs.CreateFileLinkage(txtWorkflowParameterFilepath.Text);

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
