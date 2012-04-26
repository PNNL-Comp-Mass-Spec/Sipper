using NUnit.Framework;
using Sipper.ViewModel;

namespace Sipper.UnitTesting.ViewModelTests
{
    [TestFixture]
    public class ManualViewingWithNoRawDataTests
    {
        [Test]
        public void Test1()
        {


            ManualViewingWithoutRawDataViewModel viewModel = new ManualViewingWithoutRawDataViewModel();

            viewModel.FileInputs.ResultImagesFolderPath = @"D:\Data\Temp\Results\Visuals";

            viewModel.FileInputs.TargetsFilePath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_testing_results.txt";

            viewModel.LoadResults(viewModel.FileInputs.TargetsFilePath);


        }

    }
}
