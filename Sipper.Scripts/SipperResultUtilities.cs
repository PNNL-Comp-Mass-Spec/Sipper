using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;

namespace Sipper.Scripts
{
    public class SipperResultUtilities
    {


        public static TargetedResultRepository LoadResultsForDataset(string dataset)
        {
            string resultFolder = @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results";

            resultFolder = @"D:\Data\Temp\Results";

            string targetResultFilename = resultFolder + "\\" + dataset + "_results.txt";


            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(targetResultFilename);
            var resultRepo = importer.Import();

            return resultRepo;

        }


        public static List<SipperLcmsFeatureTargetedResultDTO>ApplyFilteringScheme1(List<SipperLcmsFeatureTargetedResultDTO> allResults)
        {
            var filter1 = allResults.Where(p => p.AreaUnderRatioCurveRevised > 0).ToList();

            var filter2 =
                filter1.Where(p => p.ChromCorrelationAverage > 0.9 && p.ChromCorrelationMedian > 0.95).ToList();

            var filter3 = filter2.Where(p => p.IScore <= 0.15).ToList();

            var filter4 = filter3.Where(p => p.RSquaredValForRatioCurve >= 0.925).ToList();

            return filter4;
        }



        public static TargetedResultRepository LoadAllResults()
        {
            string resultFileName = @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\_mergedResults.txt";
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);
            var resultRepo = importer.Import();
            return resultRepo;
        }

        public static TargetedResultRepository LoadC12Results()
        {
            string resultFileName =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration03_Sum05\_Yellow_C12_MergedResults.txt";
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);
            var resultRepo = importer.Import();
            return resultRepo;
        }

        public static TargetedResultRepository LoadC13Results()
        {
            string resultFileName =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration03_Sum05\_Yellow_C13_MergedResults.txt";
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);
            var resultRepo = importer.Import();
            return resultRepo;
        }


        public static void CheckForMissingResults()
        {
            string resultFolder = @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results";

            CheckForMissingResults(resultFolder);

        }

        public static void CheckForMissingResults(string resultFolder)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(resultFolder);

            var resultFiles = dirInfo.GetFiles("*_results.txt").Select(p => p.Name.Replace("_results.txt", "")).ToList();




            var datasets = SipperDatasetUtilities.GetDatasetNames();

            StringBuilder sb = new StringBuilder();
            foreach (var dataset in datasets)
            {
                sb.Append(dataset);
                sb.Append("\t");

                if (resultFiles.Contains(dataset))
                {
                    sb.Append("OK");
                }
                else
                {
                    sb.Append("Missing");
                }

                sb.Append(Environment.NewLine);



            }

            Console.WriteLine(sb.ToString());

        }
    }
}
