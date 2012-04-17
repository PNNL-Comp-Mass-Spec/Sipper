using System.Linq;
using System.Text;
using System.Windows;
using DeconTools.Workflows.Backend.Results;
using Sipper.ViewModel;

namespace Sipper.View
{
    /// <summary>
    /// Interaction logic for ManualViewingView.xaml
    /// </summary>
    public partial class ManualViewingView : Window
    {
        public ManualViewingView()
        {
            InitializeComponent();

            ViewModel = new ManualViewingViewModel();

            //ViewModel.CurrentResultUpdated += new CurrentResultChangedHandler(ViewModel_CurrentResultUpdated);

            DataContext = ViewModel;

            var chromGraph = (GWSGraphLibrary.ChromGraphControl) chromGraphHost.Child;
            

        }


        public ManualViewingViewModel ViewModel { get; set; }   


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

        private void txtWorkflowParameterFilepath_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void txtResultsFilePath_DragOver(object sender, DragEventArgs e)
        {
            bool dropEnabled = true;

            if (e.Data.GetDataPresent(DataFormats.FileDrop, true))
            {
                string[] filenames = e.Data.GetData(DataFormats.FileDrop, true) as string[];

                foreach (string filename in filenames)
                {
                    if (System.IO.Path.GetExtension(filename).ToUpperInvariant() != ".TXT")
                    {
                        dropEnabled = false;
                        break;
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

        private void ListView_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count==0) return;

            ViewModel.CurrentResult = (SipperLcmsFeatureTargetedResultDTO) e.AddedItems[0];

            ViewModel.ExecuteWorkflow();

            updateGraphs();
        }

        private void updateGraphs()
        {
            var msgraphUserControl = (GWSGraphLibrary.MSGraphControl)(msGraphHost.Child);

            var xvals = ViewModel.MassSpecXYData.Xvalues;
            var yvals = ViewModel.MassSpecXYData.Yvalues;

            if (xvals == null || xvals.Length < 2)
            {
                return;
            }

            //string titleString = ViewModel.GetInfoStringOnCurrentResult();

            //graphUserControl.zedGraphControl1.GraphPane.Title.Text = titleString;
            msgraphUserControl.GenerateGraph(xvals, yvals, ViewModel.MSGraphMinX, ViewModel.MSGraphMaxX);

            
            var chromUserControl = (GWSGraphLibrary.ChromGraphControl)(chromGraphHost.Child);

            var chromxvals = ViewModel.ChromXYData.Xvalues;
            var chromyvals = ViewModel.ChromXYData.Yvalues;

            if (xvals == null || xvals.Length < 2)
            {
                return;
            }

            //string titleString = ViewModel.GetInfoStringOnCurrentResult();

            //graphUserControl.zedGraphControl1.GraphPane.Title.Text = titleString;

            chromUserControl.GenerateGraph(chromxvals, chromyvals, ViewModel.ChromGraphMinX, ViewModel.ChromGraphMaxX);


            var theorMSUserControl = (GWSGraphLibrary.MSGraphControl)(theorMSGraphHost.Child);
            var theorMSXvals = ViewModel.TheorProfileXYData.Xvalues;
            var theorMSYvals = ViewModel.TheorProfileXYData.Yvalues;

            if (xvals == null || xvals.Length < 2)
            {
                return;
            }

            theorMSUserControl.GenerateGraph(theorMSXvals, theorMSYvals, ViewModel.MSGraphMinX, ViewModel.MSGraphMaxX);

            msgraphUserControl.Refresh();
            chromUserControl.Refresh();
            theorMSUserControl.Refresh();

        }

        private void btnSaveResultsClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SaveResults();
        }

        private void btnCopyMSToClipboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyMSDataToClipboard();
        }

        private void btnCopyTheorMSToClipboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyTheorMSToClipboard();
        }

        private void btnCopyChromatogramToClipboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyChromatogramToClipboard();
        }

       
    }
}
