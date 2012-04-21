using System.Windows;
using Sipper.ViewModel;

namespace Sipper.View
{
    /// <summary>
    /// Interaction logic for ManualAnnotationResultImageView.xaml
    /// </summary>
    public partial class ManualAnnotationResultImageView : Window
    {
        public ManualAnnotationResultImageView()
        {
            InitializeComponent();

            ViewModel = new ManualViewingWithoutRawDataViewModel();

            DataContext = ViewModel;

            ViewModel.ResultImagesFolderPath = @"D:\Data\Temp\Results\Visuals";

            ViewModel.LoadResults(
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_testing_results.txt");

        }


        public ManualViewingWithoutRawDataViewModel ViewModel { get; set; }   
    }
}
