using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{

    public class ParameterOptimizationDataItem
    {

        public ParameterOptimizationDataItem(double rsquaredVal, double area, double iscore, double chromCorr, int sharedCount, int uniqueToAuto)
        {
            RSquareVal = rsquaredVal;
            Area = area;
            IScore = iscore;
            ChromCorr = chromCorr;
            SharedCount = sharedCount;
            UniqueToAuto = uniqueToAuto;
        }

        public double Area { get; set; }
        public double RSquareVal { get; set; }
        public double IScore { get; set; }
        public double ChromCorr { get; set; }
        public int SharedCount { get; set; }
        public int UniqueToAuto { get; set; }

    }


    [TestFixture]
    public class FigureXX_manualAnalysisOfDataset
    {
        [Test]
        public void CollectAnnotatedResultsAndOutput()
        {

            //NOTE:  annotated results were in two files. Will use the original results and iterate over these and update the annotations.


            string originalResultsFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";

            string firstAnnotatedFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_NR_results_validated.txt";

            string secondAnnotatedFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_NR_F1_results_validated.txt";

            string exportedAnnotedFile = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_all_validated.txt";


            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(originalResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(firstAnnotatedFile);
            var annoResults1 = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(secondAnnotatedFile);
            var annoResults2 = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();



            foreach (var originalResult in originalResults)
            {

                if (!SipperFilters.PassesF1Filter(originalResult))
                {
                    originalResult.ValidationCode = ValidationCode.Maybe;
                }
                else
                {
                    SipperLcmsFeatureTargetedResultDTO annoResult1 = annoResults1.FirstOrDefault(p => p.TargetID == originalResult.TargetID);
                    if (annoResult1 != null)
                    {
                        originalResult.ValidationCode = annoResult1.ValidationCode;
                    }


                    SipperLcmsFeatureTargetedResultDTO annoResult2 = annoResults2.FirstOrDefault(p => p.TargetID == originalResult.TargetID);
                    if (annoResult2 != null)
                    {
                        originalResult.ValidationCode = annoResult2.ValidationCode;
                    }


                }
            }


            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportedAnnotedFile);
            exporter.ExportResults(originalResults);




        }

        [Test]
        public void SortResultsIntoAnnotationCatagoriesAndExport()
        {
            string originalResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_ALL_validated.txt";



            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(originalResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
            var noResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.No).ToList();
            var maybeResults = originalResults.Where(p => p.ValidationCode == ValidationCode.Maybe).ToList();
            var noneResults = originalResults.Where(p => p.ValidationCode == ValidationCode.None).ToList();

            string exportedYesResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_YES_validated.txt";

            string exportedNoResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_NO_validated.txt";

            string exportedMaybeResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_MAYBE_validated.txt";


            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportedYesResultsFile);
            exporter.ExportResults(yesResultsOnly);

            exporter = new SipperResultToLcmsFeatureExporter(exportedNoResultsFile);
            exporter.ExportResults(noResultsOnly);

            exporter = new SipperResultToLcmsFeatureExporter(exportedMaybeResultsFile);
            exporter.ExportResults(maybeResults);


            Console.WriteLine("Yes\t" + yesResultsOnly.Count);
            Console.WriteLine("No\t" + noResultsOnly.Count);
            Console.WriteLine("Maybe\t" + maybeResults.Count);
            Console.WriteLine("No annotation\t" + noneResults.Count);
        }

        [Test]
        public void GetStatsOnResults()
        {
            string originalResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_ALL_validated.txt";


            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(originalResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
            var noResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.No).ToList();
            var maybeResults = originalResults.Where(p => p.ValidationCode == ValidationCode.Maybe).ToList();


            var currentResultsToAnalyze = maybeResults;


            var statsInfo = GetStatsData(currentResultsToAnalyze);

            Console.WriteLine(statsInfo);


        }

        [Test]
        public void GetComparisonsBetweenAutoAndManualAnalysis()
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
            var maybeResults = originalResults.Where(p => p.ValidationCode == ValidationCode.Maybe).ToList();

            var looseFilteredResults = autoResults.Where(SipperFilters.PassesLabelLooseFilterF2).ToList();
            var tightFilteredResults = autoResults.Where(SipperFilters.PassesLabelTightFilterF1).ToList();
            

            var manualResults = yesResultsOnly;
            var intersectResults = tightFilteredResults.Select(p => p.TargetID).Intersect(manualResults.Select(p => p.TargetID)).ToList();
            var uniqueToManual = manualResults.Select(p => p.TargetID).Except(tightFilteredResults.Select(p => p.TargetID)).ToList();
            var uniqueToAuto = tightFilteredResults.Select(p => p.TargetID).Except(manualResults.Select(p => p.TargetID)).ToList();
            var problemResults = tightFilteredResults.Where(r => uniqueToAuto.Contains(r.TargetID)).ToList();
            var uniqueToManualResults = manualResults.Where(r => uniqueToManual.Contains(r.TargetID)).ToList();

            Console.WriteLine("Manual count= \t" + manualResults.Count);
            Console.WriteLine("Auto count - looseFilter= \t" + looseFilteredResults.Count);
            Console.WriteLine("Auto count - tightFilter= \t" + tightFilteredResults.Count);

            Console.WriteLine("Shared count - tight = \t" + intersectResults.Count);
            Console.WriteLine("Unique to Manual - tight = \t" + uniqueToManual.Count);
            Console.WriteLine("Unique to Auto- tight = \t" + uniqueToAuto.Count);

            var statsInfo = GetStatsData(problemResults);

            Console.WriteLine();
            Console.WriteLine(statsInfo);

            //intersectResults = f4FilteredResults.Select(p => p.TargetID).Intersect(manualResults.Select(p => p.TargetID)).ToList();
            //uniqueToManual = manualResults.Select(p => p.TargetID).Except(f4FilteredResults.Select(p => p.TargetID)).ToList();
            //uniqueToAuto = f4FilteredResults.Select(p => p.TargetID).Except(manualResults.Select(p => p.TargetID)).ToList();
            //problemResults = f4FilteredResults.Where(r => uniqueToAuto.Contains(r.TargetID)).ToList();
            //uniqueToManualResults = manualResults.Where(r => uniqueToManual.Contains(r.TargetID)).ToList();

            //Console.WriteLine("Manual count= \t" + manualResults.Count);
            //Console.WriteLine("Auto count= \t" + f4FilteredResults.Count);
            //Console.WriteLine("Shared count = \t" + intersectResults.Count);
            //Console.WriteLine("Unique to Manual = \t" + uniqueToManual.Count);
            //Console.WriteLine("Unique to Auto = \t" + uniqueToAuto.Count);

            //statsInfo = GetStatsData(problemResults);

            //Console.WriteLine();
            //Console.WriteLine(statsInfo);

            //string exportedResultsFileName =
            //       @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_filtered_results.txt";

            //SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportedResultsFileName);
            //exporter.ExportResults(problemResults);

        }


        [Test]
        public void ParameterOptimization_forLabeledPeptides()
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
            var maybeResults = originalResults.Where(p => p.ValidationCode == ValidationCode.Maybe).ToList();

            var manualResults = yesResultsOnly;

            double maxRatio = 0;


            StringBuilder sb = new StringBuilder();

            List<ParameterOptimizationDataItem> optimizationData = new List<ParameterOptimizationDataItem>();

            sb.Append("rsquared\tarea\tiscore\tshared\tuniqueToAuto\tratio\n");

            for (double rsquaredVal = 0.55; rsquaredVal < 1.1; rsquaredVal = rsquaredVal + 0.01)
            {
                for (double area = 0; area < 10.5; area = area + 0.5)
                {
                    for (double iscore = 0; iscore < 0.50; iscore = iscore + 0.02)
                    {
                        for (double chromCorr = 0.5; chromCorr <= 1.0; chromCorr = chromCorr + 0.05)
                        {

                            var f3FilteredResults = (from n in autoResults
                                                     where n.IScore <= iscore &&
                                                           n.AreaUnderRatioCurveRevised >= area &&
                                                           n.RSquaredValForRatioCurve >= rsquaredVal &&
                                                           n.ChromCorrelationMedian >= chromCorr
                                                     select n).ToList();

                            var intersectResults = f3FilteredResults.Select(p => p.TargetID).Intersect(manualResults.Select(p => p.TargetID)).ToList();
                            var uniqueToAuto = f3FilteredResults.Select(p => p.TargetID).Except(manualResults.Select(p => p.TargetID)).ToList();
                            var ratio = intersectResults.Count / (double)uniqueToAuto.Count;


                            ParameterOptimizationDataItem optimizationDataItem = new ParameterOptimizationDataItem(rsquaredVal, area, iscore, chromCorr,
                                                                  intersectResults.Count, uniqueToAuto.Count);


                            if (double.IsNaN(ratio))
                            {
                                ratio = -1;
                            }

                            optimizationData.Add(optimizationDataItem);

                            sb.Append(rsquaredVal + "\t" + area + "\t" + iscore + "\t" + chromCorr + "\t" + intersectResults.Count + "\t" + uniqueToAuto.Count + "\t" + ratio + "\n");


                        }
                    }



                }

            }


            int maxNumUniqueToAuto = optimizationData.Select(p => p.UniqueToAuto).Max();


            for (int i = 0; i <= maxNumUniqueToAuto; i++)
            {
                var maxSharedVal = optimizationData.Where(p => p.UniqueToAuto == i).Select(p => p.SharedCount).Max();

                Console.WriteLine(i + "\t" + maxSharedVal);


            }


            Console.WriteLine(sb.ToString());
        }

        [Test]
        public void ParameterOptimization_forNonLabeledPeptides()
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
            var maybeResults = originalResults.Where(p => p.ValidationCode == ValidationCode.Maybe).ToList();

            var manualResults = noResultsOnly;

            double maxRatio = 0;


            StringBuilder sb = new StringBuilder();

            List<ParameterOptimizationDataItem> optimizationData = new List<ParameterOptimizationDataItem>();

            sb.Append("rsquared\tarea\tiscore\tshared\tuniqueToAuto\tratio\n");

            for (double rsquaredVal = 0; rsquaredVal < 1.1; rsquaredVal = rsquaredVal + 0.1)
            {
                for (double area = 0; area < 10; area = area + 0.5)
                {
                    for (double iscore = 0; iscore < 0.50; iscore = iscore + 0.02)
                    {
                        for (double chromCorr = 0.5; chromCorr <= 1.0; chromCorr = chromCorr + 0.05)
                        {

                            var f3FilteredResults = (from n in autoResults
                                                     where n.IScore <= iscore &&
                                                           n.AreaUnderRatioCurveRevised <= area &&
                                                           n.RSquaredValForRatioCurve <= rsquaredVal &&
                                                           n.ChromCorrelationMedian >= chromCorr
                                                     select n).ToList();

                            var intersectResults = f3FilteredResults.Select(p => p.TargetID).Intersect(manualResults.Select(p => p.TargetID)).ToList();
                            var uniqueToAuto = f3FilteredResults.Select(p => p.TargetID).Except(manualResults.Select(p => p.TargetID)).ToList();
                            var ratio = intersectResults.Count / (double)uniqueToAuto.Count;

                            ParameterOptimizationDataItem optimizationDataItem = new ParameterOptimizationDataItem(rsquaredVal, area, iscore, chromCorr,
                                                                  intersectResults.Count, uniqueToAuto.Count);


                            if (double.IsNaN(ratio))
                            {
                                ratio = -1;
                            }

                            optimizationData.Add(optimizationDataItem);

                            sb.Append(rsquaredVal + "\t" + area + "\t" + iscore + "\t" + chromCorr + "\t" + intersectResults.Count + "\t" + uniqueToAuto.Count + "\t" + ratio + "\n");


                        }
                    }



                }

            }


            int maxNumUniqueToAuto = optimizationData.Select(p => p.UniqueToAuto).Max();


            for (int i = 0; i <= maxNumUniqueToAuto; i++)
            {
                var sharedData = optimizationData.Where(p => p.UniqueToAuto == i).Select(p => p.SharedCount);

                int maxSharedVal;
                if (!sharedData.Any())
                {
                    maxSharedVal = 0;
                }
                else
                {
                    maxSharedVal = optimizationData.Where(p => p.UniqueToAuto == i).Select(p => p.SharedCount).Max();    
                }

                
                

                Console.WriteLine(i + "\t" + maxSharedVal);


            }


            //Console.WriteLine(sb.ToString());
        }




        [Test]
        public void FilterOutRedundantResultsFromResultFiles()
        {
            //NOTE:  don't really need to do this now that I'm filtering on STAC


            string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Analysis\FigureXX_ManualAnalysisOfDataset";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Analysis\FigureXX_ManualAnalysisOfDataset\Output";

            int featureIDColIndex = 1;


            var dirInfo = new DirectoryInfo(sourceFolder);

            var fileInfos = dirInfo.GetFiles("*.txt");

            foreach (var fileInfo in fileInfos)
            {

                string outputFileName = outputFolder + Path.DirectorySeparatorChar + fileInfo.Name.Replace(".txt", "_NR.txt");

                //if (File.Exists(outputFileName)) File.Delete(outputFileName);

                using (StreamWriter writer = new StreamWriter(outputFileName))
                {
                    List<int> featureIDs = new List<int>();

                    using (StreamReader reader = new StreamReader(fileInfo.FullName))
                    {
                        writer.WriteLine(reader.ReadLine());


                        int counter = 0;
                        while (reader.Peek() != -1)
                        {
                            string line = reader.ReadLine();

                            var parsedLine = line.Split('\t');

                            int featureID = Convert.ToInt32(parsedLine[featureIDColIndex]);


                            if (!featureIDs.Contains(featureID))
                            {
                                counter++;
                                writer.WriteLine(line);
                                featureIDs.Add(featureID);
                            }

                        }

                        Console.WriteLine(fileInfo.Name + "\t" + counter);

                        reader.Close();
                    }

                    writer.Close();

                }
            }
        }



        public static string GetStatsData(List<SipperLcmsFeatureTargetedResultDTO> currentResultsToAnalyze)
        {
            StringBuilder sb = new StringBuilder();
            var fitScoreAverage = currentResultsToAnalyze.Average(p => p.FitScore);
            var fitScoreMedian = MathUtils.GetMedian(currentResultsToAnalyze.Select(p => (double)p.FitScore).ToList());
            var fitscoreSD = MathUtils.GetStDev(currentResultsToAnalyze.Select(p => (double)p.FitScore).ToList());

            var iScoreAverage = currentResultsToAnalyze.Average(p => p.IScore);
            var iScoreMedian = MathUtils.GetMedian(currentResultsToAnalyze.Select(p => (double)p.IScore).ToList());
            var iScoreSD = MathUtils.GetStDev(currentResultsToAnalyze.Select(p => (double)p.IScore).ToList());

            var intensityAverage = currentResultsToAnalyze.Average(p => p.Intensity);
            var intensityMedian = MathUtils.GetMedian(currentResultsToAnalyze.Select(p => (double)p.Intensity).ToList());
            var intensitySD = MathUtils.GetStDev(currentResultsToAnalyze.Select(p => (double)p.Intensity).ToList());

            var areaAverage = currentResultsToAnalyze.Average(p => p.AreaUnderRatioCurveRevised);
            var areaMedian =
                MathUtils.GetMedian(currentResultsToAnalyze.Select(p => (double)p.AreaUnderRatioCurveRevised).ToList());
            var areaSD = MathUtils.GetStDev(currentResultsToAnalyze.Select(p => (double)p.AreaUnderRatioCurveRevised).ToList());

            var numHQPeaksAverage = currentResultsToAnalyze.Average(p => p.NumHighQualityProfilePeaks);
            var numHQPeaksMedian =
                MathUtils.GetMedian(currentResultsToAnalyze.Select(p => (double)p.NumHighQualityProfilePeaks).ToList());
            var numHQPeaksSD =
                MathUtils.GetStDev(currentResultsToAnalyze.Select(p => (double)p.NumHighQualityProfilePeaks).ToList());

            var numCarbonsAverage = currentResultsToAnalyze.Average(p => p.NumCarbonsLabelled);
            var numCarbonsMedian =
                MathUtils.GetMedian(currentResultsToAnalyze.Select(p => (double)p.NumCarbonsLabelled).ToList());
            var numCarbonsSD = MathUtils.GetStDev(currentResultsToAnalyze.Select(p => (double)p.NumCarbonsLabelled).ToList());

            var percentPeptidesAverage = currentResultsToAnalyze.Average(p => p.PercentPeptideLabelled);
            var percentPeptidesMedian =
                MathUtils.GetMedian(currentResultsToAnalyze.Select(p => (double)p.PercentPeptideLabelled).ToList());
            var percentPeptidesSD =
                MathUtils.GetStDev(currentResultsToAnalyze.Select(p => (double)p.PercentPeptideLabelled).ToList());

            var chromCorrMedianAverage = currentResultsToAnalyze.Average(p => p.ChromCorrelationMedian);
            var chromCorrMedianMedian =
                MathUtils.GetMedian(currentResultsToAnalyze.Select(p => (double)p.ChromCorrelationMedian).ToList());
            var chromCorrSD = MathUtils.GetStDev(currentResultsToAnalyze.Select(p => (double)p.ChromCorrelationMedian).ToList());

            var monomassAverage = currentResultsToAnalyze.Average(p => p.MonoMass);
            var monomassMedian = MathUtils.GetMedian(currentResultsToAnalyze.Select(p => (double)p.MonoMass).ToList());
            var monomassSD = MathUtils.GetStDev(currentResultsToAnalyze.Select(p => (double)p.MonoMass).ToList());

            var rsquaredAverage = currentResultsToAnalyze.Average(p => p.RSquaredValForRatioCurve);
            var rsquaredsMedian =
                MathUtils.GetMedian(currentResultsToAnalyze.Select(p => (double)p.RSquaredValForRatioCurve).ToList());
            var rsquaredSD =
                MathUtils.GetStDev(currentResultsToAnalyze.Select(p => (double)p.RSquaredValForRatioCurve).ToList());

            sb.Append("n= \t" + currentResultsToAnalyze.Count);
            sb.Append(Environment.NewLine);


            sb.Append("fit");
            sb.Append("\t");
            sb.Append(fitScoreAverage);
            sb.Append("\t");
            sb.Append(fitScoreMedian);
            sb.Append("\t");
            sb.Append(fitscoreSD);

            sb.Append(Environment.NewLine);

            sb.Append("iscore");
            sb.Append("\t");
            sb.Append(iScoreAverage);
            sb.Append("\t");
            sb.Append(iScoreMedian);
            sb.Append("\t");
            sb.Append(iScoreSD);

            sb.Append(Environment.NewLine);

            sb.Append("intensity");
            sb.Append("\t");
            sb.Append(intensityAverage);
            sb.Append("\t");
            sb.Append(intensityMedian);
            sb.Append("\t");
            sb.Append(intensitySD);

            sb.Append(Environment.NewLine);

            sb.Append("area");
            sb.Append("\t");
            sb.Append(areaAverage);
            sb.Append("\t");
            sb.Append(areaMedian);
            sb.Append("\t");
            sb.Append(areaSD);

            sb.Append(Environment.NewLine);

            sb.Append("numPeaks");
            sb.Append("\t");
            sb.Append(numHQPeaksAverage);
            sb.Append("\t");
            sb.Append(numHQPeaksMedian);
            sb.Append("\t");
            sb.Append(numHQPeaksSD);

            sb.Append(Environment.NewLine);

            sb.Append("numCarbons");
            sb.Append("\t");
            sb.Append(numCarbonsAverage);
            sb.Append("\t");
            sb.Append(numCarbonsMedian);
            sb.Append("\t");
            sb.Append(numCarbonsSD);

            sb.Append(Environment.NewLine);

            sb.Append("percentPeptides");
            sb.Append("\t");
            sb.Append(percentPeptidesAverage);
            sb.Append("\t");
            sb.Append(percentPeptidesMedian);
            sb.Append("\t");
            sb.Append(percentPeptidesSD);

            sb.Append(Environment.NewLine);

            sb.Append("chromCorr");
            sb.Append("\t");
            sb.Append(chromCorrMedianAverage);
            sb.Append("\t");
            sb.Append(chromCorrMedianMedian);
            sb.Append("\t");
            sb.Append(chromCorrSD);

            sb.Append(Environment.NewLine);

            sb.Append("monoMass");
            sb.Append("\t");
            sb.Append(monomassAverage);
            sb.Append("\t");
            sb.Append(monomassMedian);
            sb.Append("\t");
            sb.Append(monomassSD);

            sb.Append(Environment.NewLine);

            sb.Append("rsquared");
            sb.Append("\t");
            sb.Append(rsquaredAverage);
            sb.Append("\t");
            sb.Append(rsquaredsMedian);
            sb.Append("\t");
            sb.Append(rsquaredSD);

            sb.Append(Environment.NewLine);
            return sb.ToString();
        }
    }
}
