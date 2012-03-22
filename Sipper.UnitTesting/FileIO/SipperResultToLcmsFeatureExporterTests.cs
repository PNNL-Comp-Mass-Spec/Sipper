
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;
using Sipper.Backend;
using Sipper.Backend.FileIO;
using Sipper.Backend.Results;

namespace Sipper.UnitTesting.FileIO
{
    public class SipperResultToLcmsFeatureExporterTests
    {

        [Test]
        public void executeWorkflowTest1()
        {
            string testFile = FileRefs.RawDataFile1;


            string exportedResultFile = FileRefs.OutputFolderPath + Path.DirectorySeparatorChar + "ExportedSipperResults1.txt";
            

            string peaksFile =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2011_02_20_SIPPER_workflow_standards\Yellow_C13_070_23Mar10_Griffin_10-01-28_peaks.txt";

            Run run = RunUtilities.CreateAndLoadPeaks(testFile, peaksFile);


            string lcmsfeaturesFile =
             @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2011_02_20_SIPPER_workflow_standards\Yellow_C13_070_23Mar10_Griffin_10-01-28_LCMSFeatures.txt";

            // load LCMSFeatures as targets
            LcmsTargetFromFeaturesFileImporter importer =
                new LcmsTargetFromFeaturesFileImporter(lcmsfeaturesFile);

            var lcmsTargetCollection = importer.Import();


            // load MassTags
            string massTagFile1 =
                @"C:\Users\d3x720\Documents\PNNL\My_DataAnalysis\2012\C12C13YellowStone\2011_02_20_SIPPER_workflow_standards\Yellow_C13_070_23Mar10_Griffin_10-01-28_MassTags.txt";

            MassTagFromTextFileImporter massTagImporter = new MassTagFromTextFileImporter(massTagFile1);
            var massTagCollection = massTagImporter.Import();

            //enriched
            int[] testMassTags = new int[] { 355116553, 355129038, 355160150, 355162540, 355163371 };


            var filteredLcmsFeatureTargets = (from n in lcmsTargetCollection.TargetList
                                              where testMassTags.Contains(((LcmsFeatureTarget)n).FeatureToMassTagID)
                                              select n).ToList();


            TargetCollection.UpdateTargetsWithMassTagInfo(filteredLcmsFeatureTargets, massTagCollection.TargetList);


            TargetedWorkflowParameters parameters = new BasicTargetedWorkflowParameters();
            parameters.ChromPeakSelectorMode = Globals.PeakSelectorMode.ClosestToTarget;

            SipperTargetedWorkflow workflow = new SipperTargetedWorkflow(run, parameters);


            foreach (var target in filteredLcmsFeatureTargets)
            {
                run.CurrentMassTag = target;

                workflow.Execute();


            }

           
            

            var results = run.ResultCollection.GetMassTagResults();

            var resultsInDTOFormat =
                (from n in results select new SipperTargetedResultDTO((SipperTargetedResult) n)).ToList(); 
            
            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(exportedResultFile);
            exporter.ExportResults(resultsInDTOFormat);



        }







    }
}
