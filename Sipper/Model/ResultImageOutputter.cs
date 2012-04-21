using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using GWSGraphLibrary;

namespace Sipper.Model
{
    public class ResultImageOutputter
    {
        private FileInputsInfo _fileInputs;
        private BackgroundWorker _backgroundWorker;
        private TargetedResultRepository _resultRepositorySource;
        private TargetedWorkflowExecutorProgressInfo _progressInfo = new TargetedWorkflowExecutorProgressInfo();
        private const double DefaultMSPeakWidth = 0.01;


        private MSGraphControl _msGraph = new MSGraphControl();
        private ChromGraphControl _chromGraph = new ChromGraphControl();
        private MSGraphControl _theorMSGraph = new MSGraphControl();


        #region Constructors
        public ResultImageOutputter(FileInputsInfo fileInputs, BackgroundWorker worker = null)
        {
            _fileInputs = fileInputs;
            _backgroundWorker = worker;

            WorkflowParameters = new SipperTargetedWorkflowParameters();

            Workflow = new SipperTargetedWorkflow(WorkflowParameters);

            ChromGraphXWindowWidth = 400;

            InitializeGraphs();



        }

        private void InitializeGraphs()
        {
            UpdateGraphRelatedProperties();


            //For some reason, in the first drawing of the graph, the graph ranges aren't updated. So we
            //will do that here so that subsequent drawings will have the correct ranges
            _chromGraph.zedGraphControl1.GraphPane.GraphObjList.Clear();
            _chromGraph.GenerateGraph(ChromXYData.Xvalues, ChromXYData.Yvalues, ChromGraphMinX, ChromGraphMaxX);

            _msGraph.zedGraphControl1.GraphPane.GraphObjList.Clear();
            _msGraph.GenerateGraph(MassSpecXYData.Xvalues, MassSpecXYData.Yvalues, MSGraphMinX, MSGraphMaxX);

            _theorMSGraph.zedGraphControl1.GraphPane.GraphObjList.Clear();
            _theorMSGraph.GenerateGraph(MassSpecXYData.Xvalues, MassSpecXYData.Yvalues, MSGraphMinX, MSGraphMaxX);
        }

        #endregion

        #region Properties
        private Run _run;
        public Run Run
        {
            get { return _run; }
            set
            {
                _run = value;
                Workflow.Run = _run;

            }
        }



        public string CurrentDatasetName
        {
            get
            {
                if (Run == null)
                {
                    return String.Empty;
                }

                return Run.DatasetName;

            }
        }




        private SipperTargetedWorkflowParameters _workflowParameters;
        public SipperTargetedWorkflowParameters WorkflowParameters
        {
            get { return _workflowParameters; }
            set { _workflowParameters = value; }
        }



        private SipperTargetedWorkflow _workflow;
        public SipperTargetedWorkflow Workflow
        {
            get { return _workflow; }
            set
            {
                _workflow = value;

            }
        }


        #endregion

        #region Public Methods


        public void Execute()
        {

            //Load results
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(_fileInputs.TargetsFilePath);
            _resultRepositorySource = importer.Import();


            //Load Parameters
            WorkflowParameters.LoadParameters(_fileInputs.ParameterFilePath);

            //Sort by dataset
            var sortedDatasets = (from n in _resultRepositorySource.Results orderby n.DatasetName select n);

            //Set output folder


            //iterate over results
            foreach (SipperLcmsFeatureTargetedResultDTO result in sortedDatasets)
            {
                CurrentResult = result;


                if (result.DatasetName != CurrentDatasetName)
                {
                    if (Run != null)
                    {
                        Run.Close();
                    }


                    InitializeRun(result.DatasetName);
                }


                SetCurrentWorkflowTarget(result);

                Workflow.Execute();

                double fwhm;
                if (Workflow.Result.IsotopicProfile != null)
                {
                    fwhm = Workflow.Result.IsotopicProfile.GetFWHM();
                }
                else
                {
                    fwhm = DefaultMSPeakWidth;
                }

                TheorProfileXYData = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(Workflow.Result.Target.IsotopicProfile, fwhm);

                OutputImages();

            }




        }

        protected SipperLcmsFeatureTargetedResultDTO CurrentResult { get; set; }

        private void OutputImages()
        {
            if (!Directory.Exists(_fileInputs.ResultsSaveFilePath))
                Directory.CreateDirectory(_fileInputs.ResultsSaveFilePath);

            string baseFilename = _fileInputs.ResultsSaveFilePath + Path.DirectorySeparatorChar +  CurrentResult.DatasetName + "_ID" + CurrentResult.TargetID;

            string msfilename = baseFilename + "_MS.png";
            string theorMSFilename = baseFilename + "_theorMS.png";
            string chromFilename = baseFilename + "_chrom.png";

            UpdateGraphRelatedProperties();

            _chromGraph.zedGraphControl1.GraphPane.GraphObjList.Clear();
            _chromGraph.GenerateGraph(ChromXYData.Xvalues, ChromXYData.Yvalues, ChromGraphMinX, ChromGraphMaxX);

            _msGraph.zedGraphControl1.GraphPane.GraphObjList.Clear();
            _msGraph.GenerateGraph(MassSpecXYData.Xvalues, MassSpecXYData.Yvalues, MSGraphMinX, MSGraphMaxX);

            _theorMSGraph.zedGraphControl1.GraphPane.GraphObjList.Clear();
            _theorMSGraph.GenerateGraph(TheorProfileXYData.Xvalues, TheorProfileXYData.Yvalues, MSGraphMinX, MSGraphMaxX);


            _msGraph.SaveGraph(msfilename);
            _chromGraph.SaveGraph(chromFilename);
            _theorMSGraph.SaveGraph(theorMSFilename);



        }

        protected XYData TheorProfileXYData { get; set; }

        public double ChromGraphMaxX { get; set; }

        public double ChromGraphMinX { get; set; }

        public double ChromGraphXWindowWidth { get; set; }



        public double MSGraphMaxX { get; set; }

        public double MSGraphMinX { get; set; }


        private void UpdateGraphRelatedProperties()
        {
            ChromXYData = new XYData();
            ChromXYData.Xvalues = Workflow.ChromatogramXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.ChromatogramXYData.Xvalues;
            ChromXYData.Yvalues = Workflow.ChromatogramXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.ChromatogramXYData.Yvalues;

            MassSpecXYData = new XYData();
            MassSpecXYData.Xvalues = Workflow.MassSpectrumXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.MassSpectrumXYData.Xvalues;
            MassSpecXYData.Yvalues = Workflow.MassSpectrumXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.MassSpectrumXYData.Yvalues;



            if (CurrentResult != null)
            {
                MSGraphMinX = CurrentResult.MonoMZ - 2;
                MSGraphMaxX = CurrentResult.MonoMZ + 10;
            }
            else
            {
                MSGraphMinX = MassSpecXYData.Xvalues.Min();
                MSGraphMaxX = MassSpecXYData.Xvalues.Max();
            }


            if (CurrentResult != null)
            {
                ChromGraphMinX = CurrentResult.ScanLC - ChromGraphXWindowWidth / 2;
                ChromGraphMaxX = CurrentResult.ScanLC + ChromGraphXWindowWidth / 2;
            }
            else
            {
                ChromGraphMinX = ChromXYData.Xvalues.Min();
                ChromGraphMaxX = ChromXYData.Xvalues.Max();
            }



        }

        protected XYData MassSpecXYData { get; set; }

        protected XYData ChromXYData { get; set; }


        private void SetCurrentWorkflowTarget(SipperLcmsFeatureTargetedResultDTO result)
        {
            TargetBase target = new LcmsFeatureTarget();
            target.ChargeState = (short)result.ChargeState;
            target.ChargeStateTargets.Add(target.ChargeState);
            target.ElutionTimeUnit = Globals.ElutionTimeUnit.ScanNum;
            target.EmpiricalFormula = result.EmpiricalFormula;
            target.ID = (int)result.TargetID;


            target.IsotopicProfile = null;   //workflow will determine this

            target.MZ = result.MonoMZ;
            target.MonoIsotopicMass = result.MonoMass;
            target.ScanLCTarget = result.ScanLC;

            Run.CurrentMassTag = target;



        }


        private void InitializeRun(string datasetName)
        {

            //Datasets have to all be in the same folder
            //currently works for datasets that have a File reference as
            //opposed to datasets having a Folder reference (Agilent/Bruker)


            DirectoryInfo dirInfo = new DirectoryInfo(_fileInputs.DatasetDirectory);


            var fileInfo = dirInfo.GetFiles(datasetName + ".*");

            if (!fileInfo.Any())
            {
                throw new FileNotFoundException("Run could not be initialized. File not found");
            }

            RunFactory rf = new RunFactory();
            Run = rf.CreateRun(fileInfo.First().FullName);

            bool peaksFileExists = checkForPeaksFile();
            if (!peaksFileExists)
            {
                ReportGeneralProgress("Creating extracted ion chromatogram (XIC) source data... takes 1-5 minutes.. only needs to be done once.");

                CreatePeaksForChromSourceData();
                ReportGeneralProgress("Done creating XIC source data.");
            }


            string baseFileName;
            baseFileName = this.Run.DataSetPath + "\\" + this.Run.DatasetName;

            string expectedPeaksFilename = baseFileName + "_peaks.txt";

            if (File.Exists(expectedPeaksFilename))
            {

                PeakImporterFromText peakImporter = new PeakImporterFromText(expectedPeaksFilename, _backgroundWorker);

                peakImporter.ImportPeaks(this.Run.ResultCollection.MSPeakResultList);
            }
            else
            {
                ReportGeneralProgress(DateTime.Now + "\tCRITICAL FAILURE. Chrom source data (_peaks.txt) file not loaded.");
                return;
            }


            ReportGeneralProgress(DateTime.Now + "\tPeak Loading complete.");
            return;



        }

        #endregion

        #region Private Methods
        private void ReportGeneralProgress(string generalProgressString, int progressPercent = 0)
        {
            if (_backgroundWorker == null)
            {
                Console.WriteLine(DateTime.Now + "\t" + generalProgressString);
            }
            else
            {
                _progressInfo.ProgressInfoString = generalProgressString;
                _progressInfo.IsGeneralProgress = true;
                _backgroundWorker.ReportProgress(progressPercent, _progressInfo);
            }


        }


        private bool checkForPeaksFile()
        {
            string baseFileName;
            baseFileName = this.Run.DataSetPath + "\\" + this.Run.DatasetName;

            string possibleFilename1 = baseFileName + "_peaks.txt";

            if (File.Exists(possibleFilename1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        private void CreatePeaksForChromSourceData()
        {
            PeakDetectAndExportWorkflowParameters parameters = new PeakDetectAndExportWorkflowParameters();
            TargetedWorkflowParameters deconParam = (TargetedWorkflowParameters)this._workflowParameters;

            parameters.PeakBR = deconParam.ChromGenSourceDataPeakBR;
            parameters.PeakFitType = DeconTools.Backend.Globals.PeakFitType.QUADRATIC;
            parameters.SigNoiseThreshold = deconParam.ChromGenSourceDataSigNoise;
            PeakDetectAndExportWorkflow peakCreator = new PeakDetectAndExportWorkflow(this.Run, parameters, _backgroundWorker);
            peakCreator.Execute();
        }
        #endregion

    }
}
