using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using Sipper.Model;
using Sipper.ViewModel;

namespace Sipper.View
{
    /// <summary>
    /// Interaction logic for FilterParameterOptimizerWindow.xaml
    /// </summary>
    public partial class FilterParameterOptimizerWindow : Window
    {
        public FilterParameterOptimizerWindow():this(null)
        {
            

        }


        public FilterParameterOptimizerWindow(Project sipperProject)
        {
            InitializeComponent();

            if (sipperProject==null) sipperProject = new Project();

            ViewModel = new SipperParameterOptimizerViewModel();
            DataContext = ViewModel;

        }

        public SipperParameterOptimizerViewModel ViewModel { get; set; }

        private void btnDoFilterOptimization_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.DoCalculationsOnAllParameterCombinations();
        }

        private void btnSelectUnlabeledFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            bool? result = openFileDialog.ShowDialog();

            if (result==true)
            {
                ViewModel.UnlabeledFilePath = openFileDialog.FileName;
            }

        }

        private void btnSelectLabeledFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            bool? result = openFileDialog.ShowDialog();

            if (result == true)
            {
                ViewModel.LabeledFilePath = openFileDialog.FileName;
            }
        }

        private void btnSetOutputFileName_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            bool? result = saveFileDialog.ShowDialog();

            if (result==true)
            {
                ViewModel.OutputFileName = saveFileDialog.FileName;
            }
        }

        private void btnUpdateMaxFalsePos_Click(object sender, RoutedEventArgs e)
        {

        }

        private void filterDatagrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            
        }


        private void BtnSetFavoriteFilter_OnClick(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedFilterParameter = ViewModel.CurrentFilterParameter;
        }

        private void btnExportRocToFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                ViewModel.SaveRocCurve(saveFileDialog.FileName);
            }
        }
    }
}
