using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace Sipper.Scripts
{
    [TestFixture]
    public class ResultsFilteringScripts
    {
        [Test]
        public void showC12ResultStats()
        {
            var allResults = SipperResultUtilities.LoadC12Results().Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();

            OutputFilteringStats(allResults);
        }


        [Test]
        public void showC13ResultStats()
        {
            var allResults = SipperResultUtilities.LoadC13Results().Results.Select(p=>(SipperLcmsFeatureTargetedResultDTO)p).ToList();

            OutputFilteringStats(allResults);
        }



        [Test]
        public void ExportFilteredResults()
        {
            var allResults = SipperResultUtilities.LoadC13Results().Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();

            var filteredResults = SipperResultUtilities.ApplyFilteringScheme1(allResults);

            string exportFileName =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration03_Sum05\Yellow_C13_Enriched_results.txt";

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
