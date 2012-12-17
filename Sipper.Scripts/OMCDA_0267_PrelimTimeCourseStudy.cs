using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace Sipper.Scripts
{
    [TestFixture]
    public class OMCDA_0267_PrelimTimeCourseStudy
    {

        //see:   https://jira.pnnl.gov/jira/browse/OMCDA-267


        [Test]
        public void FilterLCMSFeatures()
        {
            string sourceFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_06_13_LCMSFeatures_From_YSIP_data";

            sourceFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets";

            sourceFolder =
                @"\\protoapps\DataPkgs\Public\2012\622_Sipper_C13_analysis_of_initial_time_course_study\Targets\Original_UMCs";


            int massTagIDCol = 26;

            //massTagIDCol = 25;


            bool filterOutMultipleFeatureIDs = false;
            bool filterOutFeaturesWithoutIDs = false;


            var dirInfo = new DirectoryInfo(sourceFolder);

            var fileInfos = dirInfo.GetFiles("*_LCMSFeatures.txt");

            foreach (var fileInfo in fileInfos)
            {
                string outputFileName = fileInfo.FullName.Replace("_LCMSFeatures.txt", "_targets.txt");

                using (StreamWriter writer = new StreamWriter(outputFileName))
                {
                    List<int> featureIDs = new List<int>();

                    using (StreamReader reader = new StreamReader(fileInfo.FullName))
                    {
                        writer.WriteLine(reader.ReadLine());



                        while (reader.Peek() != -1)
                        {
                            string line = reader.ReadLine();

                            var parsedLine = line.Split('\t');

                            int massTagID = Convert.ToInt32(parsedLine[massTagIDCol]);

                            int featureID = Convert.ToInt32(parsedLine[0]);


                            if (filterOutFeaturesWithoutIDs &&   massTagID > 0)
                            {
                                if (filterOutMultipleFeatureIDs && !featureIDs.Contains(featureID))
                                {
                                    writer.WriteLine(line);
                                    featureIDs.Add(featureID);
                                }
                                else
                                {
                                    writer.WriteLine(line);
                                }
                            }
                            else
                            {
                                writer.WriteLine(line);
                            }
                        }

                        reader.Close();
                    }

                    writer.Close();

                }
            }


        }




        [Test]
        public void ExecuteSipper_timeCourse()
        {
            string paramFile =
                @"\\protoapps\DataPkgs\Public\2012\622_Sipper_C13_analysis_of_initial_time_course_study\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.CopyRawFileLocal = false;


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_SIP_13-11_HL_14Aug12_Falcon_12-06-02.RAW";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            executor.Execute();


        }

        [Test]
        public void FilterOutRedundantResultsFromResultFiles()
        {
            string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\622_Sipper_C13_analysis_of_initial_time_course_study\Results\Original";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\622_Sipper_C13_analysis_of_initial_time_course_study\Results\Non-redundant";

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
                                writer.WriteLine(line);
                                featureIDs.Add(featureID);
                            }

                        }

                        reader.Close();
                    }

                    writer.Close();

                }
            }
        }


        [Test]
        public void ApplyFilterF1_on_eachResultFile()
        {
            string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\622_Sipper_C13_analysis_of_initial_time_course_study\Results\Non-redundant";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\622_Sipper_C13_analysis_of_initial_time_course_study\Results\Filter_F1";

            int featureIDColIndex = 1;


            var dirInfo = new DirectoryInfo(sourceFolder);
            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            var fileInfos = dirInfo.GetFiles("*_results.txt");

            foreach (var fileInfo in fileInfos)
            {

                string outputFileName = outputFolder + Path.DirectorySeparatorChar + fileInfo.Name.Replace("_results.txt", "_F1b_results.txt");

                if (File.Exists(outputFileName)) File.Delete(outputFileName);

                SipperResultFromTextImporter importer = new SipperResultFromTextImporter(fileInfo.FullName);
                var repo = importer.Import();

                foreach (var sipperLcmsFeatureTargetedResultDto in repo.Results)
                {
                    ApplyFilterF1b(sipperLcmsFeatureTargetedResultDto as SipperLcmsFeatureTargetedResultDTO);

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
            string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\622_Sipper_C13_analysis_of_initial_time_course_study\Results\Non-redundant";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\622_Sipper_C13_analysis_of_initial_time_course_study\Results\Filter_F2";

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
            if (result.ChromCorrelationMedian > 0.95 && result.ChromCorrelationAverage > 0.90)
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
