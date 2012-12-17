﻿using System;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DeconTools.Backend;
using DeconTools.Workflows.Backend;
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
        private bool _graphsWereSetup = false;
        private int _counter;


        public ManualViewingView(Project project = null)
        {
            InitializeComponent();

            if (project == null)
            {
                project = new Project();
            }


            ViewModel = new ManualViewingViewModel(project.ResultRepository, project.FileInputs);

            LoadSettings();

            ViewModel.AllDataLoadedAndReadyEvent += new AllDataLoadedAndReadyEventHandler(ViewModel_AllDataLoadedAndReadyEvent);


            DataContext = ViewModel;

            ViewModel.Run = project.Run;

            setupGraphEventHandlers();

            updateGraphs();

        }

        private void setupGraphEventHandlers()
        {
            var msgraphUserControl = (GWSGraphLibrary.MSGraphControl)(msGraphHost.Child);
            msgraphUserControl.zedGraphControl1.ZoomEvent += zedGraphControl1_ZoomEvent;
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

            try
            {
                updateGraphs();
            }
            catch (Exception exception)
            {
                ViewModel.GeneralStatusMessage = exception.Message + "\t" + exception.StackTrace;
            }

        }



        private void setupGraphs()
        {
            var chromCorrUserControl = (GWSGraphLibrary.BasicGraphControl)(chromCorrGraphHost.Child);
            var chromUserControl = (GWSGraphLibrary.ChromGraphControl)(chromGraphHost.Child);
            var msgraphUserControl = (GWSGraphLibrary.MSGraphControl)(msGraphHost.Child);
            var theorMSUserControl = (GWSGraphLibrary.MSGraphControl)(theorMSGraphHost.Child);
            var ratioGraphUserControl = (GWSGraphLibrary.BasicGraphControl)(ratioGraphHost.Child);
            var logRatioGraphUserControl = (GWSGraphLibrary.BasicGraphControl)(logRatioGraphHost.Child);
            var labelDistribUserControl = (GWSGraphLibrary.BasicGraphControl)(labelDistributionGraphHost.Child);

            chromCorrUserControl.GraphPane.GraphObjList.Clear();
            chromUserControl.GraphPane.GraphObjList.Clear();
            msgraphUserControl.GraphPane.GraphObjList.Clear();
            theorMSUserControl.GraphPane.GraphObjList.Clear();
            labelDistribUserControl.GraphPane.GraphObjList.Clear();


            chromCorrUserControl.GraphPane.XAxis.Title.FontSpec.Size = 9;
            chromCorrUserControl.GraphPane.YAxis.Title.FontSpec.Size = 9;
            chromCorrUserControl.GraphPane.XAxis.Scale.FontSpec.Size = 9;
            chromCorrUserControl.GraphPane.YAxis.Scale.FontSpec.Size = 9;

            chromCorrUserControl.GraphPane.XAxis.MinorTic.Size = 0;

            chromCorrUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Format = "0.#";
            chromCorrUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Min = 0.5;
            chromCorrUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Max = 1.1;
            chromCorrUserControl.zedGraphControl1.GraphPane.XAxis.Title.Text = "peak num";
            chromCorrUserControl.zedGraphControl1.GraphPane.YAxis.Title.Text = "r2";
            chromCorrUserControl.zedGraphControl1.GraphPane.Title.FontSpec.Size = 9;
            chromCorrUserControl.zedGraphControl1.GraphPane.Title.Text = "Chrom correlation data";


            msgraphUserControl.GraphPane.YAxis.Scale.MagAuto = false;
            msgraphUserControl.GraphPane.YAxis.Scale.Mag = 0;


            chromUserControl.GraphPane.XAxis.Scale.IsUseTenPower = false;
            chromUserControl.GraphPane.XAxis.Scale.FormatAuto = false;
            chromUserControl.GraphPane.Title.IsVisible = false;
            chromUserControl.GraphPane.YAxis.Scale.MaxAuto = true;



            theorMSUserControl.GraphPane.YAxis.Scale.Format = "0.0";

            labelDistribUserControl.zedGraphControl1.GraphPane.Border.IsVisible = false;
            labelDistribUserControl.zedGraphControl1.GraphPane.X2Axis.IsVisible = false;
            labelDistribUserControl.zedGraphControl1.GraphPane.Y2Axis.IsVisible = false;
            labelDistribUserControl.zedGraphControl1.GraphPane.XAxis.Scale.Format = "0";
            labelDistribUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Mag = 0;
            labelDistribUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Format = "0.000";
            labelDistribUserControl.GraphPane.XAxis.Title.Text = "num labels";
            labelDistribUserControl.GraphPane.YAxis.Title.Text = "freq";
            labelDistribUserControl.GraphPane.Title.FontSpec.Size = 12;
            labelDistribUserControl.GraphPane.Title.Text = "Label distribution";
            labelDistribUserControl.zedGraphControl1.GraphPane.XAxis.MinorTic.Size = 0;
            labelDistribUserControl.zedGraphControl1.GraphPane.XAxis.Scale.MajorStep = 1;
            labelDistribUserControl.zedGraphControl1.GraphPane.YAxis.Scale.MajorStep = 0.05;
            labelDistribUserControl.zedGraphControl1.GraphPane.YAxis.Scale.MinorStep = 0.01;


            ratioGraphUserControl.GraphPane.XAxis.Title.FontSpec.Size = 9;
            ratioGraphUserControl.GraphPane.YAxis.Title.FontSpec.Size = 9;
            ratioGraphUserControl.GraphPane.XAxis.Scale.FontSpec.Size = 9;
            ratioGraphUserControl.GraphPane.YAxis.Scale.FontSpec.Size = 9;

            ratioGraphUserControl.GraphPane.XAxis.MinorTic.Size = 0;

            ratioGraphUserControl.zedGraphControl1.GraphPane.YAxis.Scale.MaxAuto = true;
            ratioGraphUserControl.zedGraphControl1.GraphPane.YAxis.Scale.MinAuto = true;

            ratioGraphUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Format = "0.#";
            ratioGraphUserControl.zedGraphControl1.GraphPane.XAxis.Title.Text = "peak num";
            ratioGraphUserControl.zedGraphControl1.GraphPane.YAxis.Title.Text = "(obs-theor)/(theor) ";
            ratioGraphUserControl.zedGraphControl1.GraphPane.Title.FontSpec.Size = 9;
            ratioGraphUserControl.zedGraphControl1.GraphPane.Title.Text = "Obs/Theor ratio data";



            logRatioGraphUserControl.GraphPane.XAxis.Title.FontSpec.Size = 9;
            logRatioGraphUserControl.GraphPane.YAxis.Title.FontSpec.Size = 9;
            logRatioGraphUserControl.GraphPane.XAxis.Scale.FontSpec.Size = 9;
            logRatioGraphUserControl.GraphPane.YAxis.Scale.FontSpec.Size = 9;

            logRatioGraphUserControl.GraphPane.XAxis.MinorTic.Size = 0;

            logRatioGraphUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Format = "0.#";
            logRatioGraphUserControl.zedGraphControl1.GraphPane.YAxis.Scale.MaxAuto = true;
            logRatioGraphUserControl.zedGraphControl1.GraphPane.YAxis.Scale.MinAuto = true;
            logRatioGraphUserControl.zedGraphControl1.GraphPane.XAxis.Title.Text = "peak num";
            logRatioGraphUserControl.zedGraphControl1.GraphPane.YAxis.Title.Text = "log (obs-theor)/(theor) ";
            logRatioGraphUserControl.zedGraphControl1.GraphPane.Title.FontSpec.Size = 9;
            logRatioGraphUserControl.zedGraphControl1.GraphPane.Title.Text = "Log(Obs/Theor) ratio data";






            _graphsWereSetup = true;




        }

        void zedGraphControl1_ZoomEvent(ZedGraphControl sender, ZoomState oldState, ZoomState newState)
        {

            var theorMSUserControl = (GWSGraphLibrary.MSGraphControl)(theorMSGraphHost.Child);
            theorMSUserControl.zedGraphControl1.GraphPane.XAxis.Scale.Min = sender.GraphPane.XAxis.Scale.Min;
            theorMSUserControl.zedGraphControl1.GraphPane.XAxis.Scale.Max = sender.GraphPane.XAxis.Scale.Max;


            theorMSUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Min = 0;
            theorMSUserControl.zedGraphControl1.GraphPane.YAxis.Scale.Max =
                ViewModel.GetMaxY(ViewModel.TheorProfileXYData, sender.GraphPane.XAxis.Scale.Min, sender.GraphPane.XAxis.Scale.Max);


            theorMSUserControl.zedGraphControl1.AxisChange();
            theorMSUserControl.Refresh();

        }


        private void updateGraphs()
        {
            if (!_graphsWereSetup)
            {
                setupGraphs();
            }
            setupGraphs();
            
            var chromCorrUserControl = (GWSGraphLibrary.BasicGraphControl)(chromCorrGraphHost.Child);
            var chromUserControl = (GWSGraphLibrary.ChromGraphControl)(chromGraphHost.Child);
            var msgraphUserControl = (GWSGraphLibrary.MSGraphControl)(msGraphHost.Child);
            var labelDistribUserControl = (GWSGraphLibrary.BasicGraphControl)(labelDistributionGraphHost.Child);
            var theorMSUserControl = (GWSGraphLibrary.MSGraphControl)(theorMSGraphHost.Child);
            var ratioGraphUserControl = (GWSGraphLibrary.BasicGraphControl)(ratioGraphHost.Child);
            var logRatioGraphUserControl = (GWSGraphLibrary.BasicGraphControl)(logRatioGraphHost.Child);



            chromCorrUserControl.GraphPane.GraphObjList.Clear();
            chromUserControl.GraphPane.GraphObjList.Clear();
            msgraphUserControl.GraphPane.GraphObjList.Clear();
            theorMSUserControl.GraphPane.GraphObjList.Clear();
            ratioGraphUserControl.GraphPane.GraphObjList.Clear();
            logRatioGraphUserControl.GraphPane.GraphObjList.Clear();
            labelDistribUserControl.GraphPane.GraphObjList.Clear();


            if (ViewModel.MassSpecXYData != null)
            {
                double ymax = ViewModel.GetMaxY(ViewModel.MassSpecXYData, ViewModel.MSGraphMinX, ViewModel.MSGraphMaxX);

                msgraphUserControl.zedGraphControl1.GraphPane.CurveList.Clear();

                msgraphUserControl.GenerateGraph(ViewModel.MassSpecXYData.Xvalues, ViewModel.MassSpecXYData.Yvalues, ViewModel.MSGraphMinX, ViewModel.MSGraphMaxX, 0, ymax);
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

            if (ViewModel.ChromXYData != null)
            {
                chromUserControl.GenerateGraph(ViewModel.ChromXYData.Xvalues, ViewModel.ChromXYData.Yvalues, ViewModel.ChromGraphMinX, ViewModel.ChromGraphMaxX);
            }


            if (ViewModel.LabelDistributionXYData != null)
            {
                labelDistribUserControl.GraphPane.CurveList.Clear();
                labelDistribUserControl.GraphPane.AddBar("", ViewModel.LabelDistributionXYData.Xvalues,
                                                         ViewModel.LabelDistributionXYData.Yvalues, Color.DarkSlateGray);


                double xMin, xMax, yMin, yMax;

                GetMinMaxValuesForLabelDistributionGraph(ViewModel.LabelDistributionXYData, out xMin, out xMax, out yMin,
                                                         out yMax);

                labelDistribUserControl.GraphPane.YAxis.Scale.MaxAuto = false;

                labelDistribUserControl.GraphPane.YAxis.Scale.Min = yMin;
                labelDistribUserControl.GraphPane.YAxis.Scale.Max = yMax;
                labelDistribUserControl.GraphPane.XAxis.Scale.Min = xMin;
                labelDistribUserControl.GraphPane.XAxis.Scale.Max = xMax;

                labelDistribUserControl.GraphPane.YAxis.Scale.MajorStepAuto=true;
                labelDistribUserControl.GraphPane.YAxis.Scale.MinorStepAuto = true;
                labelDistribUserControl.GraphPane.YAxis.Scale.MajorStep = yMax/10;

                ////var curve = labelDistribUserControl.GraphPane.CurveList.FirstOrDefault();

                //if (curve != null)
                //{
                //    if (curve is LineItem)
                //    {
                //        ((LineItem)curve).Line.Width = 2;
                //    }
                //}

            }


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





            if (ViewModel.ChromCorrXYData != null)
            {
                chromCorrUserControl.GenerateGraph(ViewModel.ChromCorrXYData.Xvalues, ViewModel.ChromCorrXYData.Yvalues);
                chromUserControl.AddAnnotationRelativeAxis(ViewModel.ChromTitleText, 0.5, 0, 10f);

            }

            if (ViewModel.RatioXYData != null)
            {
                ratioGraphUserControl.GenerateGraph(ViewModel.RatioXYData.Xvalues, ViewModel.RatioXYData.Yvalues);


            }

            var logRatioGraph = (GWSGraphLibrary.BasicGraphControl)(logRatioGraphHost.Child);
            if (ViewModel.RatioLogsXYData != null)
            {
                logRatioGraph.GenerateGraph(ViewModel.RatioLogsXYData.Xvalues, ViewModel.RatioLogsXYData.Yvalues);


            }



            msgraphUserControl.Refresh();
            chromUserControl.Refresh();
            theorMSUserControl.Refresh();
            labelDistribUserControl.Refresh();
            chromCorrUserControl.Refresh();
            logRatioGraph.Refresh();
            ratioGraphUserControl.Refresh();
        }

        private void GetMinMaxValuesForLabelDistributionGraph(XYData labelDistributionXYData, out double xMin, out double xMax, out double yMin, out double yMax)
        {

            xMin = 0.5;

            bool dataIsEmpty = labelDistributionXYData == null || labelDistributionXYData.Xvalues == null;

            bool dataIsLimited = dataIsEmpty || labelDistributionXYData.Xvalues.Length < 5;

            if (dataIsEmpty || dataIsLimited)
            {
                xMax = 5;
            }
            else
            {
                xMax = labelDistributionXYData.Xvalues.Last() + 0.5;
            }

            yMin = 0;




            if (dataIsEmpty)
            {
                yMax = 1.1;
            }
            else
            {


                bool dataIsMostlyUnlabelled = labelDistributionXYData.Yvalues.First() > 0.98;

                if (dataIsMostlyUnlabelled)
                {
                    yMax = 1.1;
                }
                else
                {
                    yMax = 0;
                    for (int i = 1; i < labelDistributionXYData.Yvalues.Length; i++)
                    {
                        var currentVal = labelDistributionXYData.Yvalues[i];

                        if (currentVal > yMax)
                        {
                            yMax = currentVal + currentVal *0.10;
                        }
                    }

                }



            }




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

        private void StackPanel_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (ViewModel.CurrentResult == null) return;


            if (e.Key==Key.Y)
            {
                ViewModel.CurrentResultValidationCode = ValidationCode.Yes;
                
                
            }
            else if (e.Key==Key.N)
            {
                ViewModel.CurrentResultValidationCode = ValidationCode.No;
                
            }
            else if (e.Key==Key.M)
            {
                ViewModel.CurrentResultValidationCode = ValidationCode.Maybe;
                
            }
            else if (e.Key==Key.O)
            {
                ViewModel.CurrentResultValidationCode = ValidationCode.None;
                
            }
            else
            {
                
            }

            

        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }

        private  void LoadSettings()
        {

            double minWidth = 1;
            double mzwidth = Properties.Settings.Default.MSGraphMZWindow;

            if (mzwidth<minWidth)
            {
                mzwidth = minWidth;
            }

            ViewModel.MassSpecVisibleWindowWidth = mzwidth;





        }

        private void SaveSettings()
        {
            if (ViewModel.MassSpecVisibleWindowWidth > 1)
            {
                Properties.Settings.Default.MSGraphMZWindow = ViewModel.MassSpecVisibleWindowWidth;
            }
            else
            {
                Properties.Settings.Default.MSGraphMZWindow = 10;
            }

            Properties.Settings.Default.Save();
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }

        private void TxtTargetFilterStringChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            //got this from stack overflow
            //use this to update the binding when anything is typed
            TextBox tBox = (TextBox)sender;
            DependencyProperty prop = TextBox.TextProperty;

            BindingExpression binding = BindingOperations.GetBindingExpression(tBox, prop);
            if (binding != null) { binding.UpdateSource(); }

        }

        private void btnClearTargetFilterClick(object sender, RoutedEventArgs e)
        {
            txtTargetFilterString.Text = string.Empty;
        }

    }
}
