using System.Linq;
using DeconTools.Backend.FileIO;
using NUnit.Framework;

namespace Sipper.Scripts
{
    [TestFixture]
    public class TargetUtilities
    {
        [Test]
        public void Test1()
        {

            string db = "MT_Yellowstone_Communities_P627";
            string server = "pogo";

            string outputFolder = @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\Targets";


           //Read LCMS targets
            string targetsFileName =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\All_LcmsFeatures_StacFiltered.txt";
            DeconTools.Backend.FileIO.LcmsTargetFromFeaturesFileImporter importer = new LcmsTargetFromFeaturesFileImporter(targetsFileName);

            var targets = importer.Import().TargetList.Select(p=>(DeconTools.Backend.Core.LcmsFeatureTarget)p).ToList();

            var massTagIDs = targets.Select(p => (long)p.FeatureToMassTagID).Distinct().ToList();

            


            //

            MassTagFromSqlDbImporter mtImporter = new MassTagFromSqlDbImporter(db, server, massTagIDs);
            var massTagsForReference = mtImporter.Import().TargetList;


            foreach (var lcmsFeatureTarget in targets)
            {
                var currentMassTag = massTagsForReference.First(p => p.ID == lcmsFeatureTarget.FeatureToMassTagID);

                if (currentMassTag == null)
                {
                    //Console.WriteLine(lcmsFeatureTarget.FeatureToMassTagID + " NOT FOUND in DB");
                }
                else
                {
                    lcmsFeatureTarget.EmpiricalFormula = currentMassTag.EmpiricalFormula;
                    lcmsFeatureTarget.Code = currentMassTag.Code;
                    //Console.WriteLine(currentMassTag);
                }


            }







            //Iterate over dataset names:
           var datasetNames=  SipperDatasetUtilities.GetDatasetNames();

           // datasetNames = datasetNames.Where(p => p.Contains("Yellow_C13_070")).ToList();


            //foreach (var datasetName in datasetNames)
            //{
            //    var filtered = targets.Where(p => p.DatabaseName == datasetName);


            //    string outputFilename = outputFolder + "\\" + datasetName + "_targets.txt";

            //    LcmsTargetToTextExporter exporter = new LcmsTargetToTextExporter(outputFilename);

            //    exporter.ExportResults(filtered);


            //}

            string allTargetsExportFilename = outputFolder + "\\" + "allTargets.txt";
            var allresultsExporter = new LcmsTargetToTextExporter(allTargetsExportFilename);

            allresultsExporter.ExportResults(targets);


        }

    }
}
