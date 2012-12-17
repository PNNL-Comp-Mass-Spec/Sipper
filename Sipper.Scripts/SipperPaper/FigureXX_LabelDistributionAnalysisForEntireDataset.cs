using System;
using System.IO;
using System.Linq;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using DeconTools.Workflows.Backend.Utilities;
using GWSGraphLibrary.GraphGenerator;
using MSFeatureStatsAnalysis;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FigureXX_LabelDistributionAnalysisForEntireDataset
    {



        [Test]
        public void ApplyFilterF1_on_eachResultFile()
        {
               string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Non-redundant\Yellow_C13";


            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Filter_F1";

            int featureIDColIndex = 1;


            var dirInfo = new DirectoryInfo(sourceFolder);

            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);


            var fileInfos = dirInfo.GetFiles("*_results.txt");

            foreach (var fileInfo in fileInfos)
            {

                var outputFileName = outputFolder + Path.DirectorySeparatorChar +  fileInfo.Name.Replace("_results.txt", "_F1_results.txt");


                SipperResultFromTextImporter importer = new SipperResultFromTextImporter(fileInfo.FullName);
                var repo = importer.Import();

                foreach (var sipperLcmsFeatureTargetedResultDto in repo.Results)
                {
                    ApplyFilterF1(sipperLcmsFeatureTargetedResultDto as SipperLcmsFeatureTargetedResultDTO);

                }

                var exportedResults =
                    repo.Results.Where(p => ((SipperLcmsFeatureTargetedResultDTO) p).PassesFilter);



                SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(outputFileName);
                exporter.ExportResults(exportedResults);


                Console.WriteLine(fileInfo.Name + "\t" + repo.Results.Count + "\t" + exportedResults.Count());

            }



        }





        [Test]
        public void MergeResults()
        {
            string resultFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Non-redundant\Yellow_C13";
            string outputFile = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Non-redundant\Yellow_C13_Merged_results.txt";
            ResultUtilities.MergeResultFiles(resultFolder, outputFile);

            resultFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Non-redundant\Yellow_C12";
            outputFile = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Non-redundant\Yellow_C12_Merged_results.txt";
            ResultUtilities.MergeResultFiles(resultFolder, outputFile);

        }







        [Test]
        public void GetHistograms()
        {
            string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Non-redundant";


            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Histograms";

            int featureIDColIndex = 1;


            var dirInfo = new DirectoryInfo(sourceFolder);

            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);


            var fileInfos = dirInfo.GetFiles("*_results.txt");

            foreach (var fileInfo in fileInfos)
            {

                SipperResultFromTextImporter importer = new SipperResultFromTextImporter(fileInfo.FullName);
                var repo = importer.Import();

                foreach (var sipperLcmsFeatureTargetedResultDto in repo.Results)
                {
                    ApplyFilterF1(sipperLcmsFeatureTargetedResultDto as SipperLcmsFeatureTargetedResultDTO);

                }

                var exportedResults =
                    repo.Results.Where(p => ((SipperLcmsFeatureTargetedResultDTO)p).PassesFilter);

                var incorporationValues = exportedResults.Select(p => ((SipperLcmsFeatureTargetedResultDTO)p).PercentCarbonsLabelled).ToList();
                var peptideLabelingValues = exportedResults.Select(p => ((SipperLcmsFeatureTargetedResultDTO)p).PercentPeptideLabelled).ToList();
                var ratioAreaValues = exportedResults.Select(p => ((SipperLcmsFeatureTargetedResultDTO)p).AreaUnderRatioCurveRevised).ToList();



                MSFeatureStatsAnalysis.StatsBuilder statsBuilder = new StatsBuilder();
                var binList = statsBuilder.BuildBinTable(0, 2, 0.01);
                statsBuilder.GetStats(incorporationValues, binList);
                var incorporationXvals = binList.Select(p => (double)p.Value).ToArray();
                var incorporationYvals = binList.Select(p => (double)p.Count).ToArray();
                var secondHighestIncorp = incorporationYvals.OrderByDescending(p => p).Skip(1).First();

                binList = statsBuilder.BuildBinTable(0, 40, 0.5);
                statsBuilder.GetStats(peptideLabelingValues, binList);
                var peptideLabelingXvals = binList.Select(p => (double)p.Value).ToArray();
                var peptideLabelingYvals = binList.Select(p => (double)p.Count).ToArray();
                var secondHighestpeptideLabeling = peptideLabelingYvals.OrderByDescending(p => p).Skip(1).First();


                //------------------------------ incorporation histograms --------------------------------------------------
                GWSGraphLibrary.GraphGenerator.HistogramGraphGenerator graphGenerator = new HistogramGraphGenerator();
                graphGenerator.GraphWidth = 600;
                graphGenerator.GraphHeight = 600;

                graphGenerator.GenerateGraph(incorporationXvals, incorporationYvals, -0.5, incorporationXvals.Max());

                string outputHistogramFileName = outputFolder + Path.DirectorySeparatorChar +
                                                 fileInfo.Name.ToLower().Replace("_results.txt", "_incorpHist.png");

                graphGenerator.SaveGraph(outputHistogramFileName);


                graphGenerator.GenerateGraph(incorporationXvals, incorporationYvals, 0.5, incorporationXvals.Max(), 0, secondHighestIncorp);
                outputHistogramFileName = outputFolder + Path.DirectorySeparatorChar +
                                                 fileInfo.Name.ToLower().Replace("_results.txt", "_incorpHist2.png");

                graphGenerator.SaveGraph(outputHistogramFileName);


                //----------------------------- peptide percentage histograms -----------------------------------------------
                graphGenerator = new HistogramGraphGenerator();
                graphGenerator.GraphWidth = 600;
                graphGenerator.GraphHeight = 600;

                graphGenerator.GenerateGraph(peptideLabelingXvals, peptideLabelingYvals, -0.5, peptideLabelingXvals.Max());

                string outputPeptidePercentHistogramFileName = outputFolder + Path.DirectorySeparatorChar +
                                                 fileInfo.Name.ToLower().Replace("_results.txt", "_peptidePercentHist.png");

                graphGenerator.SaveGraph(outputPeptidePercentHistogramFileName);


                graphGenerator.GenerateGraph(peptideLabelingXvals, peptideLabelingYvals, 0.5, peptideLabelingXvals.Max(), 0, secondHighestpeptideLabeling);
                outputPeptidePercentHistogramFileName = outputFolder + Path.DirectorySeparatorChar +
                                                 fileInfo.Name.ToLower().Replace("_results.txt", "_peptidePercentHist2.png");

                graphGenerator.SaveGraph(outputPeptidePercentHistogramFileName);



                Console.WriteLine(fileInfo.Name + "\t" + repo.Results.Count + "\t" + exportedResults.Count());

                //SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(outputFileName);
                //exporter.ExportResult s(exportedResults);


                //--------------------------   ratioArea histograms --------------------------------------

                binList = statsBuilder.BuildBinTable(0, 150, 1);
                statsBuilder.GetStats(ratioAreaValues, binList);
                var ratioXVals = binList.Select(p => (double)p.Value).ToArray();
                var ratioYVals = binList.Select(p => (double)p.Count).ToArray();


                graphGenerator = new HistogramGraphGenerator();
                graphGenerator.GraphWidth = 600;
                graphGenerator.GraphHeight = 600;




                var maxX = ratioXVals.Max();
                maxX = maxX + maxX * 0.05;

                var minX = ratioXVals.Min();
                minX = minX - (maxX - minX) * 0.05;

                graphGenerator.GenerateGraph(ratioXVals, ratioYVals, minX, maxX);

                string ratioAreaHistogramFilename = outputFolder + Path.DirectorySeparatorChar +
                                                 fileInfo.Name.ToLower().Replace("_results.txt", "_ratioHistRSq.png");

                graphGenerator.SaveGraph(ratioAreaHistogramFilename);


            }
        }







        private void ApplyFilterF1(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.NumHighQualityProfilePeaks>1 && result.Intensity>0)
            {
                result.PassesFilter = true;
            }
        }


        private void ApplyFilterF1b(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.ChromCorrelationMedian > 0.90 && result.ChromCorrelationAverage > 0.85)
            {
                if (result.RSquaredValForRatioCurve > 0.8 && result.IScore < 0.2)
                {
                    result.PassesFilter = true;
                }

            }
        }


        private void ApplyFilterF1c(SipperLcmsFeatureTargetedResultDTO result)
        {

            if (result.AreaUnderRatioCurveRevised > 0)
            {
                if (result.ChromCorrelationAverage > 0.85 && result.ChromCorrelationMedian > 0.9)
                {
                    if (result.RSquaredValForRatioCurve > 0.85)
                    {

                        //high quality results
                        if (result.RSquaredValForRatioCurve > 0.95 && result.AreaUnderRatioCurve > 75)
                        {
                            if (result.IScore <= 0.4)
                            {
                                result.PassesFilter = true;
                            }
                        }
                        //high quality results
                        else if (result.RSquaredValForRatioCurve > 0.5 && result.AreaUnderRatioCurveRevised > 15)
                        {
                            if (result.IScore <= 0.4)
                            {
                                result.PassesFilter = true;
                            }
                        }
                        else if (result.ChromCorrelationMedian > 0.99 && result.ChromCorrelationAverage > 0.99)
                        {
                            if (result.IScore <= 0.4)
                            {
                                result.PassesFilter = true;
                            }
                        }
                        //high intensity results
                        else if (result.RSquaredValForRatioCurve > 0.95 && result.Intensity > 1e5)
                        {
                            if (result.IScore <= 0.4)
                            {
                                result.PassesFilter = true;
                            }

                        }
                        //all other results - 
                        else if (result.RSquaredValForRatioCurve > 0.95 && result.IScore <= 0.25)
                        {
                            result.PassesFilter = true;
                        }

                    }


                }
            }

        }

    }
}
