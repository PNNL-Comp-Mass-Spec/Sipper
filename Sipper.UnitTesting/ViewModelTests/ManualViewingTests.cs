using System.Linq;
using NUnit.Framework;
using Sipper.ViewModel;

namespace Sipper.UnitTesting.ViewModelTests
{
    [TestFixture]
    public class ManualViewingTests
    {
        [Test]
        public void loadResultsTest()
        {
            string testResultFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_testing_results.txt";

            ManualViewingViewModel viewModel = new ManualViewingViewModel();

            viewModel.LoadResults(testResultFile);
            Assert.IsNotEmpty(viewModel.Results);
        }



        [Test]
        public void loadParametersTest1()
        {
            string testParameterFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperTargetedWorkflowParameters1.xml";

            ManualViewingViewModel viewModel = new ManualViewingViewModel();

            viewModel.LoadParameters(testParameterFile);
            Assert.IsNotNull(viewModel.WorkflowParameters);


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
