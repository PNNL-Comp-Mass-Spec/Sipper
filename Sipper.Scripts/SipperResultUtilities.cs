using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.UnitTesting2;
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


        public static List<int>LoadMassTagIDs(string fileContainingMassTagIDs)
        {

            List<int> masstagIDs = new List<int>();

            using (StreamReader reader = new StreamReader(fileContainingMassTagIDs))
            {
                while (reader.Peek()!=-1)
                {
                    string line = reader.ReadLine();

                    int id = Convert.ToInt32(line);

                    masstagIDs.Add(id);

                }
            }

            return masstagIDs;

        }



        //public static void ApplyFilteringScheme1(List<SipperLcmsFeatureTargetedResultDTO> allResults)
        //{
        //    //var filter1 = allResults.Where(p => p.AreaUnderRatioCurveRevised > 0).ToList();

        //    //var filter2 =
        //    //    filter1.Where(p => p.ChromCorrelationAverage > 0.9 && p.ChromCorrelationMedian > 0.95).ToList();

        //    //var filter3 = filter2.Where(p => p.IScore <= 0.15).ToList();

        //    //var filter4 = filter3.Where(p => p.RSquaredValForRatioCurve >= 0.925).ToList();


        //    foreach (var sipperLcmsFeatureTargetedResultDto in allResults)
        //    {
                
        //        if (sipperLcmsFeatureTargetedResultDto.AreaUnderRatioCurveRevised>0)
        //        {
                    
        //            if (sipperLcmsFeatureTargetedResultDto.ChromCorrelationAverage>0.9 && sipperLcmsFeatureTargetedResultDto.ChromCorrelationMedian>0.95)
        //            {
                        
        //                if (sipperLcmsFeatureTargetedResultDto.RSquaredValForRatioCurve>0.925)
        //                {
                            
        //                    if (sipperLcmsFeatureTargetedResultDto.IScore<=0.15)
        //                    {
        //                        sipperLcmsFeatureTargetedResultDto.PassesFilter = true;
        //                    }

        //                }



        //            }




        //        }


        //    }

            
        //}



        public static TargetedResultRepository LoadAllResults()
        {
            string resultFileName =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\_mergedResults.txt";
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);
            var resultRepo = importer.Import();
            return resultRepo;
        }

        public static TargetedResultRepository LoadC12Results()
        {
            string resultFileName =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\Yellow_C12\_Yellow_C12_MergedResults.txt";
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);
            var resultRepo = importer.Import();
            return resultRepo;
        }

        public static TargetedResultRepository LoadC13Results()
        {
            string resultFileName =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\Yellow_C13\_Yellow_C13_MergedResults.txt";
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


        public static string ResultDetailsToString(SipperLcmsTargetedResult result, bool showTheorUnlabelledData = true, bool showObsIso=true, bool showChromData=true, bool showLabelDist=true)
        {
            StringBuilder sb = new StringBuilder();

            LcmsFeatureTarget target = (LcmsFeatureTarget) result.Target;
            char delim = '\n';

            sb.Append("FeatureID= \t" + result.Target.ID);
            sb.Append(delim);
            sb.Append("MassTagID= \t" +  target.FeatureToMassTagID);
            sb.Append(delim);
            sb.Append("Sequence= \t"+  target.Code);
            sb.Append(delim);

            sb.Append("NumQualityChromPeaks= \t" + result.NumQualityChromPeaks);
            sb.Append(delim);

            sb.Append("ChromCorrMedian= \t" + result.ChromCorrelationMedian);
            sb.Append(delim);

            sb.Append("ChromCorrAverage= \t" + result.ChromCorrelationAverage);
            sb.Append(delim);

            
            sb.Append("AreaRevised= \t" + result.AreaUnderRatioCurveRevised);
            sb.Append(delim);

            sb.Append("AreaDifferenceCurve= \t" + result.AreaUnderDifferenceCurve);
            sb.Append(delim);

            sb.Append("NumCarbonsLabelled= \t" + result.NumCarbonsLabelled);
            sb.Append(delim);

            sb.Append("PercentCarbonsLabelled= \t" + result.PercentCarbonsLabelled);
            sb.Append(delim);

            sb.Append("PercentPeptideLabelled= \t" + result.PercentPeptideLabelled);
            sb.Append(delim);

            sb.Append("RSquaredForRatioCurve= \t" + result.RSquaredValForRatioCurve);
            sb.Append(delim);

            if (showTheorUnlabelledData)
            {
                sb.Append(Environment.NewLine);
                sb.Append("------Theor isotopic profile data----\n");
                TestUtilities.IsotopicProfileDataToStringBuilder(sb, result.Target.IsotopicProfile);
            }

            if (showObsIso)
            {
                sb.Append(Environment.NewLine);
                sb.Append("------Observed isotopic profile data----\n");
                TestUtilities.IsotopicProfileDataToStringBuilder(sb, result.IsotopicProfile);
            }
            //sb.Append(Environment.NewLine);
            //sb.Append("------Subtracted isotopic profile data----\n");
            //TestUtilities.IsotopicProfileDataToStringBuilder(sb,);



            if (showChromData)
            {
                sb.Append(Environment.NewLine);
                sb.Append("------Chrom correlation data----\n");
                sb.Append(ResultChromCorrValuesToString(result));
            }


            if (showLabelDist)
            {
                sb.Append(Environment.NewLine);
                sb.Append("------Label Distribution data----\n");
                sb.Append(ResultLabelDistributionValuesToString(result));
            }

            return sb.ToString();



        }

        public static string ResultLabelDistributionValuesToString(SipperLcmsTargetedResult result)
        {
            int peakCounter = 0;
            StringBuilder sb = new StringBuilder();

            foreach (var item in result.LabelDistributionVals)
            {
                sb.Append(peakCounter + "\t" + item + Environment.NewLine);
                peakCounter++;
            }

            return sb.ToString();
        }


        public static string ResultChromCorrValuesToString(SipperLcmsTargetedResult result)
        {

            int peakCounter = 0;
            StringBuilder sb = new StringBuilder();

            foreach (var item in result.ChromCorrelationData.CorrelationDataItems)
            {
                sb.Append(peakCounter + "\t" + item.CorrelationRSquaredVal + Environment.NewLine);
                peakCounter++;
            }

            return sb.ToString();

        }

    }
}
