using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend.Core;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FilterResultsBasedOnSTAC
    {
        [Test]
        public void Test1()
        {
            string executorParamFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\SipperExecutorParams1.xml";

            string runFilename =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(executorParamFile);


            parameters.DbTableName =
                @"[MT_Yellowstone_Communities_P627].[PNL\D3X720].[T_Tmp_Slysz_PeakMatchingResultsWithSTAC]";

            parameters.DbName = "";


            //SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, runFilename);

            //Console.WriteLine(executor.Targets.TargetList.Count);

            string targetsFileName =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\All_LcmsFeatures_StacFiltered.txt";
            DeconTools.Backend.FileIO.LcmsTargetFromFeaturesFileImporter importer =
                new LcmsTargetFromFeaturesFileImporter(targetsFileName);

            var targets = importer.Import();

            string fileContainingListofFiles =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\fileList.txt";

            StreamReader listReader = new StreamReader(fileContainingListofFiles);
            List<string> resultFileList = new List<string>();
            while (listReader.Peek() != -1)
            {
                resultFileList.Add(listReader.ReadLine());
            }
            listReader.Close();


            foreach (var file in resultFileList)
            {
                int datasetNameIndex = 0;
                int featureIDIndex = 1;
                int massTagIDIndex = 22;


                string outputFilename = file.Replace(".txt", "_fSTAC.txt");

                using (StreamReader reader = new StreamReader(file))
                {


                    using (StreamWriter writer = new StreamWriter(outputFilename))
                    {



                        writer.WriteLine(reader.ReadLine());

                        bool isFirstLine = true;

                        List<DeconTools.Backend.Core.LcmsFeatureTarget> filteredTargets = new List<LcmsFeatureTarget>();
                       // Console.WriteLine(file);
                        int counter = 0;
                        while (reader.Peek() != -1)
                        {
                          
                            string line = reader.ReadLine();

                            var parsedLine = line.Split('\t');

                            if (isFirstLine)
                            {
                                string datasetName = parsedLine[datasetNameIndex];
                                filteredTargets =
                                    targets.TargetList.Where(p => ((LcmsFeatureTarget)p).DatabaseName == datasetName).
                                        Select(n => (LcmsFeatureTarget)n).ToList();
                                isFirstLine = false;

                            }

                            int featureID = Convert.ToInt32(parsedLine[featureIDIndex]);
                            int massTagID = Convert.ToInt32(parsedLine[massTagIDIndex]);

                            bool writeOutLine = false;

                            if (massTagID > 0)
                            {
                                var goodMassTagIDs =
                                    filteredTargets.Where(p => p.ID == featureID).Select(p => p.FeatureToMassTagID).
                                        ToList();
                                if (goodMassTagIDs.Contains(massTagID))
                                {

                                    writeOutLine = true;
                                }
                            }
                            else
                            {
                                writeOutLine = true;
                                
                            }

                            if (writeOutLine)
                            {
                                counter++;
                                writer.WriteLine(line);
                            }
                            

                        }
                        Console.WriteLine(file + "\t" + counter);
                    }
                    
                }


            }






        }

    }
}
