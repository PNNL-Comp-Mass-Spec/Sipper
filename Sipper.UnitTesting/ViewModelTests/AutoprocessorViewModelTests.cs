using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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


    }
}
