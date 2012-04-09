using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.FileIO;
using NUnit.Framework;
using Sipper.ViewModel;

namespace Sipper.UnitTesting.ViewModelTests
{
    [TestFixture]
    public class AutoprocessorViewModelTests
    {
        [Test]
        public void createFileLinkageSingleXMLFile_Test1()
        {
            AutoprocessorViewModel viewModel = new AutoprocessorViewModel();

            string testWorkflowFile = FileRefs.TestFilePath + "\\" + "SipperTargetedWorkflowParameters1.XML";
            viewModel.CreateFileLinkage(testWorkflowFile);

            Assert.IsTrue(viewModel.ExecutorParameters.WorkflowParameterFile == testWorkflowFile);
        }

        [Test]
        public void createFileLinkageSingleTextFile_Test1()
        {
            AutoprocessorViewModel viewModel = new AutoprocessorViewModel();

            string testTargetFile1 = FileRefs.TestFilePath + "\\" + "Yellow_C13_070_23Mar10_Griffin_10-01-28_LCMSFeatures.txt";
            viewModel.CreateFileLinkage(testTargetFile1);

            Assert.IsTrue(viewModel.ExecutorParameters.TargetsFilePath == testTargetFile1);
        }


        [Test]
        public void createFileLinkageMultiple_XMLFile_Test1()
        {
            AutoprocessorViewModel viewModel = new AutoprocessorViewModel();

            string testWorkflowFile = FileRefs.TestFilePath + "\\" + "SipperTargetedWorkflowParameters1.XML";
            string testWorkflowFile2 = FileRefs.TestFilePath + "\\" + "SipperTargetedWorkflowParameters1.xml";

            string testTargetFile1 = FileRefs.TestFilePath + "\\" + "Yellow_C13_070_23Mar10_Griffin_10-01-28_LCMSFeatures.txt";

            List<string> inputFiles = new List<string>();
            inputFiles.Add(testWorkflowFile);
            inputFiles.Add(testWorkflowFile2);
            inputFiles.Add(testTargetFile1);


            viewModel.CreateFileLinkages(inputFiles);

            Assert.IsTrue(viewModel.ExecutorParameters.WorkflowParameterFile == testWorkflowFile);
            Assert.IsTrue(viewModel.ExecutorParameters.TargetsFilePath==testTargetFile1);
        }

        [Test]
        public void create_Run__Test1()
        {
            AutoprocessorViewModel viewModel = new AutoprocessorViewModel();

            string testFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            viewModel.CreateFileLinkage(testFile);

            Assert.IsTrue(viewModel.DatasetFilePath == testFile);
        }

        [Test]
        public void process_test1()
        {
            AutoprocessorViewModel viewModel=new AutoprocessorViewModel();

            string testRawDataFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            string expectedResultsFile = Path.GetDirectoryName(testRawDataFile) + "\\" +
                                         RunUtilities.GetDatasetName(testRawDataFile) + "_results.txt";

            if (File.Exists(expectedResultsFile)) File.Delete(expectedResultsFile);



            string testWorkflowFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\SipperTargetedWorkflowParameters1.xml";
            string testTargetFile1 =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_testing_results.txt";

            viewModel.CreateFileLinkage(testRawDataFile);
            viewModel.CreateFileLinkage(testTargetFile1);
            viewModel.CreateFileLinkage(testWorkflowFile);

            viewModel.Execute();

            Assert.That(File.Exists(expectedResultsFile));
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(expectedResultsFile);
            var resultRepo = importer.Import();

            Assert.AreEqual(19, resultRepo.Results.Count);

            var testResult = (DeconTools.Workflows.Backend.Results.SipperLcmsFeatureTargetedResultDTO)resultRepo.Results[1];
            Assert.AreEqual("Yellow_C13_070_23Mar10_Griffin_10-01-28", testResult.DatasetName);
            Assert.AreEqual(7585, testResult.TargetID);
            Assert.AreEqual("C63H109N17O21", testResult.EmpiricalFormula);
            Assert.AreEqual(11805, testResult.ScanLC);
        }


    }
}
