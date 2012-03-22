using System;
using System.Collections.Generic;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.ChromatogramProcessing;
using DeconTools.Backend.ProcessingTasks.FitScoreCalculators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.ProcessingTasks.ResultValidators;
using DeconTools.Backend.ProcessingTasks.Smoothers;
using DeconTools.Backend.ProcessingTasks.TargetedFeatureFinders;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Workflows.Backend.Core;
using Sipper.Backend.Results;

namespace Sipper.Backend
{
    public class SipperTargetedWorkflow:TargetedWorkflow
    {

        private JoshTheorFeatureGenerator _theorFeatureGen;
        private PeakChromatogramGenerator _chromGen;
        private DeconToolsSavitzkyGolaySmoother _chromSmoother;
        private ChromPeakDetector _chromPeakDetector;
        private ResultValidatorTask _resultValidator;

        private ChromPeakSelectorBase _chromPeakSelector;

        //private SmartChromPeakSelector chromPeakSelector;

        private TFFBase _iterativeMSFeatureFinder;
        private DeconToolsPeakDetector _msPeakDetector;
        private MassTagFitScoreCalculator _fitScoreCalc;
        private SipperQuantifier _quantifier;

        #region Constructors

        public SipperTargetedWorkflow(Run run, TargetedWorkflowParameters parameters)
        {
            WorkflowParameters = parameters;

            Run = run;
            InitializeWorkflow();
                
        }

        public override sealed void InitializeWorkflow()
        {
            ValidateParameters();

            _theorFeatureGen = new JoshTheorFeatureGenerator(DeconTools.Backend.Globals.LabellingType.NONE, 0.0001);

            _chromGen = new PeakChromatogramGenerator(_workflowParameters.ChromToleranceInPPM, _workflowParameters.ChromGeneratorMode);
            _chromGen.TopNPeaksLowerCutOff = 0.333;

            int pointsToSmooth = (_workflowParameters.ChromSmootherNumPointsInSmooth + 1) / 2;   // adding 0.5 prevents rounding problems
            _chromSmoother = new DeconToolsSavitzkyGolaySmoother(pointsToSmooth, pointsToSmooth, 2);
            _chromPeakDetector = new ChromPeakDetector(_workflowParameters.ChromPeakDetectorPeakBR, _workflowParameters.ChromPeakDetectorSigNoise);


            _chromPeakSelector = CreateChromPeakSelector(_workflowParameters);


            _msPeakDetector = new DeconToolsPeakDetector(_workflowParameters.MSPeakDetectorPeakBR, _workflowParameters.MSPeakDetectorSigNoise, DeconTools.Backend.Globals.PeakFitType.QUADRATIC, false);

            IterativeTFFParameters iterativeTFFParameters = new IterativeTFFParameters();
            iterativeTFFParameters.ToleranceInPPM = _workflowParameters.MSToleranceInPPM;

            _iterativeMSFeatureFinder = new SipperIterativeMSFeatureFinder(iterativeTFFParameters);
           // _iterativeMSFeatureFinder = new IterativeTFF(iterativeTFFParameters);

            _quantifier = new SipperQuantifier();
            _fitScoreCalc = new MassTagFitScoreCalculator();
            _resultValidator = new ResultValidatorTask();

            ChromatogramXYData = new XYData();
            MassSpectrumXYData = new XYData();
            ChromPeaksDetected = new List<ChromPeak>();
        }


        public override void Execute()
        {
            ResetStoredData();

            try
            {

                if (Run.ResultCollection.MassTagResultList.ContainsKey(Run.CurrentMassTag))
                {
                    Run.ResultCollection.CurrentTargetedResult =
                        Run.ResultCollection.MassTagResultList[Run.CurrentMassTag];
                }
                else
                {
                     Run.ResultCollection.CurrentTargetedResult=  CreateMassTagResult(Run.CurrentMassTag);
                }

                Result = Run.ResultCollection.CurrentTargetedResult;
                Result.ResetResult();


                ExecuteTask(_theorFeatureGen);
                ExecuteTask(_chromGen);
                ExecuteTask(_chromSmoother);
                updateChromDataXYValues(Run.XYData);

                ExecuteTask(_chromPeakDetector);
                updateChromDetectedPeaks(Run.PeakList);

                ExecuteTask(_chromPeakSelector);
                ChromPeakSelected = Result.ChromPeakSelected;


                Result.ResetMassSpectrumRelatedInfo();


                ExecuteTask(MSGenerator);
                updateMassSpectrumXYValues(Run.XYData);

                double minMZ = Run.CurrentMassTag.MZ - 3;
                double maxMz = Run.CurrentMassTag.MZ + 20;
                Run.XYData = Run.XYData.TrimData(minMZ, maxMz);

                ExecuteTask(_iterativeMSFeatureFinder);
                ExecuteTask(_fitScoreCalc);
                ExecuteTask(_resultValidator);

                ExecuteTask(_quantifier);


            }
            catch (Exception ex)
            {
                TargetedResultBase result = Run.ResultCollection.CurrentTargetedResult;
                result.ErrorDescription = ex.Message + "\n" + ex.StackTrace;
                Console.WriteLine(((LcmsFeatureTarget)result.Target).FeatureToMassTagID + "; "+ result.ErrorDescription);

                return;
            }
        }


        public TargetedResultBase CreateMassTagResult(TargetBase massTag)
        {
            TargetedResultBase result = new SipperTargetedResult(massTag);
           

            Run.ResultCollection.MassTagResultList.Add(massTag, result);
            result.MSFeatureID = Run.ResultCollection.MSFeatureCounter;
            result.Score = 1;
            result.Run = this.Run;

            Run.ResultCollection.MSFeatureCounter++;
            return result;
        }


        #endregion

        #region Properties
        TargetedWorkflowParameters _workflowParameters;
        public override WorkflowParameters WorkflowParameters
        {
            get
            {
                return _workflowParameters;
            }
            set
            {
                _workflowParameters = value as TargetedWorkflowParameters;
            }
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        private void ValidateParameters()
        {
            bool pointsInSmoothIsEvenNumber = (_workflowParameters.ChromSmootherNumPointsInSmooth % 2 == 0);
            if (pointsInSmoothIsEvenNumber)
            {
                throw new ArgumentOutOfRangeException("Points in chrom smoother is an even number, but must be an odd number.");
            }

            //add parameter validation

        }

        #endregion

    }
}
