using System;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class Figure03_FeatureElutionScripts
    {
        public struct ElutionInputData
        {
            internal int FeatureID;
            internal int MinScan;
            internal int MaxScan;
            internal int MinMZ;
            internal int MaxMZ;
        }


        [Test]
        public void Output3DElutionProfilesForSelectFeatures()
        {
            string rawfile = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Analysis\SampleElutionProfiles";

            var run = RunUtilities.CreateAndLoadPeaks(rawfile);

            ElutionInputData f1 = new ElutionInputData();
            f1.FeatureID = 8517;
            f1.MinScan = 8400;
            f1.MaxScan = 8800;
            f1.MinMZ = 769;
            f1.MaxMZ = 777;

            ExtractElutionProfile(outputFolder, run, f1);

        }


        [Test]
        public void Output3DElutionProfilesForLargeRange()
        {
            string rawfile = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            string outputFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX - massiveElutionImage";

            var run = RunUtilities.CreateAndLoadPeaks(rawfile);

            ElutionInputData f1 = new ElutionInputData();
            f1.FeatureID = 0;
            f1.MinScan = 4000;
            f1.MaxScan = 12000;
            f1.MinMZ = 600;
            f1.MaxMZ = 1000;

            ExtractElutionProfile(outputFolder, run, f1);

        }

       
        [Test]
        public void Output3DElutionProfilesForBadFeatures()
        {
            string rawfile = @"F:\Yellowstone\RawData\Yellow_C12_075_19Mar10_Griffin_10-01-13.raw";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Analysis\SampleElutionProfiles";

            var run = RunUtilities.CreateAndLoadPeaks(rawfile);



            ElutionInputData f1 = new ElutionInputData();
            f1.FeatureID = 9128;
            f1.MinScan = 7900;
            f1.MaxScan = 8300;
            f1.MinMZ = 635;
            f1.MaxMZ = 643;

            ExtractElutionProfile(outputFolder, run, f1);


            ElutionInputData f2 = new ElutionInputData();
            f2.FeatureID = 11985;
            f2.MinScan = 6400;
            f2.MaxScan = 6800;
            f2.MinMZ = 926;
            f2.MaxMZ = 934;

            ExtractElutionProfile(outputFolder, run, f2);


            ElutionInputData f3 = new ElutionInputData();
            f3.FeatureID = 12084;
            f3.MinScan = 8300;
            f3.MaxScan = 8700;
            f3.MinMZ = 931;
            f3.MaxMZ = 939;

            ExtractElutionProfile(outputFolder, run, f3);


            ElutionInputData f4 = new ElutionInputData();
            f4.FeatureID = 7781;
            f4.MinScan = 5700;
            f4.MaxScan = 6100;
            f4.MinMZ = 731;
            f4.MaxMZ = 738;

            ExtractElutionProfile(outputFolder, run, f4);

            ElutionInputData f5 = new ElutionInputData();
            f5.FeatureID = 6228;
            f5.MinScan = 6800;
            f5.MaxScan = 7200;
            f5.MinMZ = 562;
            f5.MaxMZ = 568;

            ExtractElutionProfile(outputFolder, run, f5);




        }

        [Test]
        public void GetChromCorrValues_GoodFeature()
        {

            string paramFile =
               @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            //parameters.DbName = "";
            //parameters.DbServer = "";
            //parameters.TargetsFilePath =
            //    @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Original\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = false;


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            int testTarget = 8517;

            OutputChromData(testDataset, executor, testTarget);


        }

        [Test]
        public void GetChromCorrValues_BadFeature()
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
                @"F:\Yellowstone\RawData\Yellow_C12_075_19Mar10_Griffin_10-01-13.raw";


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            int testTarget = 9128;

            OutputChromData(testDataset, executor, testTarget);
        }

        public static void OutputChromData(string testDataset, SipperWorkflowExecutor executor, int testTarget)
        {
            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTarget).ToList();

            executor.InitializeRun(testDataset);

            executor.ProcessDataset();


            SipperLcmsTargetedResult result = executor.TargetedWorkflow.Result as SipperLcmsTargetedResult;
            ChromatogramCorrelatorTask chromatogramCorrelator = new ChromatogramCorrelatorTask();
            chromatogramCorrelator.MinimumRelativeIntensityForChromCorr = 0.00001;
            chromatogramCorrelator.ChromToleranceInPPM = 10;

            chromatogramCorrelator.Execute(executor.TargetedWorkflow.Run.ResultCollection);

            Console.WriteLine("Dataset= " + testDataset);
            Console.WriteLine("Feature= \t" + result.Target.ID + "; mz= " + result.Target.MZ + ";z= " +
                              result.Target.ChargeState);

            Console.WriteLine();
            Console.WriteLine("------------Target Info -----------");
            Console.WriteLine("ID= \t" + result.Target.ID);
            Console.WriteLine("z= \t" + result.Target.ChargeState);
            Console.WriteLine("m/z= \t" + result.Target.MZ);
            Console.WriteLine("empFormula=\t" + result.Target.EmpiricalFormula);
            Console.WriteLine("seq=\t" + result.Target.Code);
            Console.WriteLine("scanTarget=\t" + result.Target.ScanLCTarget);


            //Console.WriteLine();
            //TestUtilities.DisplayIsotopicProfileData(result.Target.IsotopicProfile);

            Console.WriteLine();
            Console.WriteLine("-------------Isotopic profile data ---------------");
            int peakCounter = 0;
            foreach (var item in result.IsotopicProfile.Peaklist)
            {
                Console.WriteLine(peakCounter + "\t" + item.XValue + "\t" + item.Height);
                peakCounter++;
            }


            Console.WriteLine("-------------Theoretical Isotopic profile data ---------------");
            peakCounter = 0;
            foreach (var item in result.Target.IsotopicProfile.Peaklist)
            {
                Console.WriteLine(peakCounter + "\t" + item.XValue + "\t" + item.Height);
                peakCounter++;
            }



            Console.WriteLine();
            Console.WriteLine("-------------Correlation Values ---------------");
            peakCounter = 0;
            foreach (var item in result.ChromCorrelationData.CorrelationDataItems)
            {
                Console.WriteLine(peakCounter + "\t" + item.CorrelationRSquaredVal);
                peakCounter++;
            }


            Console.WriteLine();
            Console.WriteLine("-------------Label distribution Values ---------------");
            peakCounter = 0;
            foreach (var item in result.LabelDistributionVals)
            {
                Console.WriteLine(peakCounter + "\t" + item);
                peakCounter++;
            }


        }


        public static void ExtractElutionProfile(string outputFolder, Run run, ElutionInputData f1, bool applyLogTransform = false)
        {
            float[] intensities;
            int[] scans;
            double[] mzBinVals;
            var extractor = new IsotopicProfileElutionExtractor();
            extractor.Get3DElutionProfileFromPeakLevelData(run, f1.MinScan, f1.MaxScan, f1.MinMZ, f1.MaxMZ, out scans, out mzBinVals,
                                          out intensities, 0.01, applyLogTransform);

            string outputFilename = outputFolder + Path.DirectorySeparatorChar + f1.FeatureID.ToString().PadLeft(5, '0') +
                                    "_elutionData.txt";

            int numDecimals = 0;
            if (applyLogTransform) numDecimals = 2;
            
            ScriptUtilities.OutputStringToFile(outputFilename, extractor.OutputElutionProfileAsString('\t', true, numDecimals));

            Console.WriteLine("Retention time start = " + run.GetTime(f1.MinScan));
            Console.WriteLine("Retention time stop = " + run.GetTime(f1.MaxScan));
        }

        [Test]
        public void tempGetElutionProfile()
        {
            string rawfile = @"D:\Data\O16O18\Vlad_ALZ\Alz_P01_D10_142_26Apr12_Roc_12-03-26.RAW";

            var run = RunUtilities.CreateAndLoadPeaks(rawfile);
            int minScan = 2300;
            int maxScan = 2600;

            double minMZ = 624;
            double maxMZ = 638;

            float[] intensities;
            int[] scans;
            double[] mzBinVals;


            var extractor = new IsotopicProfileElutionExtractor();
            extractor.Get3DElutionProfileFromPeakLevelData(run, minScan, maxScan, minMZ, maxMZ, out scans, out mzBinVals,
                                          out intensities);

            string outputFilename = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\DeconTools_related\2012_10_03_Sample_3D_ElutionProfile\ElutionData_scan2300_2600.txt";

            ScriptUtilities.OutputStringToFile(outputFilename, extractor.OutputElutionProfileAsString());

            Console.WriteLine("Retention time start = " + run.GetTime(minScan));
            Console.WriteLine("Retention time stop = " + run.GetTime(maxScan));


        }
    }
}
