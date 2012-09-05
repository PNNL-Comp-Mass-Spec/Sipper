using System.Linq;
using DeconTools.UnitTesting2;
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
            Assert.IsNotNull(viewModel.LabelDistributionXYData);


            TestUtilities.DisplayXYValues(viewModel.LabelDistributionXYData);


        }


        [Test]
        public void executeWorkflowTest2_crashTest()
        {
            string testDatafile =
                @"F:\Yellowstone\RawData\Yellow_C13_086_23Mar10_Griffin_10-01-28.raw";

            string testResultFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\_Yellow_C13_F2_Enriched_NR_LS_GWS_validated.txt";

            string testParameterFile =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\SipperTargetedWorkflowParameters_Sum5.xml";

            ManualViewingViewModel viewModel = new ManualViewingViewModel();

            viewModel.FileInputs.ParameterFilePath = testParameterFile;
            viewModel.FileInputs.TargetsFilePath = testResultFile;
            viewModel.FileInputs.DatasetPath = testDatafile;

            viewModel.CurrentResult = viewModel.Results.First(p => p.TargetID == 15922);

            viewModel.ExecuteWorkflow();

            viewModel.CurrentResult = (from n in viewModel.Results
                                       where
                                           n.DatasetName == "Yellow_C13_085_23Mar10_Griffin_10-03-01" &&
                                           n.TargetID == 15937
                                       select n).First();

            viewModel.ExecuteWorkflow();

            Assert.IsNotNull(viewModel.MassSpecXYData);
            Assert.IsNotNull(viewModel.ChromXYData);
            Assert.IsNotNull(viewModel.TheorProfileXYData);
        }

    }
}
