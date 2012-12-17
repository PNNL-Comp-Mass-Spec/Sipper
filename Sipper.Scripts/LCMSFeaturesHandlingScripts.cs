using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Workflows.Backend.Core;
using GwsDMSUtilities;
using NUnit.Framework;

namespace Sipper.Scripts
{
    [TestFixture]
    public class LCMSFeaturesHandlingScripts
    {
        [Test]
        public void GatherAllLCMSFeaturesFiles()
        {
            string baseLCMSFeaturesFolder = @"\\pogo\MTD_Peak_Matching\results\MT_Yellowstone_Communities_P627\VOrbi05";

            string ouputFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_06_13_LCMSFeatures_From_YSIP_data";

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

                        string newFilename = ouputFolder + Path.DirectorySeparatorChar + datasetname + "_LCMSFeatures.txt";
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

        [Test]
        public void FilterLCMSFeaturesForOnesHavingID()
        {
            string sourceFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_06_13_LCMSFeatures_From_YSIP_data";

            sourceFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets";

            sourceFolder =
                @"\\protoapps\DataPkgs\Public\2012\622_Sipper_C13_analysis_of_initial_time_course_study\Targets";


            int massTagIDCol = 26;

            //massTagIDCol = 25;


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


                            if (massTagID>0)
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
        public void FilterLCMSFeaturesForOnesHavingID_2()
        {
            string sourceFolder = @"C:\Sipper\SipperDemo\SipperInputs\Targets";

            int massTagIDCol = 25;


            var dirInfo = new DirectoryInfo(sourceFolder);

            var fileInfos = dirInfo.GetFiles("*_LCMSFeatures.txt");

            foreach (var fileInfo in fileInfos)
            {
                string outputFileName = fileInfo.FullName.Replace("_LCMSFeatures.txt", "_f0_NR_LCMSFeatures.txt");

                using (StreamWriter writer = new StreamWriter(outputFileName))
                {
                    List<int> featureIDs = new List<int>();

                    using (StreamReader reader = new StreamReader(fileInfo.FullName))
                    {
                        writer.WriteLine(reader.ReadLine());


                        int counter = 0;
                        while (reader.Peek() != -1)
                        {
                            counter++;

                            string line = reader.ReadLine();

                            var parsedLine = line.Split('\t');

                            int massTagID=0;
                            int featureID=0;
                            try
                            {
                                
                                massTagID = Convert.ToInt32(parsedLine[massTagIDCol]);

                               
                                featureID = Convert.ToInt32(parsedLine[0]);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Error on line= " + counter);

                            }

                         


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
        public void GetLCMSFeaturesFromDatabaseTest1()
        {
            




        }


  


    }
}
