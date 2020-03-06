using NUnit.Framework;
using Sipper.Model;

namespace Sipper.UnitTesting.ModelTests
{
    [TestFixture]
    public class ResultImageOutputterTests
    {
        [Test]
        [Ignore("Local file paths")]
        public void Test1()
        {
            var fileInputs = new FileInputsInfo
            {
                DatasetDirectory = @"C:\Data\Sipper",
                ResultsSaveFilePath = @"C:\Data\Temp\Results\Visuals",
                TargetsFilePath =
                    @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\" +
                    @"SIPPER_standard_testing\" + "Yellow_C13_070_23Mar10_Griffin_10-01-28_testing_results.txt",
                ParameterFilePath =
                    @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\" +
                    @"SIPPER_standard_testing\SipperTargetedWorkflowParameters1.xml"
            };

            var imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }

        [Test]
        [Ignore("Local file paths")]
        public void OutputResultsForLaurey()
        {
            var fileInputs = new FileInputsInfo
            {
                DatasetDirectory = @"F:\Yellowstone\RawData",
                ResultsSaveFilePath =
                    @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\Visuals",
                TargetsFilePath =
                    @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\_Yellow_C13_Enriched_results.txt",
                ParameterFilePath =
                    @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperTargetedWorkflowParameters1.xml"
            };



            var imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }

        [Test]
        [Ignore("Local file paths")]
        public void OutputResultsForSelected300MassTags_C12()
        {
            var fileInputs = new FileInputsInfo
            {
                DatasetDirectory = @"F:\Yellowstone\RawData",
                ResultsSaveFilePath =
                    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C12_Visuals",
                TargetsFilePath =
                    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C12_withSelected300MassTags_results.txt",
                ParameterFilePath =
                    @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperTargetedWorkflowParameters1.xml"
            };

            var imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }

        [Test]
        [Ignore("Local file paths")]
        public void OutputResultsForSelected300MassTags_C13()
        {
            var fileInputs = new FileInputsInfo
            {
                DatasetDirectory = @"F:\Yellowstone\RawData",
                ResultsSaveFilePath =
                    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_Visuals",
                TargetsFilePath =
                    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_withSelected300MassTags_results.txt",
                ParameterFilePath =
                    @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperTargetedWorkflowParameters1.xml"
            };

            var imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }

        [Test]
        [Ignore("Local file paths")]
        public void OutputResultsForASMS_1()
        {
            var fileInputs = new FileInputsInfo
            {
                DatasetDirectory = @"F:\Yellowstone\RawData",
                ResultsSaveFilePath =
                    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant\Visuals",
                TargetsFilePath =
                    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant_enriched_results.txt",
                ParameterFilePath =
                    @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\SipperTargetedWorkflowParameters_Sum5.xml"
            };


            var imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }

        [Test]
        [Ignore("Local file paths")]
        public void ViewProteinResultsAcrossAllDatasets()
        {
            var fileInputs = new FileInputsInfo
            {
                DatasetDirectory = @"F:\Yellowstone\RawData",
                ResultsSaveFilePath =
                    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_06_25_SipperQuant_testing\ProteinCentricResults\Visuals",
                TargetsFilePath =
                    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_06_25_SipperQuant_testing\ProteinCentricResults\Targets\ref38803_results.txt",
                ParameterFilePath =
                    @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\SipperTargetedWorkflowParameters_Sum5.xml"
            };

            var imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }
    }
}
