using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation.LabeledIsotopicDistUtilities;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FigureXX_LILS_score_demo
    {
        [Category("Paper")]
        [Test]
        public void OutputResultsForFeature8616_LILSDemo()
        {
            string paramFile =
          @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\ExecutorParameters1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = false;
            parameters.TargetType = Globals.TargetType.LcmsFeature;


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            var executor = new BasicTargetedWorkflowExecutor(parameters, testDataset);
            int testTarget = 8616;

            executor.Targets.TargetList = executor.Targets.TargetList.Where(p => p.ID == testTarget).ToList();
            executor.Execute();

            var msData = executor.TargetedWorkflow.MassSpectrumXYData;

            var chromData = executor.TargetedWorkflow.ChromatogramXYData;

            var result = executor.TargetedWorkflow.Result;


            SipperTargetedWorkflow workflow = (SipperTargetedWorkflow) executor.TargetedWorkflow;

            //the observed composite profile
            TestUtilities.DisplayIsotopicProfileData(result.IsotopicProfile);


            Console.WriteLine();

            //the high quality subtracted iso
            TestUtilities.DisplayIsotopicProfileData(workflow.SubtractedIso);

            Console.WriteLine();

            foreach (var item in workflow.FitScoreData)
            {
                Console.WriteLine(item.Key + "\t" + item.Value);
            }

           

             LabeledIsotopicProfileUtilities isoCreator = new LabeledIsotopicProfileUtilities();

            double percentIncorp = 8.25;
            var labeledIso= isoCreator.CreateIsotopicProfileFromEmpiricalFormula(result.Target.EmpiricalFormula, "C", 12, 13, percentIncorp, result.Target.ChargeState);


            Console.WriteLine();
            Console.WriteLine("----- labeled iso at " + percentIncorp + "%");
            TestUtilities.DisplayIsotopicProfileData(labeledIso);



        }


        [Category("Paper")]
        [Test]
        public void GetLisrScoresForAllConfirmedPeptidesFraction70()
        {


            string paramFile =
 @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\ExecutorParameters1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);

            parameters.FolderPathForCopiedRawDataset = @"D:\data\temp";
            parameters.CopyRawFileLocal = false;
            parameters.TargetType = Globals.TargetType.LcmsFeature;


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";


            var executor = new BasicTargetedWorkflowExecutor(parameters, testDataset);
            int[] confirmedIds = {
                                     5555, 5677, 5746, 5905, 6496, 6968, 7039, 7116, 7220, 7229, 7585, 7699, 8221, 8338, 8491, 8517, 8616,
                                     8618, 8715, 8947, 8958, 8968, 8973, 9240, 9261, 9328, 9441, 9474, 9583, 9706,9965, 10223, 10251,
                                     10329, 10649, 10856, 10870, 11249, 11367, 11523, 11677, 11912, 12178, 12187, 12304, 12395, 12492, 12517
                                     , 12571, 12700, 12828, 13107, 13443, 13494, 13525, 13590, 13740, 13922, 14090, 14256
                                 };



            string noLabelCuratedFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_NO_validated.txt";

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(noLabelCuratedFile);
            confirmedIds = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select (int)n.TargetID).ToArray();


            string maybeCuratedFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_MAYBE_validated.txt";

            importer = new SipperResultFromTextImporter(maybeCuratedFile);
            confirmedIds = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select (int)n.TargetID).ToArray();


            executor.Targets.TargetList = (from n in executor.Targets.TargetList where confirmedIds.Contains(n.ID) select n).ToList();

            executor.InitializeRun(testDataset);

            executor.TargetedWorkflow.Run = executor.Run;

            StringBuilder sb = new StringBuilder();

            Dictionary<int, double> lisrScoreDictionary = new Dictionary<int, double>();

            int totalTargets = executor.Targets.TargetList.Count;
            int targetCounter = 1;

            foreach (var target in executor.Targets.TargetList)
            {
                Console.WriteLine("Working on " + targetCounter + " of " + totalTargets);
                targetCounter++;

                executor.TargetedWorkflow.Run.CurrentMassTag = target;
                executor.TargetedWorkflow.Execute();

                SipperLcmsTargetedResult result = (SipperLcmsTargetedResult) executor.TargetedWorkflow.Result;

                sb.Append(result.Target.ID + "\t" + result.Target.ChargeState + "\t" + result.Target.MZ.ToString("0.0000") + "\t" +
                    result.IntensityAggregate + "\t"+   result.AreaUnderRatioCurveRevised+ "\t"+  result.FitScoreLabeledProfile);
                sb.Append(Environment.NewLine);




            }

            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine(sb.ToString());


        }




    }
}
