using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using GwsDMSUtilities;
using NUnit.Framework;
using Sipper.Model;
using Sipper.Scripts.SipperPaper;

namespace Sipper.Scripts
{
    [TestFixture]
    public class SteinkePaperScripts
    {

        [Test]
        public void automaticallyOutputEnriched_FilterF1()
        {
            string resultFileName =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\Yellow_C13\_Yellow_C13_MergedResults.txt";


            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);

            var repo = importer.Import();

            var sipperResults = repo.Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();
            ResultFilteringUtilities.ApplyFilteringScheme1(sipperResults);

            var filteredResults = sipperResults.Where(p => p.PassesFilter).ToList();

            string outputFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper";

            FileInfo fileInfo = new FileInfo(resultFileName);




            string exportFileName = outputFolder + Path.DirectorySeparatorChar + fileInfo.Name.Replace("_MergedResults.txt", "_F1_Enriched_results.txt");
            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportFileName);
            exporter.ExportResults(filteredResults);





        }


        [Test]
        public void automaticallyOutputEnriched_FilterF2()
        {
            string resultFileName =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\Yellow_C13\_Yellow_C13_MergedResults.txt";


            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);

            var repo = importer.Import();

            var sipperResults = repo.Results.Select(p => (SipperLcmsFeatureTargetedResultDTO)p).ToList();
            ResultFilteringUtilities.ApplyFilteringScheme2(sipperResults);

            var filteredResults = sipperResults.Where(p => p.PassesFilter).ToList();

            string outputFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper";

            FileInfo fileInfo = new FileInfo(resultFileName);

            string exportFileName = outputFolder + Path.DirectorySeparatorChar + fileInfo.Name.Replace("_MergedResults.txt", "_F2_Enriched_results.txt");
            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportFileName);
            exporter.ExportResults(filteredResults);
        }



        [Test]
        public void OutputImageResults()
        {
            FileInputsInfo fileInputs = new FileInputsInfo();

            fileInputs.DatasetDirectory = @"F:\Yellowstone\RawData";
            fileInputs.ResultsSaveFilePath = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\Visuals";
            fileInputs.TargetsFilePath =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\_Yellow_C13_F2_Enriched_results.txt";

            fileInputs.ParameterFilePath =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\SipperTargetedWorkflowParameters_Sum5.xml";


            ResultImageOutputter imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }


        [Test]
        public void filterOutRedundantResults_FilterF1()
        {
            //In a dataset, one feature is commonly mapped to multiple massTagIDs. Here we are filtering the out the redundant ones.


            string resultFileName =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\_Yellow_C13_F1_Enriched_results.txt";
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);

            var repo = importer.Import();

            var datasetNames = repo.Results.GroupBy(x => x.DatasetName).Select(g => g.First().DatasetName).ToList();

            List<TargetedResultDTO> exportedNonRedundant = new List<TargetedResultDTO>();

            foreach (var datasetName in datasetNames)
            {
                var datasetResults = repo.Results.Where(n => n.DatasetName == datasetName).ToList();

                var nonRedundant = datasetResults.GroupBy(x => x.TargetID).Select(g => g.First()).ToList();

                exportedNonRedundant.AddRange(nonRedundant);
            }

            string exportFileName = resultFileName.Replace("_results.txt", "_nonRedundant_results.txt");

            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportFileName);
            exporter.ExportResults(exportedNonRedundant);



        }



        [Test]
        public void filterOutRedundantResults_FilterF2()
        {
            //In a dataset, one feature is commonly mapped to multiple massTagIDs. Here we are filtering the out the redundant ones.


            string resultFileName =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\_Yellow_C13_F2_Enriched_results.txt";
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFileName);

            var repo = importer.Import();

            var datasetNames = repo.Results.GroupBy(x => x.DatasetName).Select(g => g.First().DatasetName).ToList();

            List<TargetedResultDTO> exportedNonRedundant = new List<TargetedResultDTO>();

            foreach (var datasetName in datasetNames)
            {
                var datasetResults = repo.Results.Where(n => n.DatasetName == datasetName).ToList();

                var nonRedundant = datasetResults.GroupBy(x => x.TargetID).Select(g => g.First()).ToList();

                exportedNonRedundant.AddRange(nonRedundant);
            }

            string exportFileName = resultFileName.Replace("_results.txt", "_nonRedundant_results.txt");

            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportFileName);
            exporter.ExportResults(exportedNonRedundant);



        }


        [Test]
        public void OutputImageResults_nonRedundant()
        {
            FileInputsInfo fileInputs = new FileInputsInfo();

            fileInputs.DatasetDirectory = @"F:\Yellowstone\RawData";
            fileInputs.ResultsSaveFilePath = @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\Visuals";
            fileInputs.TargetsFilePath =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\_Yellow_C13_F2_Enriched_nonRedundant_results.txt";

            fileInputs.ParameterFilePath =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\SipperTargetedWorkflowParameters_Sum5.xml";


            ResultImageOutputter imageOutputter = new ResultImageOutputter(fileInputs);
            imageOutputter.Execute();
        }


        [Test]
        public void CountAminoAcidFrequencies()
        {
            //string peptidedatafile =
            //    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\peptide sequences for enriched peptides.txt";

            ////peptidedatafile =
            ////    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\peptide sequences - all.txt";


            //peptidedatafile =
            //    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\Roseiflexus - no and maybe peptides.txt";

            //peptidedatafile =
            //    @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\Roseiflexus - Yes peptides.txt";

            string folderContainingDatafiles =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_07_06_Laurey amino acid frequency study\PeptideTextFiles2";

            DirectoryInfo dirinfo = new DirectoryInfo(folderContainingDatafiles);
            var datafileList = dirinfo.GetFiles("*.txt").Select(p => p.FullName);
            
           
            foreach (var datafile in datafileList)
            {
                Script_GetAminoAcidFrequencies.DisplayAminoAcidFrequencies(datafile);
            }


           



        }

        


        [Test]
        public void UpdateMassTag_MSGFScores()
        {
            List<int> massTagIDs = new List<int>();

            string mtidFilename =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\massTagIDs_needing_MSGFScore_updates.txt";

            using (StreamReader reader = new StreamReader(mtidFilename))
            {
                while (reader.Peek() != -1)
                {
                    string line = reader.ReadLine();

                    int val = Convert.ToInt32(line);

                    massTagIDs.Add(val);

                }
            }

            Dictionary<int, double> massTagMSGFScores = new Dictionary<int, double>();
            Dictionary<int, int> pmtQualityScores = new Dictionary<int, int>();

            string filecontainingMassTagInfo =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\c12_c13_all_matching_2012_06_06_withMoreInfo_massTags.txt";


            int indexMSGFCol = 5;
            int massTagidCol = 0;
            int pmtqCol = 6;

            using (StreamReader reader = new StreamReader(filecontainingMassTagInfo))
            {
                reader.ReadLine();

                while (reader.Peek() != -1)
                {
                    var parsedString = reader.ReadLine().Split('\t');

                    int masstagid = Convert.ToInt32(parsedString[0]);
                    double msgfScore = Convert.ToDouble(parsedString[indexMSGFCol]);
                    int pmtscore = (int)Convert.ToDouble(parsedString[pmtqCol]);

                    if (!massTagMSGFScores.ContainsKey(masstagid))
                    {
                        massTagMSGFScores.Add(masstagid, msgfScore);
                        pmtQualityScores.Add(masstagid, pmtscore);

                    }

                }


            }



            StringBuilder sb = new StringBuilder();

            foreach (var massTagID in massTagIDs)
            {

                double msgfScore = 1;
                double pmtQualityScore = -1;

                if (massTagMSGFScores.ContainsKey(massTagID))
                {
                    msgfScore = massTagMSGFScores[massTagID];
                }

                if (pmtQualityScores.ContainsKey(massTagID))
                {
                    pmtQualityScore = pmtQualityScores[massTagID];
                }


                sb.Append(massTagID + "\t" + msgfScore + "\t" + pmtQualityScore + Environment.NewLine);

            }

            Console.WriteLine(sb.ToString());


        }


        /// <summary>
        /// This gathers 
        /// </summary>
        [Test]
        public void GatherAllLCMSFeaturesFiles()
        {
            string baseLCMSFeaturesFolder = @"\\pogo\MTD_Peak_Matching\results\MT_Yellowstone_Communities_P627\VOrbi05";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\548_YSIP_C12_C13_data_analysis\ViperOutput";

            int[] jobIDs = new int[]
                               {
                                   811873, 811875, 811877, 811879, 811881, 811883, 811885, 811887, 811889, 811891, 811893,
                                   811895, 811897, 811899, 811901, 811903, 811905, 811907, 811909, 811911, 811872, 811874,
                                   811876, 811878, 811880, 811882, 811884, 811886, 811888, 811890, 811892, 811894, 811896,
                                   811898, 811900, 811902, 811904, 811906, 811908, 811910, 822717, 822719, 817365, 822735,
                                   822737, 817367, 822721, 822723, 822725, 822727, 822729, 822731, 822733, 822734, 822715,
                                   822716, 822718, 817364, 822736, 822738, 817366, 822720, 822722, 822724, 828790, 803734,
                                   803736, 828792, 803738, 803740, 803742, 803744, 803746, 803748, 803750, 803779, 803773,
                                   803775, 803777, 803752, 822726, 822728, 822730, 822732, 803754, 803756, 803758, 803760,
                                   803762, 803764, 803732, 803767, 803769, 803771, 807858, 807862, 807866, 807870, 807874,
                                   807877, 807881, 807885, 807889, 807893, 807897, 807901, 807905, 807909, 807913, 807917,
                                   807920, 807924, 807927, 807930, 807857, 807861, 807865, 807869, 807873, 807876, 807880,
                                   807884, 807888, 807892, 807896, 807900, 807904, 807908, 807912, 807916, 807919, 807923,
                                   807926, 807929, 807859, 807863, 807867, 807871, 807875, 807878, 807882, 807886, 807890,
                                   807894, 807898, 807902, 807906, 807910, 807914, 807918, 807921, 807925, 807928, 807931,
                                   803733, 828801, 828791, 803735, 803737, 803739, 803741, 803743, 803745, 803747, 803749,
                                   803751, 803753, 803755, 803757, 803759, 803761, 803763, 803765, 803766, 803768, 803770,
                                   803772, 803774, 803776, 803778, 807856, 807860, 807864, 807868, 807872, 805080, 807922,
                                   807879, 807883, 807887, 807891, 807895, 807899, 807903, 807907, 807911, 807915
                               };


            //int[] jobIDs = new int[]
            //                   {
            //                       811873, 811875
            //                   };

            var parentFolder = new DirectoryInfo(baseLCMSFeaturesFolder);

            var allJobFolders = parentFolder.GetDirectories("Job*").ToList();


            GwsDMSUtilities.DatasetAndJobInfoExtractor datasetAndJobInfoExtractor = new DatasetAndJobInfoExtractor();



            foreach (var jobID in jobIDs)
            {
                var jobFolder = allJobFolders.Where(p => p.Name.Contains(jobID.ToString())).FirstOrDefault();


                string datasetname = datasetAndJobInfoExtractor.GetDatabaseNameForJob(jobID);

                if (jobFolder != null)
                {
                    string expectedUMCFile = jobFolder.FullName + Path.DirectorySeparatorChar + "Job" + jobID +
                                             "_UMCs.txt";

                    if (File.Exists(expectedUMCFile))
                    {
                        Console.WriteLine(expectedUMCFile);

                        string newFilename = outputFolder + Path.DirectorySeparatorChar + datasetname + "_LCMSFeatures.txt";
                        File.Copy(expectedUMCFile, newFilename, true);

                    }
                    else
                    {
                        Console.WriteLine("----------------- missing the feature file: " + expectedUMCFile);
                    }
                }
                else
                {
                    Console.WriteLine("!!!!  Cannot find Job folder. Job = " + jobID);
                }
            }


        }


        /// <summary>
        /// Viper output shows all LCMSFeatures. We are only interested in those with an identification.
        /// Also, there is a redundancy in the LCMSFeatures (often matched to multiple mass tags). 
        /// This filters out redundant features
        /// </summary>
        [Test]
        public void FilterLCMSFeaturesForOnesHavingID()
        {
            string sourceFolder =
                @"\\protoapps\DataPkgs\Public\2012\548_YSIP_C12_C13_data_analysis\LCMSFeatures output from VIPER\Original";

            string outputFolder =
                @"\\protoapps\DataPkgs\Public\2012\548_YSIP_C12_C13_data_analysis\LCMSFeatures output from VIPER\Filtered";

            int massTagIDCol = 26;


            var dirInfo = new DirectoryInfo(sourceFolder);

            var fileInfos = dirInfo.GetFiles("*_LCMSFeatures.txt");

            foreach (var fileInfo in fileInfos)
            {

                //fileInfo.FullName.Replace("_LCMSFeatures.txt", "_targets.txt");

                string outputFileName = outputFolder + Path.DirectorySeparatorChar + fileInfo.Name.Replace("_LCMSFeatures.txt", "_targets.txt");

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


                            if (massTagID > 0)
                            {
                                if (!featureIDs.Contains(featureID))
                                {
                                    writer.WriteLine(line);
                                    featureIDs.Add(featureID);
                                }
                            }
                        }

                        reader.Close();
                    }

                    writer.Close();

                }
            }


        }

        [Test]
        public void OutputMassTagIDsAllIdentified()
        {
            string sourceFolder =
                @"\\protoapps\DataPkgs\Public\2012\548_YSIP_C12_C13_data_analysis\LCMSFeatures output from VIPER\Original";

            string outputFile =
                @"\\protoapps\DataPkgs\Public\2012\548_YSIP_C12_C13_data_analysis\LCMSFeatures output from VIPER\Matched_massTagIDs.txt";

            string massTagsFile = @"\\protoapps\DataPkgs\Public\2012\548_YSIP_C12_C13_data_analysis\massTag_reference_file.txt";



            int massTagIDCol = 26;


            var dirInfo = new DirectoryInfo(sourceFolder);

            var fileInfos = dirInfo.GetFiles("*_LCMSFeatures.txt");

            HashSet<int> massTagIDs = new HashSet<int>();


            foreach (var fileInfo in fileInfos)
            {

                //fileInfo.FullName.Replace("_LCMSFeatures.txt", "_targets.txt");


                List<int> featureIDs = new List<int>();
    

                using (StreamReader reader = new StreamReader(fileInfo.FullName))
                {
                    reader.ReadLine();

                    while (reader.Peek() != -1)
                    {
                        string line = reader.ReadLine();

                        var parsedLine = line.Split('\t');

                        int massTagID = Convert.ToInt32(parsedLine[massTagIDCol]);
          
                        if (massTagID > 0)
                        {
                            massTagIDs.Add(massTagID);

                        }
                    }

                    reader.Close();



                }
            }

            var list=  massTagIDs.OrderBy(p => p).ToList();

            using (StreamWriter writer=new StreamWriter(outputFile))
            {

                foreach (var massTagID in list)
                {
                    writer.WriteLine(massTagID);
                }
 
            }

            string dbname = "MT_Yellowstone_Communities_P627";
            string serverName = "pogo";

            GwsDMSUtilities.PeptideInfoExtractor peptideInfoExtractor = new PeptideInfoExtractor(serverName, dbname);
            var peptideInfo = peptideInfoExtractor.GetPeptideInfo(list,true);

            DeconTools.Backend.FileIO.MassTagTextFileExporter massTagTextFileExporter =
                new MassTagTextFileExporter(massTagsFile);

            peptideInfo = peptideInfo.OrderBy(p => p.ID).ToList();

            massTagTextFileExporter.ExportResults(peptideInfo);



        }






        private int GetAminoAcidCount(char aminoacidCode, List<string> peptideSequences)
        {
            throw new NotImplementedException();
        }
    }
}
