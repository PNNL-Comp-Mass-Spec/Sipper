﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using DeconTools.Backend;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Results;
using Sipper.Model;
using Sipper.ViewModel;
using Globals = DeconTools.Backend.Globals;

namespace Sipper.View
{
    /// <summary>
    /// Interaction logic for ViewAndAnnotateView.xaml
    /// </summary>
    public partial class ViewAndAnnotateView : Window
    {

        public ViewAndAnnotateView(Project project = null)
        {
            InitializeComponent();

            if (project == null)
            {
                project = new Project();
            }

            ViewModel = new ViewAndAnnotateViewModel(project.ResultRepository, project.FileInputs);

            LoadSettings();

            ViewModel.AllDataLoadedAndReadyEvent += ViewModel_AllDataLoadedAndReadyEvent;

            DataContext = ViewModel;
            ViewModel.Run = project.Run;
        }

        private void ViewModel_AllDataLoadedAndReadyEvent(object sender, EventArgs e)
        {
            resultsTab.Focus();
            resultsListView.SelectedItem = resultsListView.Items[0];
        }

        public ViewAndAnnotateViewModel ViewModel { get; set; }

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

        private void txtWorkflowParameterFilepath_TextChanged(object sender, TextChangedEventArgs e)
        {
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

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) return;

            ViewModel.CurrentResult = (SipperLcmsFeatureTargetedResultDTO)e.AddedItems[0];
            ViewModel.ExecuteWorkflow();
        }

        [Obsolete("Unused")]
        private void GetMinMaxValuesForLabelDistributionGraph(XYData labelDistributionXYData, out double xMin, out double xMax, out double yMin, out double yMax)
        {

            xMin = 0.5;

            var dataIsEmpty = labelDistributionXYData?.Xvalues == null;

            var dataIsLimited = dataIsEmpty || labelDistributionXYData.Xvalues.Length < 5;

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
                var dataIsMostlyUnlabeled = labelDistributionXYData.Yvalues.First() > 0.98;

                if (dataIsMostlyUnlabeled)
                {
                    yMax = 1.1;
                }
                else
                {
                    yMax = 0;
                    for (var i = 1; i < labelDistributionXYData.Yvalues.Length; i++)
                    {
                        var currentVal = labelDistributionXYData.Yvalues[i];

                        if (currentVal > yMax)
                        {
                            yMax = currentVal + currentVal * 0.10;
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
            ViewModel.CopyMsDataToClipboard();
        }

        private void btnCopyTheorMSToClipboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyTheorMsToClipboard();
        }

        private void btnCopyChromatogramToClipboard_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.CopyChromatogramToClipboard();
        }

        private void StackPanel_KeyDown(object sender, KeyEventArgs e)
        {
            if (ViewModel.CurrentResult == null) return;

            if (e.Key == Key.Y)
            {
                ViewModel.CurrentResultValidationCode = ValidationCode.Yes;
            }
            else if (e.Key == Key.N)
            {
                ViewModel.CurrentResultValidationCode = ValidationCode.No;
            }
            else if (e.Key == Key.M)
            {
                ViewModel.CurrentResultValidationCode = ValidationCode.Maybe;
            }
            else if (e.Key == Key.O)
            {
                ViewModel.CurrentResultValidationCode = ValidationCode.None;
            }
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SaveSettings();
        }

        private void LoadSettings()
        {

            double minWidth = 1;
            var mzWidth = Properties.Settings.Default.MSGraphMZWindow;

            if (mzWidth < minWidth)
            {
                mzWidth = minWidth;
            }

            ViewModel.MassSpecVisibleWindowWidth = mzWidth;
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

        private void TxtTargetFilterStringChanged(object sender, TextChangedEventArgs e)
        {
            //got this from stack overflow
            //use this to update the binding when anything is typed
            var tBox = (TextBox)sender;
            var prop = TextBox.TextProperty;

            var binding = BindingOperations.GetBindingExpression(tBox, prop);
            binding?.UpdateSource();
        }

        private void btnClearTargetFilterClick(object sender, RoutedEventArgs e)
        {
            txtTargetFilterString.Text = string.Empty;
        }

        private void btnNavigateUpClick(object sender, RoutedEventArgs e)
        {
            // ReSharper disable once RedundantArgumentDefaultValue
            ViewModel.NavigateToNextMs1MassSpectrum(Globals.ScanSelectionMode.ASCENDING);
        }

        private void btnNavigateDownClick(object sender, RoutedEventArgs e)
        {
            ViewModel.NavigateToNextMs1MassSpectrum(Globals.ScanSelectionMode.DESCENDING);
        }

        private void NavigateToSpecificScanEvent(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(txtCurrentScan.Text, out var currentScan))
            {
                if (currentScan == ViewModel.CurrentLcScan) return;

                ViewModel.CurrentLcScan = currentScan;
            }
            else
            {
                return;
            }

            ViewModel.NavigateToNextMs1MassSpectrum(Globals.ScanSelectionMode.CLOSEST);
        }

        private void TextBox_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ViewModel.NavigateToNextMs1MassSpectrum();
            }
            else
            {
                ViewModel.NavigateToNextMs1MassSpectrum(Globals.ScanSelectionMode.DESCENDING);
            }
        }

        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (ViewModel?.Run == null) return;

            slider.Minimum = ViewModel.MinLcScan;
            slider.Maximum = ViewModel.MaxLcScan;

            var sliderScan = (int)e.NewValue;
            if (ViewModel.CurrentLcScan == sliderScan) return;
            ViewModel.CurrentLcScan = (int)e.NewValue;
            ViewModel.NavigateToNextMs1MassSpectrum(Globals.ScanSelectionMode.CLOSEST);
        }

        private void MsGraphMinMouseWheelEvent(object sender, MouseWheelEventArgs e)
        {
            if (ViewModel?.Run == null) return;

            if (e.Delta > 0)
            {
                ViewModel.MsGraphMinX += 1;
            }
            else
            {
                ViewModel.MsGraphMinX -= 1;
            }
        }

        private void MsGraphMaxMouseWheelEvent(object sender, MouseWheelEventArgs e)
        {
            if (ViewModel?.Run == null) return;

            if (e.Delta > 0)
            {
                ViewModel.MsGraphMaxX += 1;
            }
            else
            {
                ViewModel.MsGraphMaxX -= 1;
            }
        }
    }
}
