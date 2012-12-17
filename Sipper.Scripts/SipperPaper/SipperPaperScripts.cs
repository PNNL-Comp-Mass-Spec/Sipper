using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using DeconTools.Workflows.Backend.Utilities;
using GwsDMSUtilities;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class SipperPaperScripts
    {

        [Test]
        public void GetPeptideInfoAndExportMassTagReferenceFile()
        {
            string fileContainingMassTagIDs =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\C12_C13_allMassTags_IDsOnly.txt";


            string outputFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\TargetReferenceData.txt";

            List<int> massTagIDs = LoadMassTagIDs(fileContainingMassTagIDs).Distinct().OrderBy(p => p).ToList();

            string dbname = "MT_Yellowstone_Communities_P627";
            string serverName = "pogo";

            GwsDMSUtilities.PeptideInfoExtractor peptideInfoExtractor = new PeptideInfoExtractor(serverName, dbname);
            var peptideInfo = peptideInfoExtractor.GetPeptideInfo(massTagIDs, true);

            DeconTools.Backend.FileIO.MassTagTextFileExporter massTagTextFileExporter =
                new MassTagTextFileExporter(outputFile);

            peptideInfo = peptideInfo.OrderBy(p => p.ID).ToList();

            massTagTextFileExporter.ExportResults(peptideInfo);


        }


        [Test]
        public void ExecuteSipper()
        {
            string paramFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";


            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            executor.Execute();


        }

        [Test]
        public void CheckDatasets()
        {
            var datasetutil = new DatasetUtilities();

            string fileContainingDatasetNames =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\DatasetsToBeAnalyzed\datasetNames_masterList.txt";

            var nameList = FileUtilities.LoadStringsFromFile(fileContainingDatasetNames);


            foreach (var item in nameList)
            {
                bool isArchived = datasetutil.GetDatasetPath(item).ToLower().Contains("purged");

                Console.WriteLine(item + "\t" + isArchived);


            }


        }


        [Test]
        public void CopyProblemDatasetsToProtoapps()
        {
            var datasetutil = new DatasetUtilities();

            string fileContainingDatasetNames =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\DatasetsToBeAnalyzed\customList.txt";

            var nameList = FileUtilities.LoadStringsFromFile(fileContainingDatasetNames);

            string baseFolder = @"F:\Yellowstone\RawData";

            string destinationFolder = @"\\protoapps\UserData\Slysz\Data\Yellowstone\RawData";

            foreach (var item in nameList)
            {
                string sourceFile = baseFolder + "\\" + item + ".raw";

                string destinationFile = destinationFolder + "\\" + item + ".raw";

                FileInfo fileInfo = new FileInfo(sourceFile);
                fileInfo.CopyTo(destinationFile);
                Console.WriteLine(destinationFile + "\t.............copied");
            }



        }


      


    


        [Test]
        public void ExecuteSipper_timeCourse()
        {
            string paramFile =
                @"\\protoapps\DataPkgs\Public\2012\622_Sipper_C13_analysis_of_initial_time_course_study\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_SIP_13-4_HL_14Aug12_Falcon_12-06-02.RAW";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            executor.Execute();


        }




        [Test]
        public void ExecuteSipperOnUnlabelledFile()
        {
            string paramFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C12_070_21Feb10_Griffin_09-11-40.raw";


            string outputParameterFile = Path.GetDirectoryName(paramFile) + Path.DirectorySeparatorChar +
                                         Path.GetFileNameWithoutExtension(paramFile) + " - copy.xml";
            parameters.SaveParametersToXML(outputParameterFile);


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);
            executor.Execute();


        }






        [Test]
        public void FilterOutRedundantResultsFromResultFiles()
        {
            string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Original";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Non-redundant";

            int featureIDColIndex = 1;


            var dirInfo = new DirectoryInfo(sourceFolder);

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






        [Test]
        public void ApplyFilterF1_on_eachResultFile()
        {
            string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Non-redundant";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Filter_F1d_highLabeling";

            int featureIDColIndex = 1;




            var dirInfo = new DirectoryInfo(sourceFolder);

            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);


            var fileInfos = dirInfo.GetFiles("*_results.txt");

            foreach (var fileInfo in fileInfos)
            {

                string outputFileName = outputFolder + Path.DirectorySeparatorChar + fileInfo.Name.Replace("_results.txt", "_F1d_results.txt");

                if (File.Exists(outputFileName)) File.Delete(outputFileName);

                SipperResultFromTextImporter importer = new SipperResultFromTextImporter(fileInfo.FullName);
                var repo = importer.Import();

                foreach (var sipperLcmsFeatureTargetedResultDto in repo.Results)
                {
                    ApplyFilterF1d(sipperLcmsFeatureTargetedResultDto as SipperLcmsFeatureTargetedResultDTO);

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
            string sourceFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Filter_F1";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Filter_F2";

            int featureIDColIndex = 1;


            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);


            var dirInfo = new DirectoryInfo(sourceFolder);

            var fileInfos = dirInfo.GetFiles("*_results.txt");

            foreach (var fileInfo in fileInfos)
            {

                string outputFileName = outputFolder + Path.DirectorySeparatorChar + fileInfo.Name.Replace("_F1_results.txt", "_F2_results.txt");

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
        public void ExamineSomeTargetsAndValidateQuantification()
        {

            string paramFile =
               @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.DbName = "";
            parameters.DbServer = "";
            parameters.TargetsFilePath =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Filter_F1\Yellow_C13_070_23Mar10_Griffin_10-01-28_NR_F1_results.txt";

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = true;


            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);



            int testTarget = 8517;
            testTarget = 11367;


            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTarget).ToList();

            executor.Execute();


            SipperTargetedWorkflow workflow = (SipperTargetedWorkflow)executor.TargetedWorkflow;

            //TestUtilities.DisplayIsotopicProfileData(workflow.NormalizedIso);

            Console.WriteLine();

            //TestUtilities.DisplayIsotopicProfileData(workflow.NormalizedAdjustedIso);

            var result = workflow.Result as SipperLcmsTargetedResult;

            Console.WriteLine();
            Console.WriteLine("Percent incorp = " + result.PercentCarbonsLabelled);
            //TestUtilities.DisplayIsotopicProfileData(workflow.SubtractedIso);


        }


        [Test]
        public void ExamineSomeTargetsAndValidateQuantification2()
        {

            string paramFile =
               @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.DbName = "";
            parameters.DbServer = "";
            parameters.TargetsFilePath =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Filter_F2\Yellow_C13_070_23Mar10_Griffin_10-01-28_NR_F2_results.txt";

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = true;


            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);



            int testTarget = 8517;

            executor.InitializeRun(testDataset);
            executor.TargetedWorkflow.Run = executor.Run;
            //executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTarget).ToList();

            // executor.Execute();


            foreach (var targetBase in executor.Targets.TargetList)
            {
                executor.Run.CurrentMassTag = targetBase;
                var workflow = (SipperTargetedWorkflow)executor.TargetedWorkflow;

                workflow.Execute();
                var result = workflow.Result as SipperLcmsTargetedResult;

                StringBuilder sb = new StringBuilder();



                sb.Append(result.Target.ID + "-------------------------------------------------------------\n");
                sb.Append("%Inc= \t" + result.PercentCarbonsLabelled.ToString("0.000") + "\tPercentPeptideLabelled= \t" + result.PercentPeptideLabelled.ToString("0.000"));
                sb.Append(Environment.NewLine);

                if (result.LabelDistributionVals != null)
                {
                    for (int i = 0; i < result.LabelDistributionVals.Count; i++)
                    {
                        sb.Append(i + "\t" + result.LabelDistributionVals[i]);
                        sb.Append(Environment.NewLine);
                    }


                }
                Console.WriteLine(sb.ToString());
            }









        }


        [Test]
        public void ExamineSomeTargetsAndValidateQuantification3()
        {

            string paramFile =
               @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.DbName = "";
            parameters.DbServer = "";
            parameters.TargetsFilePath =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Filter_F2\Yellow_C13_070_23Mar10_Griffin_10-01-28_NR_F2_results.txt";

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = true;


            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);



            int testTarget = 8517;

            executor.InitializeRun(testDataset);
            executor.TargetedWorkflow.Run = executor.Run;
            //executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTarget).ToList();

            // executor.Execute();


            foreach (var targetBase in executor.Targets.TargetList)
            {
                executor.Run.CurrentMassTag = targetBase;
                var workflow = (SipperTargetedWorkflow)executor.TargetedWorkflow;

                workflow.Execute();
                var result = workflow.Result as SipperLcmsTargetedResult;

                StringBuilder sb = new StringBuilder();

                string delim = "\t";
                sb.Append(result.Target.ID);
                sb.Append(delim);
                sb.Append(result.Target.Code);
                sb.Append(delim);
                sb.Append(result.PercentCarbonsLabelled.ToString("0.0000"));
                sb.Append(delim);
                sb.Append(result.PercentPeptideLabelled.ToString("0.0000"));
                sb.Append(delim);

                if (result.LabelDistributionVals != null)
                {
                    for (int i = 0; i < result.LabelDistributionVals.Count; i++)
                    {
                        sb.Append(result.LabelDistributionVals[i].ToString("0.000000"));
                        sb.Append(delim);
                    }
                }

                Console.WriteLine(sb.ToString());
            }









        }



        [Test]
        public void GetAllHighQualityResults_andShowLabelingDistrib()
        {

            List<string> datasetNames = LoadDatasetNames();
            string paramFile =
              @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";
            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.DbName = "";
            parameters.DbServer = "";
            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = false;


            string baseResultsFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Filter_F2";

            string baseRawDataFolder = @"F:\Yellowstone\RawData";

            string expectedResultsSuffix = "_NR_F2_results.txt";


            string outputFile = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Analysis\LabelDistributionsHighQualityData.txt";


            datasetNames.RemoveRange(0, 46);

            datasetNames = datasetNames.Take(1).ToList();



            using (StreamWriter writer = new StreamWriter(outputFile))
            {
                foreach (var datasetName in datasetNames)
                {

                    string expectedResultFile = baseResultsFolder + Path.DirectorySeparatorChar + datasetName + expectedResultsSuffix;

                    if (!File.Exists(expectedResultFile)) continue;

                    parameters.TargetsFilePath = expectedResultFile;

                    string testDataset = baseRawDataFolder + Path.DirectorySeparatorChar + datasetName + ".raw";

                    SipperWorkflowExecutor executor;
                    try
                    {
                        executor = new SipperWorkflowExecutor(parameters, testDataset);
                    }
                    catch (Exception)
                    {

                        continue;

                    }



                    executor.InitializeRun(testDataset);
                    executor.TargetedWorkflow.Run = executor.Run;


                    StringBuilder sb = new StringBuilder();
                    foreach (var targetBase in executor.Targets.TargetList)
                    {
                        executor.Run.CurrentMassTag = targetBase;
                        var workflow = (SipperTargetedWorkflow)executor.TargetedWorkflow;

                        workflow.Execute();
                        var result = workflow.Result as SipperLcmsTargetedResult;

                        string delim = "\t";
                        sb.Append(result.Run.DatasetName);
                        sb.Append(delim);
                        sb.Append(result.Target.ID);
                        sb.Append(delim);
                        sb.Append(((DeconTools.Backend.Core.LcmsFeatureTarget)targetBase).FeatureToMassTagID);
                        sb.Append(delim);
                        sb.Append(result.Target.Code);
                        sb.Append(delim);
                        sb.Append(result.PercentCarbonsLabelled.ToString("0.0000"));
                        sb.Append(delim);
                        sb.Append(result.PercentPeptideLabelled.ToString("0.0000"));
                        sb.Append(delim);

                        if (result.LabelDistributionVals != null)
                        {
                            for (int i = 0; i < result.LabelDistributionVals.Count; i++)
                            {
                                sb.Append(result.LabelDistributionVals[i].ToString("0.000000"));
                                sb.Append(delim);
                            }
                        }

                        sb.Append(Environment.NewLine);
                    }

                    Console.WriteLine(sb.ToString());
                    writer.Write(sb.ToString());

                }
                writer.Close();
            }









        }

        private List<string> LoadDatasetNames()
        {
            List<string> datasetnames = new List<string>();

            string fileContainingDatasetNames =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\DatasetsToBeAnalyzed\datasetNames_C13Only.txt";

            using (StreamReader reader = new StreamReader(fileContainingDatasetNames))
            {
                while (reader.Peek() != -1)
                {
                    datasetnames.Add(reader.ReadLine());
                }
                reader.Close();

            }

            return datasetnames;


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


        private void ApplyFilterF1c(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.ChromCorrelationMedian > 0.90 && result.ChromCorrelationAverage > 0.85)
            {
                if (result.NumHighQualityProfilePeaks > 2)
                {
                    result.PassesFilter = true;
                }

            }
        }

        private void ApplyFilterF1d(SipperLcmsFeatureTargetedResultDTO result)
        {


            if (result.NumCarbonsLabelled >= 2 && result.PercentPeptideLabelled > 5)
            {
                result.PassesFilter = true;
            }
        }

        private void ApplyFilterF2(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.ChromCorrelationAverage > 0.90 && result.ChromCorrelationMedian > 0.95)
            {
                if (result.Intensity > 1e5)
                {
                    result.PassesFilter = true;
                }


            }
        }

        private List<int> LoadMassTagIDs(string fileContainingMassTagIDs)
        {

            List<int> massTagIDs = new List<int>();

            using (StreamReader reader = new StreamReader(fileContainingMassTagIDs))
            {
                while (reader.Peek() != -1)
                {
                    int mtid = Convert.ToInt32(reader.ReadLine());
                    massTagIDs.Add(mtid);
                }

                reader.Close();
            }

            return massTagIDs;
        }


        [Test]
        public void playWithNullables()
        {
            var x = new int?();
            Display(x);
        }


        static void Display(Nullable<int> x)
        {
            Console.WriteLine("HasValue: {0}", x.HasValue);
            if (x.HasValue)
            {
                Console.WriteLine("Value: {0}", x.Value);
                Console.WriteLine("Explicit conversion: {0}", (int)x);
            }
            Console.WriteLine("GetValueOrDefault(): {0}",
            x.GetValueOrDefault());
            Console.WriteLine("GetValueOrDefault(10): {0}",
            x.GetValueOrDefault(10));
            Console.WriteLine("ToString(): \"{0}\"", x.ToString());
            Console.WriteLine("GetHashCode(): {0}", x.GetHashCode());
            Console.WriteLine();
        }

    }
}
