using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;
using Sipper.Model;

namespace Sipper.Scripts
{
    [TestFixture]
    public class ASMS2012Scripts
    {


        [Test]
        public void filterResultsToGiveOnlyEnriched()
        {
            string resultFileName =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant_results.txt";

            //string resultFileName =
            //    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant\Yellow_C13_070_23Mar10_Griffin_10-01-28_NR_NoFormula_results.txt";

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);

            var repo = importer.Import();

            var sipperResults = repo.Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();
            ResultFilteringUtilities.ApplyFilteringScheme2(sipperResults);

            var filteredResults = sipperResults.Where(p => p.PassesFilter).ToList();

            Console.WriteLine("total results = " + sipperResults.Count);
            Console.WriteLine("Num filtered results = " + filteredResults.Count);



            string exportFileName = resultFileName.Replace("_results.txt", "_enriched_results.txt");
            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportFileName);
            exporter.ExportResults(filteredResults);



        }
        
        [Test]
        public void findSharedResultsForIdentified_vs_unidentified()
        {
            string resultsWithFormulaFileName =
              @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant_results.txt";

            string resultsNoFormulaFileName =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant\Yellow_C13_070_23Mar10_Griffin_10-01-28_NR_NoFormula_results.txt";

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultsWithFormulaFileName);

            var repo = importer.Import();
            var sipperResults = repo.Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();
            ResultFilteringUtilities.ApplyFilteringScheme2(sipperResults);
            var formulaEnrichedResults = sipperResults.Where(p => p.PassesFilter).ToList();

            importer = new SipperResultFromTextImporter(resultsNoFormulaFileName);
            repo = importer.Import();
            sipperResults = repo.Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();
            ResultFilteringUtilities.ApplyFilteringScheme2(sipperResults);
            var noFormulaEnrichedResults = sipperResults.Where(p => p.PassesFilter).ToList();


            Console.WriteLine("With formula total = " + formulaEnrichedResults.Count);
            Console.WriteLine("No formula total = " + noFormulaEnrichedResults.Count);

            var intersect = formulaEnrichedResults.Select(p=>p.TargetID).Intersect(noFormulaEnrichedResults.Select(p=>p.TargetID)).ToList();

            var distictToFormula =
                formulaEnrichedResults.Select(p => p.TargetID).Except(noFormulaEnrichedResults.Select(p => p.TargetID)).
                    ToList();

            var distictToNoFormula =
               noFormulaEnrichedResults.Select(p => p.TargetID).Except(formulaEnrichedResults.Select(p => p.TargetID)).
                   ToList();

            Console.WriteLine("Intersect count = " + intersect.Count);
            Console.WriteLine("Only with formula count = " + distictToFormula.Count);

            Console.WriteLine("Only with no formula count = " + distictToNoFormula.Count);

            Console.WriteLine();
            foreach (var r in distictToNoFormula)
            {
                Console.WriteLine(r);
            }

        }


        [Test]
        public void filter_allUnidentifiedForEnriched()
        {
            string resultsUnidentifiedFileName =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_Unidentified\Yellow_C13_070_23Mar10_Griffin_10-01-28_UNIDENTIFIED_results.txt";


            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultsUnidentifiedFileName);

            var repo = importer.Import();
            var sipperResults = repo.Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();
            ResultFilteringUtilities.ApplyFilteringScheme2(sipperResults);
            var filteredResults = sipperResults.Where(p => p.PassesFilter).ToList();

            
            Console.WriteLine("Total results = " + sipperResults.Count);
            Console.WriteLine("filtered results = " + filteredResults.Count);

            foreach (var sipperLcmsFeatureTargetedResultDto in filteredResults)
            {
                sipperLcmsFeatureTargetedResultDto.PassesFilter = true;
                sipperLcmsFeatureTargetedResultDto.ValidationCode = ValidationCode.Yes;
            }


            string exportFileName = resultsUnidentifiedFileName.Replace("_results.txt", "_enrichedF2_results.txt");
            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportFileName);
            exporter.ExportResults(filteredResults);

           

        }



        [Test]
        public void OutputImageResultsForASMS_1()
        {
            FileInputsInfo fileInputs = new FileInputsInfo();

            fileInputs.DatasetDirectory = @"F:\Yellowstone\RawData";
            fileInputs.ResultsSaveFilePath = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_Unidentified\Visuals";
            fileInputs.TargetsFilePath =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_Unidentified\Yellow_C13_070_23Mar10_Griffin_10-01-28_UNIDENTIFIED_enrichedF1_results.txt";

            fileInputs.ParameterFilePath =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\SipperTargetedWorkflowParameters_Sum5.xml";


            ResultImageOutputter imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }

        [Test]
        public void OutputImageResultsForASMS_2()
        {
            FileInputsInfo fileInputs = new FileInputsInfo();

            fileInputs.DatasetDirectory = @"F:\Yellowstone\RawData";
            fileInputs.ResultsSaveFilePath = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_Unidentified\Visuals";
            fileInputs.TargetsFilePath =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_Unidentified\Yellow_C13_070_23Mar10_Griffin_10-01-28_UNIDENTIFIED_enrichedF2_results.txt";

            fileInputs.ParameterFilePath =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\SipperTargetedWorkflowParameters_Sum5.xml";


            ResultImageOutputter imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }


    }
}
