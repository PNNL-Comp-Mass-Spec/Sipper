

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;
using Sipper.Model;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class Script_OptimizeFilters
    {
        
        [Category("Paper")]
        [Test]
        public void ParameterOptimization_forLabeledPeptides()
        {

            string manualResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_all_validated.txt";

            string autoResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";


            FileInfo fileInfo = new FileInfo(autoResultsFile);

            if (fileInfo.Exists)
            {
                Console.WriteLine("Autoresults file info:\nFilename=" + fileInfo.FullName + "\nLast write time= " + fileInfo.LastWriteTime);
            }

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(manualResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(autoResultsFile);
            var autoResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
            var noResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.No).ToList();
            var maybeResults = originalResults.Where(p => p.ValidationCode == ValidationCode.Maybe).ToList();

            var manualResults = yesResultsOnly;

            double maxRatio = 0;



            StringBuilder sb = new StringBuilder();
            List<ParameterOptimizationDataItem> optimizationData = new List<ParameterOptimizationDataItem>();

            sb.Append("fitScore\tarea\tiscore\tshared\tuniqueToAuto\tratio\tcontigScore\n");

            for (double fitScoreLabelled = 0.2; fitScoreLabelled < 1.1; fitScoreLabelled = fitScoreLabelled + 0.1)
            {
                for (double area = 0; area < 10.5; area = area + 1)
                {
                    for (double iscore = 0; iscore < 1.0; iscore = iscore + 0.1)
                    {
                        for (double chromCorr = 0.955; chromCorr <= 0.96; chromCorr = chromCorr + 0.1)
                        {
                            for (double rsquaredVal = 0.55; rsquaredVal <= 0.56; rsquaredVal = rsquaredVal + 0.05)
                            {
                                for (int contigScore = 0; contigScore <= 7; contigScore++)
                                {

                                    for (double percentIncorp = 0; percentIncorp < 2; percentIncorp = percentIncorp + 0.5)
                                    {
                                        for (double peptidePop = 0; peptidePop < 2; peptidePop = peptidePop + 0.5)
                                        {


                                            var filteredResults = (from n in autoResults
                                                                   where
                                                                   n.AreaUnderRatioCurveRevised >= area
                                                                  && n.IScore <= iscore
                                                                   && n.FitScoreLabeledProfile <= fitScoreLabelled
                                                                       // && n.ChromCorrelationMedian >= chromCorr
                                                                  && n.ContiguousnessScore >= contigScore
                                                                       //&& n.NumHighQualityProfilePeaks > 2
                                                                    && n.PercentCarbonsLabelled >= percentIncorp &&
                                                                   n.PercentPeptideLabelled >= peptidePop


                                                                   select n).ToList();

                                            var intersectResults =
                                                filteredResults.Select(p => p.TargetID).Intersect(manualResults.Select(p => p.TargetID)).ToList();
                                            var uniqueToAuto =
                                                filteredResults.Select(p => p.TargetID).Except(manualResults.Select(p => p.TargetID)).ToList();
                                            var ratio = intersectResults.Count / (double)uniqueToAuto.Count;


                                            ParameterOptimizationDataItem optimizationDataItem =
                                                new ParameterOptimizationDataItem(fitScoreLabelled, area, iscore, chromCorr, contigScore, percentIncorp, peptidePop, intersectResults.Count, uniqueToAuto.Count);
                                            optimizationDataItem.RSquared = rsquaredVal;


                                            if (double.IsNaN(ratio))
                                            {
                                                ratio = -1;
                                            }

                                            optimizationData.Add(optimizationDataItem);

                                        }
                                    }
                                }
                            }
                        }
                    }
                }

            }


            int maxNumUniqueToAuto = optimizationData.Select(p => p.UniqueToAuto).Max();

            List<ParameterOptimizationDataItem> firstSetOfParameters = new List<ParameterOptimizationDataItem>();
            bool foundFirstSet = false;
            int minUniqueToAuto = 0;

            Dictionary<int, int> pairs = new Dictionary<int, int>();

            for (int i = 0; i <= maxNumUniqueToAuto; i++)
            {
                var uniqueToAuto = optimizationData.Where(p => p.UniqueToAuto == i);

                int maxSharedVal;
                bool any = uniqueToAuto.Any();
                if (any)
                {
                    maxSharedVal = uniqueToAuto.Max(p => p.SharedCount);

                    pairs.Add(i, maxSharedVal);

                    Console.WriteLine(i + "\t" + maxSharedVal);
                }
            }


            //Now I iterate over certain levels of false-negative values, and pull
            //out the parameter settings
            int[] falsePositiveArray = {0, 2,3, 4, 6, 15};
            foreach (var i in falsePositiveArray)
            {
                var anotherPair = pairs.First(p => p.Key == i);

                var bestParams =
               (from n in optimizationData where n.UniqueToAuto == anotherPair.Key && n.SharedCount == anotherPair.Value select n).ToList();

                Console.WriteLine();
                Console.WriteLine("uniqueToAuto\tShared\tArea\tChromCorr\tFitScoreLabelled\tIScore\tContig\tPercentIncorp\tPercentPeptide");
                foreach (var item in bestParams)
                {
                    Console.WriteLine(item.UniqueToAuto + "\t" + item.SharedCount + "\t" + item.Area + "\t" + item.ChromCorr + "\t" +
                                      item.FitScoreLabelled + "\t" + item.IScore + "\t" + item.ContigScore + "\t" + item.PercentIncorporation + "\t" + item.PercentPeptide);
                }
            }

           





            //Console.WriteLine(sb.ToString());
        }

   

        [Category("Paper")]
        [Test]
        public void GetFilteredOutputOnAutoProcessed()
        {


            string manualResultsFile =
           @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_ALL_validated.txt";

            string autoResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(manualResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(autoResultsFile);
            var autoResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();

            var noResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.No).ToList();

            var intersectAutoYes = autoResults.Select(p => p.TargetID).Intersect(yesResultsOnly.Select(p => p.TargetID)).ToList();


            string outputFolder =
              @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset";

            //Get all auto results and Tag the validation code
            SipperFilters.ApplyAutoValidationCodeF1TightFilter(autoResults);

            var autoYESIDsOnly = autoResults.Where(p => p.ValidationCode == ValidationCode.Yes).Select(p => p.TargetID).ToList();
            var manualResultsWithAutoYES = (from n in originalResults where autoYESIDsOnly.Contains(n.TargetID) select n).ToList();
            string outputFile = outputFolder + "\\" + Path.GetFileName(autoResultsFile).Replace("_results.txt", "_manual_But_AutoTightYES_results.txt");
            var exporter = new SipperResultToLcmsFeatureExporter(outputFile);
            exporter.ExportResults(manualResultsWithAutoYES);

            SipperFilters.ApplyAutoValidationCodeF2LooseFilter(autoResults);
            autoYESIDsOnly = autoResults.Where(p => p.ValidationCode == ValidationCode.Yes).Select(p => p.TargetID).ToList();
            manualResultsWithAutoYES = (from n in originalResults where autoYESIDsOnly.Contains(n.TargetID) select n).ToList();
            outputFile = outputFolder + "\\" + Path.GetFileName(autoResultsFile).Replace("_results.txt", "_manual_But_AutoLooseYES_results.txt");
            exporter = new SipperResultToLcmsFeatureExporter(outputFile);
            exporter.ExportResults(manualResultsWithAutoYES);

            SipperFilters.ApplyAutoValidationCodeF1TightFilter(autoResults);
            outputFile = outputFolder + "\\" + Path.GetFileName(autoResultsFile).Replace("_results.txt", "_Auto_TightFilter_results.txt");
            exporter = new SipperResultToLcmsFeatureExporter(outputFile);
            exporter.ExportResults(autoResults);

            SipperFilters.ApplyAutoValidationCodeF2LooseFilter(autoResults);
            outputFile = outputFolder + "\\" + Path.GetFileName(autoResultsFile).Replace("_results.txt", "_Auto_LooseFilter_results.txt");
            exporter = new SipperResultToLcmsFeatureExporter(outputFile);
            exporter.ExportResults(autoResults);
            
        }
   

        [Test]
        public void DisplayStatsOnAutoResults()
        {

            string manualResultsFile =
             @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_ALL_validated.txt";

            string autoResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";

            //autoResultsFile =
            //    @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Original\Yellow_C12_070_21Feb10_Griffin_09-11-40_results.txt";


            autoResultsFile =
               @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";


            //autoResultsFile =
            //    @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Original\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";



            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(manualResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(autoResultsFile);
            var autoResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();

            var noResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.No).ToList();

            var intersectResultIDs = autoResults.Select(p => p.TargetID).Intersect(yesResultsOnly.Select(p => p.TargetID)).ToList();

            //intersectResultIDs = autoResults.Select(p => p.TargetID).Intersect(noResultsOnly.Select(p => p.TargetID)).ToList();


            List<SipperLcmsFeatureTargetedResultDTO> filteredResults = new List<SipperLcmsFeatureTargetedResultDTO>();

            foreach (var intersectResult in intersectResultIDs)
            {
                filteredResults.AddRange(autoResults.Where(n => n.TargetID == intersectResult));
            }


            foreach (var sipperLcmsFeatureTargetedResultDto in filteredResults)
            {
                Console.WriteLine(sipperLcmsFeatureTargetedResultDto.ToStringWithDetailsAsRow());
            }

            string outputFolder = @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\Temp - trying to figure diff between old and new";
            string outputFile = outputFolder + "\\" + Path.GetFileName(autoResultsFile).Replace("_results.txt", "_manual_results.txt");

            var exporter = new SipperResultToLcmsFeatureExporter(outputFile);
            exporter.ExportResults(yesResultsOnly);




        }









        [Test]
        public void compareNewVsOld()
        {
            string manualResultsFile =
             @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_ALL_validated.txt";




            string autoResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";


            string autoResultsFile2 =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Original\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";


            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(manualResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(autoResultsFile);
            var autoResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(autoResultsFile2);
            var autoresults2 = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();


            var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();

            var noResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.No).ToList();


            var AutoResultsFilteredForManualYes = autoResults.Select(p => p.TargetID).Intersect(yesResultsOnly.Select(p => p.TargetID)).ToList();

            //AutoResultsFilteredForManualYes = autoResults.Select(p => p.TargetID).Intersect(noResultsOnly.Select(p => p.TargetID)).ToList();


            //Get all auto results and Tag the validation code
            SipperFilters.ApplyAutoValidationCodeF1TightFilter(autoResults);

            var autoYESIDsOnly = autoResults.Where(p => p.ValidationCode == ValidationCode.Yes).Select(p => p.TargetID).ToList();

            var manualResultsWithAutoYES = (from n in originalResults where autoYESIDsOnly.Contains(n.TargetID) select n).ToList();




            string outputFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset";
            string outputFile = outputFolder + "\\" + Path.GetFileName(autoResultsFile).Replace("_results.txt", "_manual_But_AutoYES_results.txt");

            var exporter = new SipperResultToLcmsFeatureExporter(outputFile);
            exporter.ExportResults(manualResultsWithAutoYES);




        }



        //[Ignore("Old")]
        //[Test]
        //public void ParameterOptimization_forLabeledPeptides_Old()
        //{

        //    string manualResultsFile =
        //        @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_ALL_validated.txt";


        //    string autoResultsFile =
        //        @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";


        //    SipperResultFromTextImporter importer = new SipperResultFromTextImporter(manualResultsFile);
        //    var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

        //    importer = new SipperResultFromTextImporter(autoResultsFile);
        //    var autoResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

        //    var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
        //    var noResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.No).ToList();
        //    var maybeResults = originalResults.Where(p => p.ValidationCode == ValidationCode.Maybe).ToList();

        //    var manualResults = yesResultsOnly;

        //    double maxRatio = 0;


        //    StringBuilder sb = new StringBuilder();

        //    List<ParameterOptimizationDataItem> optimizationData = new List<ParameterOptimizationDataItem>();

        //    sb.Append("rsquared\tarea\tiscore\tshared\tuniqueToAuto\tratio\n");

        //    for (double rsquaredVal = 0.55; rsquaredVal < 1.1; rsquaredVal = rsquaredVal + 0.01)
        //    {
        //        for (double area = 0; area < 10.5; area = area + 0.5)
        //        {
        //            for (double iscore = 0; iscore < 0.50; iscore = iscore + 0.02)
        //            {
        //                for (double chromCorr = 0.5; chromCorr <= 1.0; chromCorr = chromCorr + 0.05)
        //                {

        //                    var filteredResults = (from n in autoResults
        //                                           where n.IScore <= iscore &&
        //                                                 n.AreaUnderRatioCurveRevised >= area &&
        //                                               // n.RSquaredValForRatioCurve >= rsquaredVal &&
        //                                                 n.ChromCorrelationMedian >= chromCorr
        //                                           select n).ToList();

        //                    var intersectResults = filteredResults.Select(p => p.TargetID).Intersect(manualResults.Select(p => p.TargetID)).ToList();
        //                    var uniqueToAuto = filteredResults.Select(p => p.TargetID).Except(manualResults.Select(p => p.TargetID)).ToList();
        //                    var ratio = intersectResults.Count / (double)uniqueToAuto.Count;


        //                    ParameterOptimizationDataItem optimizationDataItem = new ParameterOptimizationDataItem(rsquaredVal, area, iscore, chromCorr, 0,
        //                                                          intersectResults.Count, uniqueToAuto.Count);


        //                    if (double.IsNaN(ratio))
        //                    {
        //                        ratio = -1;
        //                    }

        //                    optimizationData.Add(optimizationDataItem);

        //                    sb.Append(rsquaredVal + "\t" + area + "\t" + iscore + "\t" + chromCorr + "\t" + intersectResults.Count + "\t" + uniqueToAuto.Count + "\t" + ratio + "\n");


        //                }
        //            }



        //        }

        //    }


        //    int maxNumUniqueToAuto = optimizationData.Select(p => p.UniqueToAuto).Max();


        //    for (int i = 0; i <= maxNumUniqueToAuto; i++)
        //    {
        //        var maxSharedVal = optimizationData.Where(p => p.UniqueToAuto == i).Select(p => p.SharedCount).Max();

        //        Console.WriteLine(i + "\t" + maxSharedVal);


        //    }


        //    Console.WriteLine(sb.ToString());
        //}

        //[Ignore("0ld")]
        //[Test]
        //public void ParameterOptimization_forLabeledPeptides_testing()
        //{

        //    string manualResultsFile =
        //        @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_ALL_validated.txt";


        //    string autoResultsFile =
        //        @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";

        //    autoResultsFile =
        //        @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Yellow_C13_070_23Mar10_Griffin_10-01-28_rsquared1_results.txt";



        //    SipperResultFromTextImporter importer = new SipperResultFromTextImporter(manualResultsFile);
        //    var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

        //    importer = new SipperResultFromTextImporter(autoResultsFile);
        //    var autoResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

        //    var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
        //    var noResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.No).ToList();
        //    var maybeResults = originalResults.Where(p => p.ValidationCode == ValidationCode.Maybe).ToList();

        //    var manualResults = yesResultsOnly;

        //    double maxRatio = 0;


        //    StringBuilder sb = new StringBuilder();

        //    List<ParameterOptimizationDataItem> optimizationData = new List<ParameterOptimizationDataItem>();

        //    sb.Append("rsquared\tarea\tiscore\tshared\tuniqueToAuto\tratio\n");

        //    for (double rsquaredVal = 0.55; rsquaredVal < 1.1; rsquaredVal = rsquaredVal + 0.01)
        //    {
        //        for (double area = 0; area < 10.5; area = area + 0.5)
        //        {
        //            for (double iscore = 0; iscore < 0.50; iscore = iscore + 0.02)
        //            {
        //                for (double chromCorr = 0.5; chromCorr <= 1.0; chromCorr = chromCorr + 0.05)
        //                {

        //                    var filteredResults = (from n in autoResults
        //                                           where n.IScore <= iscore &&
        //                                                 n.AreaUnderRatioCurveRevised >= area &&
        //                                               //n.RSquaredValForRatioCurve >= rsquaredVal &&
        //                                                 n.ChromCorrelationMedian >= chromCorr
        //                                           select n).ToList();

        //                    var intersectResults = filteredResults.Select(p => p.TargetID).Intersect(manualResults.Select(p => p.TargetID)).ToList();
        //                    var uniqueToAuto = filteredResults.Select(p => p.TargetID).Except(manualResults.Select(p => p.TargetID)).ToList();
        //                    var ratio = intersectResults.Count / (double)uniqueToAuto.Count;


        //                    ParameterOptimizationDataItem optimizationDataItem = new ParameterOptimizationDataItem(rsquaredVal, area, iscore, chromCorr, 0,
        //                                                          intersectResults.Count, uniqueToAuto.Count);


        //                    if (double.IsNaN(ratio))
        //                    {
        //                        ratio = -1;
        //                    }

        //                    optimizationData.Add(optimizationDataItem);

        //                    sb.Append(rsquaredVal + "\t" + area + "\t" + iscore + "\t" + chromCorr + "\t" + intersectResults.Count + "\t" + uniqueToAuto.Count + "\t" + ratio + "\n");


        //                }
        //            }



        //        }

        //    }


        //    int maxNumUniqueToAuto = optimizationData.Select(p => p.UniqueToAuto).Max();


        //    for (int i = 0; i <= maxNumUniqueToAuto; i++)
        //    {
        //        var maxSharedVal = optimizationData.Where(p => p.UniqueToAuto == i).Select(p => p.SharedCount).Max();

        //        Console.WriteLine(i + "\t" + maxSharedVal);


        //    }


        //    Console.WriteLine(sb.ToString());
        //}



    }
}
