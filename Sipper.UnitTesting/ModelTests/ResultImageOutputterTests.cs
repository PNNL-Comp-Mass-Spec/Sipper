using NUnit.Framework;
using Sipper.Model;

namespace Sipper.UnitTesting.ModelTests
{
    [TestFixture]
    public class ResultImageOutputterTests
    {
        [Test]
        public void Test1()
        {
            FileInputsInfo fileInputs = new FileInputsInfo();

            fileInputs.DatasetDirectory = @"F:\Yellowstone\RawData";
            fileInputs.ResultsSaveFilePath = @"D:\Data\Temp\Results\Visuals";
            fileInputs.TargetsFilePath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_testing_results.txt";

            fileInputs.ParameterFilePath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperTargetedWorkflowParameters1.xml";


            ResultImageOutputter imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }



        [Test]
        public void OutputResultsForLaurey()
        {
            FileInputsInfo fileInputs = new FileInputsInfo();

            fileInputs.DatasetDirectory = @"F:\Yellowstone\RawData";
            fileInputs.ResultsSaveFilePath = @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\Visuals";
            fileInputs.TargetsFilePath =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\_Yellow_C13_Enriched_results.txt";

            fileInputs.ParameterFilePath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperTargetedWorkflowParameters1.xml";


            ResultImageOutputter imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }

    }
}
