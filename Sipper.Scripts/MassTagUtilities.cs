using System;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using GwsDMSUtilities;
using NUnit.Framework;
using Sipper.Model;

namespace Sipper.Scripts
{
    [TestFixture]
    public class MassTagUtilities
    {


        [Test]
        public void GetMassTagsFromDB()
        {
            string dbname = "MT_Yellowstone_Communities_P627";
            string serverName = "pogo";
            var infoExtractor = new PeptideInfoExtractor(serverName, dbname);

            int[] mtids = {17440471, 17460627};

            var peptides=  infoExtractor.GetPeptideInfo(mtids, true);

            string exportFilename = "testMassTagsExported.txt";
            DeconTools.Backend.FileIO.MassTagTextFileExporter exporter = new MassTagTextFileExporter(exportFilename);
            exporter.ExportResults(peptides);


        }


        [Test]
        public void ParseDescriptionAndGetOrganism()
        {
            string testMassTagFile =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_MassTags.txt";
            DeconTools.Backend.FileIO.MassTagFromTextFileImporter importer =
                new MassTagFromTextFileImporter(testMassTagFile);

            var masstags = importer.Import();

            var testMassTags = masstags.TargetList.Take(10).ToList();


            foreach (PeptideTarget peptideTarget in testMassTags)
            {
                
                var infoObject = JvciPeptideInfo.Parse(peptideTarget.ProteinDescription);

                Console.WriteLine(peptideTarget.ProteinDescription + "\t"+ infoObject.Organism);


            }


        }


        
        [Test]
        public void ExportMassTagsWithInfoTest()
        {
            string testMassTagFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2012_05_07_Data for Laurey Steinke paper\c12_all_matching_2012_06_06_massTags.txt";

            ExportMassTagsWithJCVIInformation(testMassTagFile);

        }


        public void ExportMassTagsWithJCVIInformation(string massTagFile)
        {
            

            using (StreamWriter writer = new StreamWriter(massTagFile.ToLower().Replace("_masstags.txt", "_withMoreInfo_massTags.txt")))
            {
                using (StreamReader reader = new StreamReader(massTagFile))
                {

                    string header   =  reader.ReadLine();

                    writer.WriteLine(header + "\torf\treadID\tbegin\tend\torientation\tcommonName\torganism");

                    while (reader.Peek()!=-1)
                    {
                        string line = reader.ReadLine();

                        var splitLine = line.Split('\t');

                        int proteinDescCol = 12;
                        string proteinDescription = splitLine[proteinDescCol];

                        var jvciInfo = JvciPeptideInfo.Parse(proteinDescription);

                        string newdataToWrite = jvciInfo == null ? "" : jvciInfo.MetagenomicOrfString + "\t" + jvciInfo.ReadID + "\t" +
                            jvciInfo.Begin + "\t" + jvciInfo.End + "\t" + jvciInfo.Orientation + "\t" + jvciInfo.CommonName + "\t" +
                            jvciInfo.Organism;

                        writer.WriteLine(line + "\t" + newdataToWrite);


                    }


                }
            }

           



        }







        

    }
}
