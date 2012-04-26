using System.Linq;
using NUnit.Framework;
using Sipper.Model;
using Sipper.ViewModel;

namespace Sipper.UnitTesting.ViewModelTests
{
    [TestFixture]
    public class ManualViewingTests
    {
        [Test]
        public void loadResultsTest()
        {
            string testParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperTargetedWorkflowParameters1.xml";


            string testResultFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_testing_results.txt";

            


            FileInputsInfo fileInputs = new FileInputsInfo();
            fileInputs.ParameterFilePath = testParameterFile;
            fileInputs.TargetsFilePath = testResultFile;

            ManualViewingViewModel viewModel = new ManualViewingViewModel(fileInputs);


            viewModel.LoadResults(testResultFile);
            Assert.IsNotEmpty(viewModel.Results);
        }



        [Test]
        public void loadParametersTest1()
        {
            string testParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperTargetedWorkflowParameters1.xml";

            FileInputsInfo fileInputs = new FileInputsInfo();
            fileInputs.ParameterFilePath = testParameterFile;

            ManualViewingViewModel viewModel = new ManualViewingViewModel(fileInputs);

            Assert.IsNotNull(viewModel.WorkflowParameters);
            Assert.AreEqual(2, viewModel.WorkflowParameters.MSPeakDetectorPeakBR);

        }


        [Test]
        public void executeWorkflowTest1()
        {
            string testDatafile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            string testResultFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_testing_results.txt";

            string testParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperTargetedWorkflowParameters1.xml";

            ManualViewingViewModel viewModel = new ManualViewingViewModel();

            viewModel.FileInputs.ParameterFilePath = testParameterFile;
            viewModel.FileInputs.TargetsFilePath = testResultFile;
            viewModel.FileInputs.DatasetPath = testDatafile;

            viewModel.CurrentResult = viewModel.Results.First();

            viewModel.ExecuteWorkflow();

            Assert.IsNotNull(viewModel.MassSpecXYData);
            Assert.IsNotNull(viewModel.ChromXYData);
            Assert.IsNotNull(viewModel.TheorProfileXYData);




        }

    }
}
