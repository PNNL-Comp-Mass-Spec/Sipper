using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;
using Sipper.Model;

namespace Sipper.Scripts
{
    [TestFixture]
    public class OMCDA_0284_CheckForLabeling
    {

        //see https://jira.pnnl.gov/jira/browse/OMCDA-284


        [Test]
        public void FilterOutRedundantResultsFromResultFiles()
        {
            string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Results\Original";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Results\Non-redundant";

            int featureIDColIndex = 1;


            var dirInfo = new DirectoryInfo(sourceFolder);


            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);


            var fileInfos = dirInfo.GetFiles("*_results.txt");

            foreach (var fileInfo in fileInfos)
            {

                string outputFileName = outputFolder + Path.DirectorySeparatorChar + fileInfo.Name.Replace("_results.txt", "_NR_results.txt");

                if (File.Exists(outputFileName)) File.Delete(outputFileName);

                using (StreamWriter writer = new StreamWriter(outputFileName))
                {
                    List<int> featureIDs = new List<int>();

                    int counter = 0;
                    using (StreamReader reader = new StreamReader(fileInfo.FullName))
                    {
                        writer.WriteLine(reader.ReadLine());

                        
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

                        reader.Close();
                    }

                    Console.WriteLine(fileInfo.Name + "\t" + counter);

                    writer.Close();

                }
            }
        }


        [Test]
        public void ApplyFilterF1_on_eachResultFile()
        {
            string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Results\Non-redundant";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Results\Filter_F1";

            int featureIDColIndex = 1;


            var dirInfo = new DirectoryInfo(sourceFolder);
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            var fileInfos = dirInfo.GetFiles("*_results.txt");

            foreach (var fileInfo in fileInfos)
            {

                string outputFileName = outputFolder + Path.DirectorySeparatorChar + fileInfo.Name.Replace("_results.txt", "_F1_results.txt");

                if (File.Exists(outputFileName)) File.Delete(outputFileName);

                SipperResultFromTextImporter importer = new SipperResultFromTextImporter(fileInfo.FullName);
                var repo = importer.Import();

                foreach (var sipperLcmsFeatureTargetedResultDto in repo.Results)
                {
                    ApplyFilterF1(sipperLcmsFeatureTargetedResultDto as SipperLcmsFeatureTargetedResultDTO);

                }

                var exportedResults =
                    repo.Results.Where(p => ((SipperLcmsFeatureTargetedResultDTO)p).PassesFilter);

                Console.WriteLine(fileInfo.Name + "\t" + exportedResults.Count());

                SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(outputFileName);
                exporter.ExportResults(exportedResults);

            }
        }


        [Test]
        public void ApplyFilterF2_on_eachResultFile()
        {
            string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Results\Non-redundant";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Results\Filter_F2";

            int featureIDColIndex = 1;


            var dirInfo = new DirectoryInfo(sourceFolder);
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            var fileInfos = dirInfo.GetFiles("*_results.txt");

            foreach (var fileInfo in fileInfos)
            {

                string outputFileName = outputFolder + Path.DirectorySeparatorChar + fileInfo.Name.Replace("_results.txt", "_F2_results.txt");

                if (File.Exists(outputFileName)) File.Delete(outputFileName);

                SipperResultFromTextImporter importer = new SipperResultFromTextImporter(fileInfo.FullName);
                var repo = importer.Import();

                foreach (var sipperLcmsFeatureTargetedResultDto in repo.Results)
                {
                    ApplyFilterF2(sipperLcmsFeatureTargetedResultDto as SipperLcmsFeatureTargetedResultDTO);

                }

                var exportedResults =
                    repo.Results.Where(p => ((SipperLcmsFeatureTargetedResultDTO)p).PassesFilter);

                Console.WriteLine(fileInfo.Name + "\t" + exportedResults.Count());

                SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(outputFileName);
                exporter.ExportResults(exportedResults);

            }
        }


        [Test]
        public void OutputImageResults_TestDataset()
        {
            FileInputsInfo fileInputs = new FileInputsInfo();

            fileInputs.DatasetDirectory = @"F:\Yellowstone\RawData";
            fileInputs.ResultsSaveFilePath = @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Results\Visuals\YSIP_SMART_12-9_19";
            fileInputs.TargetsFilePath =
                @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Results\Filter_F2\YSIP_SMART_12-9_19_2Mar12_Earth_12-02-06_NR_F2_results.txt";

            fileInputs.ParameterFilePath =
                @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Parameters\SipperTargetedWorkflowParameters_Sum5.xml";


            ResultImageOutputter imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }


        [Test]
        public void OutputImageResults_ReferenceDataset()
        {
            FileInputsInfo fileInputs = new FileInputsInfo();

            fileInputs.DatasetDirectory = @"F:\Yellowstone\RawData";
            fileInputs.ResultsSaveFilePath = @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Results\Visuals\Yellow_C13_070";
            fileInputs.TargetsFilePath =
                @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Results\Filter_F2\Yellow_C13_070_23Mar10_Griffin_10-01-28_NR_F2_results.txt";

            fileInputs.ParameterFilePath =
                @"\\protoapps\DataPkgs\Public\2012\632_Sipper_C13_Analysis_Verification_of_Labeling\Parameters\SipperTargetedWorkflowParameters_Sum5.xml";


            ResultImageOutputter imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }




        private void ApplyFilterF1(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.ChromCorrelationMedian > 0.98 && result.ChromCorrelationAverage > 0.95)
            {
                if (result.NumHighQualityProfilePeaks > 2)
                {
                    result.PassesFilter = true;
                }

            }
        }


        private void ApplyFilterF1b(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.ChromCorrelationMedian > 0.98 && result.ChromCorrelationAverage > 0.95)
            {
                if (result.NumHighQualityProfilePeaks > 2)
                {
                    result.PassesFilter = true;
                }

            }
        }



    


        private void ApplyFilterF2(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.ChromCorrelationMedian > 0.95 && result.ChromCorrelationAverage > 0.90)
            {
                if (result.NumHighQualityProfilePeaks > 4)
                {
                    result.PassesFilter = true;
                }

            }
        }






    }
}
