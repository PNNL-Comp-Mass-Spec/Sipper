using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FigureXX_UnidentifiedFeatures
    {
        [Test]
        public void ExecuteSipperOnUnidentifiedFeatures()
        {
            string paramFile =
              @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);


            parameters.DbName = "";
            parameters.DbServer = "";
            parameters.DbTableName = "";

            parameters.TargetsBaseFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\Original";

           // parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";


            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            executor.Execute();

        }


        [Test]
        public void Compare_Identified_VS_Averagine_for_previouslyIDedTargets()
        {
            string normalResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";

            string averagineResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_UnidentifiedFeatures\Yellow_C13_070_23Mar10_Griffin_10-01-28_AVERAGINE_results.txt";

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(normalResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(averagineResultsFile);
            var averagineResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var originalTightFilter = originalResults.Where(SipperFilters.PassesLabelTightFilterF1).ToList();
            var averagineTightFilter = averagineResults.Where(SipperFilters.PassesLabelTightFilterF1).ToList();
            var intersectResults = originalTightFilter.Select(p => p.TargetID).Intersect(averagineTightFilter.Select(p => p.TargetID)).ToList();
            var uniqueToManual = originalTightFilter.Select(p => p.TargetID).Except(averagineTightFilter.Select(p => p.TargetID)).ToList();
            var uniqueToAuto = averagineTightFilter.Select(p => p.TargetID).Except(originalTightFilter.Select(p => p.TargetID)).ToList();

            Console.WriteLine("OriginalFiltered count= \t" + originalTightFilter.Count);
            Console.WriteLine("AveragineFiltered count= \t" + averagineTightFilter.Count);
            Console.WriteLine("Shared count = \t" + intersectResults.Count);
            Console.WriteLine("Unique to Manual = \t" + uniqueToManual.Count);
            Console.WriteLine("Unique to Auto = \t" + uniqueToAuto.Count);

            var originalLooseFilter = originalResults.Where(SipperFilters.PassesLabelLooseFilterF2).ToList();
            var averagineLooseFilter = averagineResults.Where(SipperFilters.PassesLabelLooseFilterF2).ToList();
            var intersectResultsLooseFilter = originalLooseFilter.Select(p => p.TargetID).Intersect(averagineLooseFilter.Select(p => p.TargetID)).ToList();
            uniqueToManual = originalLooseFilter.Select(p => p.TargetID).Except(averagineLooseFilter.Select(p => p.TargetID)).ToList();
            uniqueToAuto = averagineLooseFilter.Select(p => p.TargetID).Except(originalLooseFilter.Select(p => p.TargetID)).ToList();

            Console.WriteLine("LooseFilter results------------------");
            Console.WriteLine("OriginalFiltered count= \t" + originalLooseFilter.Count);
            Console.WriteLine("AveragineFiltered count= \t" + averagineLooseFilter.Count);
            Console.WriteLine("Shared count = \t" + intersectResultsLooseFilter.Count);
            Console.WriteLine("Unique to Manual = \t" + uniqueToManual.Count);
            Console.WriteLine("Unique to Auto = \t" + uniqueToAuto.Count);
        }




        [Test]
        public void FilterForEnrichedFeatures_LooseFilter()
        {

            string confidentlyIdentifiedResultsFilename =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";


            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(confidentlyIdentifiedResultsFilename);
            var confidentResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            
            string resultsFilename =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_UnidentifiedFeatures\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";


            var confidentFeatureIDs = confidentResults.Select(p => p.TargetID).Distinct().ToList();


           
            importer = new SipperResultFromTextImporter(resultsFilename);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();
            var nonRedundantOriginalResults = originalResults.GroupBy(x => x.TargetID).Select(g => g.First()).ToList();

            List<SipperLcmsFeatureTargetedResultDTO> resultsWithNoID = nonRedundantOriginalResults.Where(sipperLcmsFeatureTargetedResultDto => sipperLcmsFeatureTargetedResultDto.MatchedMassTagID <= 0 
                || !confidentFeatureIDs.Contains(sipperLcmsFeatureTargetedResultDto.TargetID)).ToList();


            var resultsWithID = confidentResults.GroupBy(x => x.TargetID).Select(g => g.First()).ToList();

            var tightFiltered = resultsWithNoID.Where(SipperFilters.PassesLabelTightFilterF1).ToList();
            

            var looseFiltered = resultsWithNoID.Where(SipperFilters.PassesLabelLooseFilterF2).ToList();


            string outputResultsFilename = resultsFilename.Replace("results.txt", "LooseLabelFilter_results.txt");
            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(outputResultsFilename);
            exporter.ExportResults (looseFiltered);

            outputResultsFilename = resultsFilename.Replace("results.txt", "TightLabelFilter_results.txt");
             exporter = new SipperResultToLcmsFeatureExporter(outputResultsFilename);
            exporter.ExportResults(tightFiltered);


            Console.WriteLine("Total results= " + nonRedundantOriginalResults.Count);
            Console.WriteLine("Total unidentified results= " + resultsWithNoID.Count);
            Console.WriteLine("Total results with ID= " + resultsWithID.Count);
            Console.WriteLine("Tight filter, count= " + tightFiltered.Count());
            Console.WriteLine("Loose filter, count= " + looseFiltered.Count());
        }


        [Test]
        public void FilterForEnrichedFeatures_TightFilter()
        {
            string resultsFilename =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_UnidentifiedFeatures\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";

            string outputResultsFilename = resultsFilename.Replace("results.txt", "TightLabelFilter_results.txt");

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultsFilename);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();
            var nonRedundantOriginalResults = originalResults.GroupBy(x => x.TargetID).Select(g => g.First()).ToList();

            var resultsWithNoID = (from n in nonRedundantOriginalResults where n.MatchedMassTagID <= 0 select n).ToList();
            var resultsWithID = (from n in nonRedundantOriginalResults where n.MatchedMassTagID > 0 select n).ToList();

            var filteredResults = resultsWithNoID.Where(SipperFilters.PassesLabelTightFilterF1).ToList();

            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(outputResultsFilename);
            exporter.ExportResults(filteredResults);

            Console.WriteLine("Total results= " + nonRedundantOriginalResults.Count);
            Console.WriteLine("Total unidentified results= " + resultsWithNoID.Count);
            Console.WriteLine("Total results with ID= " + resultsWithID.Count);
            Console.WriteLine("After filter, count= " + filteredResults.Count());

        }


        [Test]
        public void GetChromCorrValues_unidentifiedFeature()
        {

            string paramFile =
               @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);


            parameters.DbName = "";
            parameters.DbServer = "";
            parameters.DbTableName = "";

            parameters.TargetsBaseFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\Original";

            parameters.CopyRawFileLocal = false;
            //parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";



            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            int testTarget = 13232;

            Figure03_FeatureElutionScripts.OutputChromData(testDataset, executor, testTarget);
        }


        [Test]
        public void Output3DElutionProfilesForSelectFeatures()
        {
            string rawfile = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Analysis\FigureXX_UnidentifiedFeatures";

            var run = RunUtilities.CreateAndLoadPeaks(rawfile);

            Figure03_FeatureElutionScripts.ElutionInputData f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            f1.FeatureID = 13232;
            f1.MinScan = 9200;
            f1.MaxScan = 9600;
            f1.MinMZ = 1004;
            f1.MaxMZ = 1012;

            Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);

        }



        [Test]
        public void GetMassSpectrumForUnidentified()
        {
            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            var run = new RunFactory().CreateRun(testDataset);

            int scan = 9301;

            var scanset = new ScanSetFactory().CreateScanSet(run, scan, 5);

            run.CurrentScanSet = scanset;

            var msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            msgen.Execute(run.ResultCollection);


            XYData massSpec = run.XYData.TrimData(1003, 1012);

            TestUtilities.DisplayXYValues(massSpec);


        }


        [Test]
        public void ExamineLabelDistDataForCertainTarget()
        {

            string paramFile =
               @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);


            parameters.DbName = "";
            parameters.DbServer = "";
            parameters.DbTableName = "";

            parameters.TargetsBaseFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\Original";

            parameters.CopyRawFileLocal = false;
            //parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";



            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            int testTarget = 259;

            Figure03_FeatureElutionScripts.OutputChromData(testDataset, executor, testTarget);
        }






     


        [Test]
        public void ExecuteSipper()
        {
          


        }

    }
}
