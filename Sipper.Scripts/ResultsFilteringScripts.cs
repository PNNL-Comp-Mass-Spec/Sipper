using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using GwsDMSUtilities;
using NUnit.Framework;
using Sipper.Model;

namespace Sipper.Scripts
{
    [TestFixture]
    public class ResultsFilteringScripts
    {
        [Test]
        public void showC12ResultStats()
        {
            var allResults = SipperResultUtilities.LoadC12Results().Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();

            OutputFilteringStats2(allResults);
        }


        [Test]
        public void showC13ResultStats()
        {
            var allResults = SipperResultUtilities.LoadC13Results().Results.Select(p=>(SipperLcmsFeatureTargetedResultDTO)p).ToList();

            OutputFilteringStats2(allResults);
        }


        [Test]
        public void showC12andC13ResultStats()
        {
            var allResults = SipperResultUtilities.LoadC12Results().Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();

            OutputFilteringStats2(allResults);

            allResults = SipperResultUtilities.LoadC13Results().Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();

            OutputFilteringStats2(allResults);
        }


        [Test]
        public void filterOutRedundantResults()
        {
            string resultFileName =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);

           var repo=  importer.Import();

            var nonRedundant = repo.Results.GroupBy(x => x.TargetID).Select(g => g.First()).ToList();

            string exportFileName = resultFileName.Replace("_results.txt", "_nonRedundant_results.txt");

            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportFileName);
            exporter.ExportResults(nonRedundant);



        }


        [Test]
        public void filterResults()
        {
            string resultFileName =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant_results.txt";

            //string resultFileName =
            //    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_070_23Mar10_Griffin_10-01-28_nonRedundant\Yellow_C13_070_23Mar10_Griffin_10-01-28_NR_NoFormula_results.txt";
            
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);

            var repo = importer.Import();

            var sipperResults = repo.Results.Select(p => (SipperLcmsFeatureTargetedResultDTO) p).ToList();
            ResultFilteringUtilities.ApplyFilteringScheme2(sipperResults);

            var filteredResults = sipperResults.Where(p => p.PassesFilter).ToList();

            Console.WriteLine("total results = " + sipperResults.Count);
            Console.WriteLine("Num filtered results = " + filteredResults.Count);



            string exportFileName = resultFileName.Replace("_results.txt", "_enriched_results.txt");
            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportFileName);
            exporter.ExportResults(filteredResults);



        }



        [Test]
        public void ExportFilteredResults()
        {
            var allResults = SipperResultUtilities.LoadC13Results().Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();

            ResultFilteringUtilities.ApplyFilteringScheme1(allResults);

            var filteredResults = allResults.Where(p => p.PassesFilter).ToList();


            string exportFileName =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\_Yellow_C13_Enriched_results.txt";

            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportFileName);
            exporter.ExportResults(filteredResults);


        }


        [Test]
        public void DisplayStatsOnFilteredResults1()
        {
            string resultFileName = @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration03_Sum05\Yellow_C13_Enriched_results.txt";

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);
            var resultRepo = importer.Import();


            


            var uniqueUMCs = (from n in resultRepo.Results
                              group n by new
                                             {
                                                 n.TargetID
                                             }
                              into grp
                              select grp.First()).ToList();

            Console.WriteLine("Total C13-Enriched UMCs matching MassTags = \t" + resultRepo.Results.Count);
            Console.WriteLine("Unique C13-Enriched UMCs matching MassTags = \t" + uniqueUMCs.Count);


            var datasetNames = SipperDatasetUtilities.GetDatasetNames().Where(p=>p.Contains("Yellow_C13")).ToList();

            foreach (var datasetName in datasetNames)
            {
                int fractionNum = SipperDatasetUtilities.GetFractionNum(datasetName);


                int count = uniqueUMCs.Count(p => p.DatasetName == datasetName);
                Console.WriteLine(datasetName + "\t" + fractionNum + "\t" +  count);
            }





        }



        [Test]
        public void DisplayStatsOnFilteredResults2()
        {
            string resultFileName = @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration03_Sum05\Yellow_C13_Enriched_results.txt";

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);
            var resultRepo = importer.Import();





            var uniqueUMCs = (from n in resultRepo.Results
                              group n by new
                              {
                                  n.MatchedMassTagID
                              }
                                  into grp
                                  select grp.First()).ToList();

            Console.WriteLine("Total C13-Enriched UMCs matching MassTags = \t" + resultRepo.Results.Count);
            Console.WriteLine("Unique C13-Enriched UMCs matching MassTags = \t" + uniqueUMCs.Count);


            var matchedMassTagIDs = uniqueUMCs.Select(p => p.MatchedMassTagID).OrderBy(p => p).ToList();

            foreach (var m in matchedMassTagIDs)
            {
                Console.WriteLine(m + ",");
            }

            //var datasetNames = SipperDatasetUtilities.GetDatasetNames().Where(p => p.Contains("Yellow_C13")).ToList();

            //foreach (var datasetName in datasetNames)
            //{
            //    int fractionNum = SipperDatasetUtilities.GetFractionNum(datasetName);


            //    int count = uniqueUMCs.Count(p => p.DatasetName == datasetName);
            //    Console.WriteLine(datasetName + "\t" + fractionNum + "\t" + count);
            //}





        }

        

        [Test]
        public void FilterAndExportResultsForSelectedMassTags()
        {

            string massTagIDFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\300SamplePeptides_2012_01_23_IDsOnly.txt";

            massTagIDFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Targets\refID22508_massTags_IDsOnly.txt";
            
            var allResults = SipperResultUtilities.LoadC13Results().Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();

            var massTagIDs=   SipperResultUtilities.LoadMassTagIDs(massTagIDFile);


            int refID1 = 38803;
            //int refID2 = 39890;

            string dbname = "MT_Yellowstone_Communities_P627";
            string serverName = "pogo";
            var infoExtractor = new PeptideInfoExtractor(serverName, dbname);


           

            var peptideList = infoExtractor.GetMassTagIDsForGivenProtein(refID1);
            massTagIDs = peptideList.Select(p => p.ID).ToList();



            var filteredResults =  (from n in allResults where massTagIDs.Contains(n.MatchedMassTagID) select n).ToList();

            //a mass tag is often found in multiple datasets. Will choose the most abundant one

            //filteredResults = (from n in filteredResults
            //                   group n by new {n.MatchedMassTagID}
            //                   into grp
            //                   select grp.OrderByDescending(p => p.Intensity).First()).ToList();


            foreach (var sipperLcmsFeatureTargetedResultDto in filteredResults)
            {
                
            }



            string exportFileName =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_04_27_ASMS_Data\Yellow_C13_withSelected300MassTags_results.txt";

            exportFileName =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Targets\exportedMassTags.txt";



            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportFileName);
            exporter.ExportResults(filteredResults);

        }



        private void OutputFilteringStats2(List<SipperLcmsFeatureTargetedResultDTO>allResults)
        {
            ResultFilteringUtilities.ApplyFilteringScheme1(allResults);

            Console.WriteLine("All results = \t" + allResults.Count);
            Console.WriteLine("C13Enriched = \t" + allResults.Count(p => p.PassesFilter));

            ResultFilteringUtilities.ApplyFilteringScheme2(allResults);
            Console.WriteLine("All results = \t" + allResults.Count);
            Console.WriteLine("C13Enriched = \t" + allResults.Count(p => p.PassesFilter));

        }



        private List<SipperLcmsFeatureTargetedResultDTO> OutputFilteringStats(List<SipperLcmsFeatureTargetedResultDTO> allResults)
        {
            
            var filter1 = allResults.Where(p => p.AreaUnderRatioCurveRevised > 0).ToList();
      
            var filter2 =
                filter1.Where(p => p.ChromCorrelationAverage > 0.9 && p.ChromCorrelationMedian > 0.95).ToList();

            var filter3 = filter2.Where(p => p.IScore <= 0.15).ToList();
            
            var filter4 = filter3.Where(p => p.RSquaredValForRatioCurve >= 0.925).ToList();

            //List<SipperLcmsFeatureTargetedResultDTO> resultsGradedIScore =
            //    new List<SipperLcmsFeatureTargetedResultDTO>();


            //foreach (var r in filter3)
            //{
            //    if (r.Intensity <=25000 && r.IScore < 0.05)
            //    {
            //        resultsGradedIScore.Add(r);
            //    }
            //    else if (r.Intensity > 25000 && r.Intensity < 50000 && r.IScore < 0.08)
            //    {
            //        resultsGradedIScore.Add(r);
            //    }
            //    else if (r.Intensity > 50000 && r.Intensity < 100000 && r.IScore < 0.1)
            //    {
            //        resultsGradedIScore.Add(r);
            //    }
            //    else if
            //        (r.Intensity > 1e5 && r.Intensity < 5e5 && r.IScore < 0.10)
            //    {
            //        resultsGradedIScore.Add(r);
            //    }
            //    else if
            //        (r.Intensity > 5e5 && r.Intensity < 1e6 && r.IScore < 0.15)
            //    {
            //        resultsGradedIScore.Add(r);
            //    }
            //    else if
            //        (r.Intensity > 1e6 && r.IScore < 0.15)
            //    {
            //        resultsGradedIScore.Add(r);
            //    }
                
            //}


            Console.WriteLine("All results = \t" + allResults.Count);
            //Console.WriteLine("Results above 25000 = \t" + resultsWithOKIntensity.Count);
            Console.WriteLine("Results with area= \t" + filter1.Count);
            Console.WriteLine("Results with good correlation= \t" + filter2.Count);
            Console.WriteLine("Results with good i_score= \t" + filter3.Count);
            //Console.WriteLine("Results after graded iscore= \t" + resultsGradedIScore.Count);

            Console.WriteLine();
            Console.WriteLine("rsquaredVal\tCount");
            for (double d = 0.85; d <= 1; d += 0.025)
            {
                int count = filter3.Count(p => p.RSquaredValForRatioCurve>=d);
                Console.WriteLine(d + "\t" + count);
            }

            return filter4;
        }
    }
}
