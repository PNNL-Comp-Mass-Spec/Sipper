using System;
using System.Linq;
using System.Text;
using System.Windows;
using DeconTools.Workflows.Backend.Results;
using Sipper.Model;
using Sipper.ViewModel;
using ZedGraph;

namespace Sipper.View
{
    /// <summary>
    /// Interaction logic for ManualViewingView.xaml
    /// </summary>
    public partial class ManualViewingView : Window
    {


        public ManualViewingView(Project project = null)
        {
            InitializeComponent();

            if (project == null)
            {
                project = new Project();
            }


            ViewModel = new ManualViewingViewModel(project.ResultRepository, project.FileInputs);

            ViewModel.AllDataLoadedAndReadyEvent+=new AllDataLoadedAndReadyEventHandler(ViewModel_AllDataLoadedAndReadyEvent);


            DataContext = ViewModel;

            ViewModel.Run = project.Run;
            updateGraphs();

        }

        private void ViewModel_AllDataLoadedAndReadyEvent(object sender, EventArgs e)
        {
            resultsTab.Focus();


            resultsListView.SelectedItem = resultsListView.Items[0];

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
            if (e.AddedItems.Count == 0) return;

            ViewModel.CurrentResult = (SipperLcmsFeatureTargetedResultDTO)e.AddedItems[0];

            ViewModel.ExecuteWorkflow();

            updateGraphs();
        }



        private void setupGraphs()
        {
            var chromCorrUserControl = (GWSGraphLibrary.BasicGraphControl)(chromCorrGraphHost.Child);
            var chromUserControl = (GWSGraphLibrary.ChromGraphControl)(chromGraphHost.Child);
            var msgraphUserControl = (GWSGraphLibrary.MSGraphControl)(msGraphHost.Child);
            var theorMSUserControl = (GWSGraphLibrary.MSGraphControl)(theorMSGraphHost.Child);

            chromCorrUserControl.GraphPane.GraphObjList.Clear();
            chromUserControl.GraphPane.GraphObjList.Clear();
            msgraphUserControl.GraphPane.GraphObjList.Clear();
            theorMSUserControl.GraphPane.GraphObjList.Clear();



            chromCorrUserControl.GraphPane.XAxis.Title.FontSpec.Size = 7;
            chromCorrUserControl.GraphPane.YAxis.Title.FontSpec.Size = 7;
            chromCorrUserControl.GraphPane.XAxis.Scale.FontSpec.Size = 7;
            chromCorrUserControl.GraphPane.YAxis.Scale.FontSpec.Size = 7;

            chromCorrUserControl.GraphPane.XAxis.MinorTic.Size = 0;





            chromCorrUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Format = "0.#";
            chromCorrUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Min = 0.5;
            chromCorrUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Max = 1.1;
            chromCorrUserControl.zedGraphControl1.GraphPane.XAxis.Title.Text = "peak num";
            chromCorrUserControl.zedGraphControl1.GraphPane.YAxis.Title.Text = "r2";
            chromCorrUserControl.zedGraphControl1.GraphPane.Title.FontSpec.Size = 8;
            chromCorrUserControl.zedGraphControl1.GraphPane.Title.Text = "Chrom correlation data";


            msgraphUserControl.GraphPane.YAxis.Scale.MagAuto = false;
            msgraphUserControl.GraphPane.YAxis.Scale.Mag = 0;



            chromUserControl.GraphPane.XAxis.Scale.IsUseTenPower = false;
            chromUserControl.GraphPane.XAxis.Scale.FormatAuto = false;
            chromUserControl.GraphPane.Title.IsVisible = false;
            chromUserControl.GraphPane.YAxis.Scale.MaxAuto = true;



            theorMSUserControl.GraphPane.YAxis.Scale.Format = "0.0";












        }


        private void updateGraphs()
        {
            setupGraphs();

            var msgraphUserControl = (GWSGraphLibrary.MSGraphControl)(msGraphHost.Child);

            if (ViewModel.MassSpecXYData != null)
            {
                msgraphUserControl.GenerateGraph(ViewModel.MassSpecXYData.Xvalues, ViewModel.MassSpecXYData.Yvalues, ViewModel.MSGraphMinX, ViewModel.MSGraphMaxX);
                var curve = msgraphUserControl.GraphPane.CurveList.FirstOrDefault();

                if (curve != null)
                {
                    if (curve is LineItem)
                    {
                        ((LineItem)curve).Line.Width = 2;
                    }
                }

                string graphTitle = "MS for scan " + (ViewModel.CurrentResult == null ? "" : ViewModel.CurrentResult.ScanLC.ToString("0"));
                msgraphUserControl.AddAnnotationRelativeAxis(graphTitle, 0.3, 0, 10);
            }

            var chromUserControl = (GWSGraphLibrary.ChromGraphControl)(chromGraphHost.Child);
            if (ViewModel.ChromXYData != null)
            {
                chromUserControl.GenerateGraph(ViewModel.ChromXYData.Xvalues, ViewModel.ChromXYData.Yvalues, ViewModel.ChromGraphMinX, ViewModel.ChromGraphMaxX);
            }


            var theorMSUserControl = (GWSGraphLibrary.MSGraphControl)(theorMSGraphHost.Child);
            if (ViewModel.TheorProfileXYData != null)
            {
                theorMSUserControl.GenerateGraph(ViewModel.TheorProfileXYData.Xvalues, ViewModel.TheorProfileXYData.Yvalues,
                                              ViewModel.MSGraphMinX, ViewModel.MSGraphMaxX);

                var curve = theorMSUserControl.GraphPane.CurveList.FirstOrDefault();

                if (curve != null)
                {
                    if (curve is LineItem)
                    {
                        ((LineItem)curve).Line.Width = 2;
                    }
                }


                string theorGraphTitle = "Theoretical MS - formula " + (ViewModel.CurrentResult == null ? "" : ViewModel.CurrentResult.EmpiricalFormula);
                theorMSUserControl.AddAnnotationRelativeAxis(theorGraphTitle, 0.3, 0, 10);
            }
            else
            {
                double[] xvals = { 1, 2, 3, 4 };
                double[] yvals = { 1, 1, 1, 1 };
                theorMSUserControl.GenerateGraph(xvals, yvals, xvals.Min(), xvals.Max());
            }


            var chromCorrUserControl = (GWSGraphLibrary.BasicGraphControl)(chromCorrGraphHost.Child);




            if (ViewModel.ChromCorrXYData != null)
            {
                chromCorrUserControl.GenerateGraph(ViewModel.ChromCorrXYData.Xvalues, ViewModel.ChromCorrXYData.Yvalues);
                chromUserControl.AddAnnotationRelativeAxis(ViewModel.ChromTitleText, 0.5, 0, 10f);

            }

            msgraphUserControl.Refresh();
            chromUserControl.Refresh();
            theorMSUserControl.Refresh();
            chromCorrUserControl.Refresh();

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
