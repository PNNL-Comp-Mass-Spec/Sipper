using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using GwsDMSUtilities;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FigureXX_UnidentifiedFeatures
    {


        [Category("Paper")]
        [Test]
        public void Compare_Identified_VS_Averagine_for_previouslyIDedTargets()
        {
            string normalResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";

            string averagineResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_UnidentifiedFeatures\Study1 - effect of using averagine\Yellow_C13_070_23Mar10_Griffin_10-01-28_AVERAGINE_2013_04_08_results.txt";

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(normalResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(averagineResultsFile);
            var averagineResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var originalTightFilter = SipperFilters.ApplyAutoValidationCodeF1TightFilter(originalResults).Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
            var averagineTightFilter = SipperFilters.ApplyAveragineBasedTightFilter(averagineResults).Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
            var intersectResults = originalTightFilter.Select(p => p.TargetID).Intersect(averagineTightFilter.Select(p => p.TargetID)).ToList();
            var uniqueToOriginal = originalTightFilter.Select(p => p.TargetID).Except(averagineTightFilter.Select(p => p.TargetID)).ToList();
            var uniqueToAveragine = averagineTightFilter.Select(p => p.TargetID).Except(originalTightFilter.Select(p => p.TargetID)).ToList();

            Console.WriteLine("OriginalFiltered count= \t" + originalTightFilter.Count);
            Console.WriteLine("AveragineFiltered count= \t" + averagineTightFilter.Count);
            Console.WriteLine("Shared count = \t" + intersectResults.Count);
            Console.WriteLine("Unique to original = \t" + uniqueToOriginal.Count);
            Console.WriteLine("Unique to averagine = \t" + uniqueToAveragine.Count);

            var originalLooseFilter = SipperFilters.ApplyAutoValidationCodeF2LooseFilter(originalResults).Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
            var averagineLooseFilter = SipperFilters.ApplyAveragineBasedLooseFilter(averagineResults).Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
            var intersectResultsLooseFilter = originalLooseFilter.Select(p => p.TargetID).Intersect(averagineLooseFilter.Select(p => p.TargetID)).ToList();
            uniqueToOriginal = originalLooseFilter.Select(p => p.TargetID).Except(averagineLooseFilter.Select(p => p.TargetID)).ToList();
            uniqueToAveragine = averagineLooseFilter.Select(p => p.TargetID).Except(originalLooseFilter.Select(p => p.TargetID)).ToList();

            Console.WriteLine();
            Console.WriteLine("LooseFilter results------------------");
            Console.WriteLine("OriginalFiltered count= \t" + originalLooseFilter.Count);
            Console.WriteLine("AveragineFiltered count= \t" + averagineLooseFilter.Count);
            Console.WriteLine("Shared count = \t" + intersectResultsLooseFilter.Count);
            Console.WriteLine("Unique to original = \t" + uniqueToOriginal.Count);
            Console.WriteLine("Unique to averagine = \t" + uniqueToAveragine.Count);
        }



        [Category("Paper")]
        [Test]
        public void ParameterOptimization_for_Averagine_LabeledPeptides()
        {

            string manualResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_all_validated.txt";

            string autoResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_UnidentifiedFeatures\Study1 - effect of using averagine\Yellow_C13_070_23Mar10_Griffin_10-01-28_AVERAGINE_2013_04_08_results.txt";


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
            int[] falsePositiveArray = { 0, 2, 3, 4, 6, 15 };
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
        public void GetFilteredOutputOnAutoProcessed_Averagine()
        {


            string manualResultsFile =
           @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_ALL_validated.txt";

            string autoResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_UnidentifiedFeatures\Study1 - effect of using averagine\Yellow_C13_070_23Mar10_Griffin_10-01-28_AVERAGINE_2013_04_08_results.txt";

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(manualResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(autoResultsFile);
            var autoResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();

            var noResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.No).ToList();

            var intersectAutoYes = autoResults.Select(p => p.TargetID).Intersect(yesResultsOnly.Select(p => p.TargetID)).ToList();


            string outputFolder =
              @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_UnidentifiedFeatures\Study1 - effect of using averagine";

            //Get all auto results and Tag the validation code
            List<SipperLcmsFeatureTargetedResultDTO> autoYES = SipperFilters.ApplyAutoValidationCodeF1TightFilter(autoResults);

            var autoYESIDsOnly = autoYES.Where(p => p.ValidationCode == ValidationCode.Yes).Select(p => p.TargetID).ToList();
            var manualResultsWithAutoYES = (from n in originalResults where autoYESIDsOnly.Contains(n.TargetID) select n).ToList();
            string outputFile = outputFolder + "\\" + Path.GetFileName(autoResultsFile).Replace("_results.txt", "_manual_But_AutoTightYES_results.txt");
            var exporter = new SipperResultToLcmsFeatureExporter(outputFile);
            exporter.ExportResults(manualResultsWithAutoYES);

            autoYES = SipperFilters.ApplyAutoValidationCodeF2LooseFilter(autoResults);
            autoYESIDsOnly = autoYES.Where(p => p.ValidationCode == ValidationCode.Yes).Select(p => p.TargetID).ToList();
            manualResultsWithAutoYES = (from n in originalResults where autoYESIDsOnly.Contains(n.TargetID) select n).ToList();
            outputFile = outputFolder + "\\" + Path.GetFileName(autoResultsFile).Replace("_results.txt", "_manual_But_AutoLooseYES_results.txt");
            exporter = new SipperResultToLcmsFeatureExporter(outputFile);
            exporter.ExportResults(manualResultsWithAutoYES);

            autoYES = SipperFilters.ApplyAutoValidationCodeF1TightFilter(autoResults);
            outputFile = outputFolder + "\\" + Path.GetFileName(autoResultsFile).Replace("_results.txt", "_Auto_TightFilter_results.txt");
            exporter = new SipperResultToLcmsFeatureExporter(outputFile);
            exporter.ExportResults(autoYES);

            autoYES = SipperFilters.ApplyAutoValidationCodeF2LooseFilter(autoResults);
            outputFile = outputFolder + "\\" + Path.GetFileName(autoResultsFile).Replace("_results.txt", "_Auto_LooseFilter_results.txt");
            exporter = new SipperResultToLcmsFeatureExporter(outputFile);
            exporter.ExportResults(autoYES);

        }


        [Category("Paper")]
        [Test]
        public void FilterFraction70ForEnrichedFeaturesFromUnidentified()
        {

            string confidentlyIdentifiedResultsFilename =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";


            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(confidentlyIdentifiedResultsFilename);
            var identifiedResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();


            string unidentifiedResultsFilename =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_UnidentifiedFeatures\Study2 - processing all unidentified features Yellow_C13_070\Yellow_C13_070_23Mar10_Griffin_10-01-28_UNIDENTIFIED_2013_04_08_results.txt";

            importer = new SipperResultFromTextImporter(unidentifiedResultsFilename);
            var unidentifiedResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();


            var tightFilteredIdentified = SipperFilters.ApplyAutoValidationCodeF1TightFilter(identifiedResults).Where(n => n.ValidationCode == ValidationCode.Yes).ToList();

            var looseFilteredIdentified = SipperFilters.ApplyAutoValidationCodeF2LooseFilter(identifiedResults).Where(n => n.ValidationCode == ValidationCode.Yes).ToList();

            var tightFilteredUnIdentified = SipperFilters.ApplyAveragineBasedTightFilter(unidentifiedResults).Where(n => n.ValidationCode == ValidationCode.Yes).ToList();

            var looseFilteredUnIdentified = SipperFilters.ApplyAveragineBasedLooseFilter(unidentifiedResults).Where(n => n.ValidationCode == ValidationCode.Yes).ToList();


            Console.WriteLine("Total identified results=\t" + identifiedResults.Count);
            Console.WriteLine("Total Unidentified results=\t " + unidentifiedResults.Count);

            Console.WriteLine("Tight filter, Identified=\t" + tightFilteredIdentified.Count);
            Console.WriteLine("Tight filter, UnIdentified=\t" + tightFilteredUnIdentified.Count);
            Console.WriteLine("Loose filter, Identified=\t" + looseFilteredIdentified.Count);
            Console.WriteLine("Loose filter, UnIdentified=\t" + looseFilteredUnIdentified.Count);
        }





        [Test]
        public void GetPeakMatchingResults_allDatasets()
        {
            GwsDMSUtilities.UMCUtilities umcUtilities = new UMCUtilities();

            int[] jobList = {
                                579982, 579902, 579900, 579898, 579896, 579894, 579892, 579890, 579888, 579886, 579478, 579476, 579474, 579472,
                                579470, 579468, 579466, 579464, 579462, 579460, 579458, 579456, 579454, 579452, 579448, 579446, 579444, 579442,
                                579440, 579438, 579436, 579434, 578838, 578836, 578834, 578832, 578830, 578828, 578826, 578824, 578822, 578820,
                                578818, 578683, 578681, 578679, 578677, 578675, 578673, 578671, 578669, 578667, 578665, 578663, 578661, 578659,
                                578657, 578560, 578558, 578556, 578554, 578419, 578417, 578415, 578413, 578411, 578409, 578407, 578405, 578403,
                                578401, 578399, 578397, 578395, 577918, 577916, 577914, 577912, 577910, 577908, 577906, 577904, 577884, 577882,
                                577880, 577878, 577876, 577874, 577872, 577870, 577868, 577866, 577820, 577818, 577816, 577398, 577396, 577394,
                                577392, 577390, 577165, 577163, 577161, 577159, 577157, 573238, 573236, 573234, 573228, 573226, 572411, 572409,
                                572138, 571966, 571964, 571930, 571482, 571480, 570651, 570649, 570647, 570645, 570643, 570421, 558485, 558477,
                                558467, 558459
                            };

            GwsDMSUtilities.DMSInfoExtractor dmsInfoExtractor = new PeptideInfoExtractor("pogo", "MT_Yellowstone_Communities_P627");

            string peakmatchingResultsFolder = @"F:\Yellowstone\PeakmatchingResults\FirstRun_withoutStac";

            string outputFolder = @"F:\Yellowstone\IQTargets";


            //jobList = jobList.Take(10).ToArray();

            foreach (var jobID in jobList)
            {
                umcUtilities.CopyUmcFile(peakmatchingResultsFolder, jobID, outputFolder);
            }


            //file://pogo/MTD_Peak_Matching/results/MT_Yellowstone_Communities_P627/LTQ_Orb/Job558459_auto_pm_123

        }

        [Category("Paper")]
        [Test]
        public void FilterForUnidentifiedFeaturesAndOutput()
        {
            var datasetNames = SipperDatasetUtilities.GetDatasetNames();

            string targetsFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\STAC_Filtered";

            string originalLCMSFeaturesFolder = @"F:\Yellowstone\IQTargets";

            string unidentifiedTargetsOutputFolder = @"F:\Yellowstone\IQTargets\UnidentifiedTargets";

            //datasetNames = datasetNames.Where(p => p.Contains("Yellow_C13_070")).ToList();


            StringBuilder sb = new StringBuilder();
            sb.Append("Datasetname\tNumIdentified\tNumUnidentified\n");

            foreach (var datasetName in datasetNames)
            {
                string stacBasedTargetFile = targetsFolder + "\\" + datasetName + "_targets.txt";

                if (File.Exists(stacBasedTargetFile))
                {

                    DeconTools.Backend.FileIO.LcmsTargetFromFeaturesFileImporter importer = new LcmsTargetFromFeaturesFileImporter(stacBasedTargetFile);
                    var targets = importer.Import().TargetList;

                    var stacIDs = targets.Select(p => p.ID).Distinct().ToList();

                    string lcmsFeaturesFile = originalLCMSFeaturesFolder + "\\" + datasetName + "_UMCs.txt";

                    if (File.Exists(lcmsFeaturesFile))
                    {

                        Console.WriteLine(datasetName + "\tStac file=\tfound\tUMC file=\tfound");


                        importer = new LcmsTargetFromFeaturesFileImporter(lcmsFeaturesFile);
                        var lcmsTargets = importer.Import().TargetList;

                        var filteredLcmsTargets = (from n in lcmsTargets where !stacIDs.Contains(n.ID) select n).ToList();

                        filteredLcmsTargets = (from n in filteredLcmsTargets
                                               group n by new
                                                              {
                                                                  n.ID
                                                              }
                                               into grp
                                               select grp.First()).ToList();


                        //filteredLcmsTargets = filteredLcmsTargets.Where(p => p.MonoIsotopicMass > 700).ToList();

                        int stacIDCount = stacIDs.Count;
                        int unidentifiedIDCount = filteredLcmsTargets.Count;


                        sb.Append(datasetName + "\t" + stacIDCount + "\t" + unidentifiedIDCount + "\n");



                        foreach (DeconTools.Backend.Core.LcmsFeatureTarget filteredLcmsTarget in filteredLcmsTargets)
                        {
                            filteredLcmsTarget.FeatureToMassTagID = 0;
                            filteredLcmsTarget.Code = "";

                        }


                        string unidentifiedTargetsFilename = unidentifiedTargetsOutputFolder + "\\" + datasetName + "_targets.txt";

                        LcmsTargetToTextExporter exporter = new LcmsTargetToTextExporter(unidentifiedTargetsFilename);
                        //exporter.ExportResults(filteredLcmsTargets);


                    }
                    else
                    {
                        Console.WriteLine(datasetName + "\tStac file \tfound\tUMC file \tNOT found");
                    }




                }
                else
                {
                    Console.WriteLine(datasetName + "\tStac file \tNOT found");
                }
            }

            Console.WriteLine();
            Console.WriteLine(sb.ToString());




        }



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

    

       

        [Category("Paper")]
        [Test]
        public void FilterForEnrichedFeatures_TightFilter()
        {
            string resultsFilename =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_UnidentifiedFeatures\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";

            string outputResultsFilename = resultsFilename.Replace("results.txt", "TightLabelFilter_results.txt");

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultsFilename);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();
            var nonRedundantOriginalResults = originalResults.GroupBy(x => x.TargetID).Select(g => g.First()).ToList();

            var unidentifiedResults = (from n in nonRedundantOriginalResults where n.MatchedMassTagID <= 0 select n).ToList();
            var identifiedResults = (from n in nonRedundantOriginalResults where n.MatchedMassTagID > 0 select n).ToList();

            unidentifiedResults = SipperFilters.ApplyAveragineBasedTightFilter(unidentifiedResults);
            identifiedResults = SipperFilters.ApplyAutoValidationCodeF1TightFilter(identifiedResults);

            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(outputResultsFilename);
            exporter.ExportResults(unidentifiedResults);

            Console.WriteLine("Total results= " + nonRedundantOriginalResults.Count);
            Console.WriteLine("Total unidentified results= " + unidentifiedResults.Count);
            Console.WriteLine("Total results with ID= " + identifiedResults.Count);
            Console.WriteLine("Labeled unidentified results (tight filter), count= " + unidentifiedResults.Count(p => p.ValidationCode == ValidationCode.Yes));
            Console.WriteLine("Labeled identified results (tight filter), count= " + identifiedResults.Count(p => p.ValidationCode == ValidationCode.Yes));


        }


        [Test]
        public void GetChromCorrValues_unidentifiedFeature()
        {

            string paramFile =
               @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\ExecutorParameters1.xml";

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
                @"D:\Data\Sipper\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";



            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            int testTarget = 13232;

            executor.Targets.TargetList = (from n in executor.Targets.TargetList where n.ID == testTarget select n).ToList();

            executor.Execute();



            //Figure03_FeatureElutionScripts.OutputChromData(testDataset, executor, testTarget);
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
