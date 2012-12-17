using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FigureXX_TypicalProteinSIPData
    {


        [Test]
        public void executeWorkflowTest1()
        {

            string paramFile =
             @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";


            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);


            //executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == 5905).ToList();
            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == 6496).ToList();
            executor.Execute();

            var iso = executor.TargetedWorkflow.Result.IsotopicProfile;


            Console.WriteLine("------------ observed ------------------------------");
            TestUtilities.DisplayIsotopicProfileData(iso);


            Console.WriteLine("------------ theoretical ------------------------------");
            TestUtilities.DisplayIsotopicProfileData(executor.TargetedWorkflow.Result.Target.IsotopicProfile);


            Console.WriteLine("------------ observed XY values ------------------------------");
            TestUtilities.DisplayXYValues(executor.TargetedWorkflow.MassSpectrumXYData);

        }




        [Test]
        public void OutputElutionProfileTest1()
        {
            string paramFile =
          @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            //parameters.DbName = "";
            //parameters.DbServer = "";
            //parameters.TargetsFilePath =
            //    @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Original\Yellow_C12_075_19Mar10_Griffin_10-01-13_results.txt";


            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = false;


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            //SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            //int testTarget = 11249;

            //Figure03_FeatureElutionScripts.OutputChromData(testDataset, executor, testTarget);

            string outputFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX - TypicalProteinSIPData";


            var run = RunUtilities.CreateAndLoadPeaks(testDataset);

            Figure03_FeatureElutionScripts.ElutionInputData f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            f1.FeatureID = 11249;
            f1.MinScan = 8400;
            f1.MaxScan = 8900;
            f1.MinMZ = 887;
            f1.MaxMZ = 897;

            Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);

        }

        public void OutputSipperInfoTest1()
        {
            string paramFile =
          @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            //parameters.DbName = "";
            //parameters.DbServer = "";
            //parameters.TargetsFilePath =
            //    @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Original\Yellow_C12_075_19Mar10_Griffin_10-01-13_results.txt";


            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = false;


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            int testTarget = 11249;


            Figure03_FeatureElutionScripts.OutputChromData(testDataset, executor, testTarget);



        }


        public void OutputSipperInfoTest_otherDataset()
        {
            string paramFile =
          @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = false;


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_069_26Mar10_Griffin_10-01-26.raw";


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            int testTarget = 5800;


            Figure03_FeatureElutionScripts.OutputChromData(testDataset, executor, testTarget);



        }


        public void OutputSipperInfoTest_feature4179_fraction069()
        {
            string paramFile =
          @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = false;


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_069_26Mar10_Griffin_10-01-26.raw";


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            int testTarget = 4179;


            Figure03_FeatureElutionScripts.OutputChromData(testDataset, executor, testTarget);



        }




        [Test]
        public void GetInfoForFeature5905()
        {
            int testTarget = 5905;

            string sequence = "ISGVDYVHAR";

            int chargeState = 2;

            string empFormula = new PeptideUtils().GetEmpiricalFormulaForPeptideSequence(sequence);


            LcmsFeatureTarget target = new LcmsFeatureTarget();
            target.ID = 5905;
            target.ChargeState = 2;
            target.EmpiricalFormula = empFormula;
            target.Code = sequence;


            DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator.JoshTheorFeatureGenerator theorFeatureGenerator =
                new JoshTheorFeatureGenerator();
            theorFeatureGenerator.LowPeakCutOff = 1e-10;

            theorFeatureGenerator.GenerateTheorFeature(target);
            TestUtilities.DisplayIsotopicProfileData(target.IsotopicProfile);


        }


        public void OutputSipperInfoTest_feature5905_fraction070()
        {
            string paramFile =
          @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = false;


            string testDataset =
                 @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            int testTarget = 5905;


            Figure03_FeatureElutionScripts.OutputChromData(testDataset, executor, testTarget);



        }

        public void OutputSipperInfoTest_feature4928_fraction071()
        {
            string paramFile =
          @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = false;


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_071_23Mar10_Griffin_10-01-26.raw";


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            int testTarget = 4928;


            Figure03_FeatureElutionScripts.OutputChromData(testDataset, executor, testTarget);



        }


        public void OutputSipperInfoTest_feature10761_fraction088()
        {
            string paramFile =
          @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = false;


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_088_23Mar10_Griffin_10-01-13.raw";


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            int testTarget = 10761;


            Figure03_FeatureElutionScripts.OutputChromData(testDataset, executor, testTarget);



        }

    }
}
