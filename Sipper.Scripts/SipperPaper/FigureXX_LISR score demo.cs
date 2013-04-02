using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FigureXX_LISR_score_demo
    {
        [Category("Paper")]
        [Test]
        public void GetInfoForFeature8616()
        {
            //344968841	1547.738815	DSEIGDLIAEVMEK	0		0		22	0	0.5948741	40198	fibr_21528106.1	JCVI_PEP_metagenomic.orf.21528106.1 /read_id=1113211797790 /begin=25037 /end=26717 /orientation=-1 /common_name="chaperonin GroL" /organism="Roseiflexus sp. 

            int testTarget = 8616;
            string sequence = "DSEIGDLIAEVMEK";
            short chargeState = 2;
            string empFormula = "C65H109N15O26S";


            LcmsFeatureTarget target = new LcmsFeatureTarget();
            target.ID = testTarget;
            target.ChargeState = chargeState;
            target.EmpiricalFormula = empFormula;
            target.Code = sequence;
            target.MonoIsotopicMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empFormula);

            DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator.JoshTheorFeatureGenerator theorFeatureGenerator =
                new JoshTheorFeatureGenerator();
            theorFeatureGenerator.LowPeakCutOff = 1e-10;

            theorFeatureGenerator.GenerateTheorFeature(target);
            TestUtilities.DisplayIsotopicProfileData(target.IsotopicProfile);


        }

        [Category("Paper")]
        [Test]
        public void OutputResultsForFeature8616()
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


            //output chrom correlation data
            int counter = 0;
            foreach (var item in result.ChromCorrelationData.CorrelationDataItems)
            {
                Console.WriteLine(counter + "\t" + item.CorrelationRSquaredVal);
                counter++;

            }

            //output observed isotopic profile
            TestUtilities.DisplayIsotopicProfileData(result.IsotopicProfile);




            //TestUtilities.DisplayXYValues(chromData);



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

                var result = executor.TargetedWorkflow.Result;

                sb.Append(Environment.NewLine);
                sb.Append(result.Target.ID + "; z= " + result.Target.ChargeState + "; m/z= "+ result.Target.MZ.ToString("0.0000"));
                sb.Append(Environment.NewLine);


                double lisrScore=0;
                if (result.IsotopicProfile!=null)
                {

                    List<double> lisrRatios = new List<double>();

                    for (int index = 0; index < result.IsotopicProfile.Peaklist.Count; index++)
                    {
                        var msPeak = result.IsotopicProfile.Peaklist[index];
                        
                        if (index>=result.Target.IsotopicProfile.Peaklist.Count)continue;
                        var theorPeak = result.Target.IsotopicProfile.Peaklist[index];

                        var chromCorrValue = result.ChromCorrelationData.CorrelationDataItems[index].CorrelationRSquaredVal;
                        var relIntens = msPeak.Height/result.IsotopicProfile.getMostIntensePeak().Height;

                        var lisrRatio = Math.Max(0,(relIntens - theorPeak.Height))/theorPeak.Height;

                        if (chromCorrValue > 0.7)
                        {
                            lisrRatios.Add(lisrRatio);
                        }


                        sb.Append(index + "\t" + theorPeak.XValue.ToString("0.0000")+  "\t" + relIntens + "\t" + theorPeak.Height + "\t" + chromCorrValue + "\t" + lisrRatio);
                        sb.Append(Environment.NewLine);

                    }

                    lisrScore = lisrRatios.Sum();

                    sb.Append("LISR=\t"+ lisrScore);
                    sb.Append(Environment.NewLine);

                    

                }

                lisrScoreDictionary.Add(result.Target.ID, lisrScore);




            }

            sb.Append(Environment.NewLine);
            sb.Append("-------------- Lisr score report --------------\n");
            sb.Append("featureID\tLISR score\n");
            foreach (var item in lisrScoreDictionary)
            {
                sb.Append(item.Key + "\t" + item.Value + "\n");
            }


            Console.WriteLine(sb.ToString());


        }





    }
}
