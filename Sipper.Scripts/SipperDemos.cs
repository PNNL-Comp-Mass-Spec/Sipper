using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace Sipper.Scripts
{
    [TestFixture]
    public class SipperDemos
    {
        private string testingFolder = @"C:\Sipper\SipperDemo";

        [Test]
        public void Test1()
        {
            string paramFile = testingFolder + Path.DirectorySeparatorChar + "SipperInputs\\SipperExecutorParameters1.xml";
       
            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);



            string testDataset =
                @"C:\Sipper\SipperDemo\RawDataFiles\Yellow_C13_070_23Mar10_Griffin_10-01-28.mz5";

            //testDataset = @"C:\Sipper\SipperDemo\RawDataFiles\Yellow_C13_070_23Mar10_Griffin_10-01-28.mzML";

            //testDataset =@"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            executor.Execute();

            


        }

    }
}
