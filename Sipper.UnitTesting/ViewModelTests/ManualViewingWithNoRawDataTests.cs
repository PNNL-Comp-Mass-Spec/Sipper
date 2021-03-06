﻿using NUnit.Framework;
using Sipper.Model;
using Sipper.ViewModel;

namespace Sipper.UnitTesting.ViewModelTests
{
    [TestFixture]
    public class ManualViewingWithNoRawDataTests
    {
        [Test]
        [Ignore("Local file paths")]
        public void Test1()
        {
            var fileInputs = new FileInputsInfo();
            fileInputs.ResultImagesFolderPath = @"C:\Data\Temp\Results\Visuals";
            fileInputs.TargetsFilePath =@"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_testing_results.txt";

            var viewModel = new ManualViewingWithoutRawDataViewModel(fileInputs);

            viewModel.LoadResults(viewModel.FileInputs.TargetsFilePath);
        }
    }
}
