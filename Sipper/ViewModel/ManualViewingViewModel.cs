using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using Sipper.Model;

namespace Sipper.ViewModel
{
    public delegate void AllDataLoadedAndReadyEventHandler(object sender, EventArgs e);

    public class ManualViewingViewModel : ViewModelBase
    {
        private const double DefaultMSPeakWidth = 0.01;



        private TargetedResultRepository _resultRepositorySource;
        private BackgroundWorker _backgroundWorker;




        #region Constructors


        public ManualViewingViewModel(FileInputsInfo fileInputs = null)
        {
            Results = new ObservableCollection<SipperLcmsFeatureTargetedResultDTO>();
            WorkflowParameters = new SipperTargetedWorkflowParameters();

            Workflow = new SipperTargetedWorkflow(WorkflowParameters);
            FileInputs = new FileInputsViewModel(fileInputs);

            FileInputs.PropertyChanged += FileInputsPropertyChanged;


            LoadParameters();



            UpdateGraphRelatedProperties();

            ChromGraphXWindowWidth = 600;
        }

        public ManualViewingViewModel(TargetedResultRepository resultRepository, FileInputsInfo fileInputs = null)
            : this(fileInputs)
        {
            _resultRepositorySource = resultRepository;
            SetResults();

            if (IsAllDataReady)
            {
                OnAllDataLoadedAndReady(new EventArgs());
            }

        }




        #endregion

        #region Event-related

        public event AllDataLoadedAndReadyEventHandler AllDataLoadedAndReadyEvent;

        public void OnAllDataLoadedAndReady(EventArgs e)
        {
            AllDataLoadedAndReadyEventHandler handler = AllDataLoadedAndReadyEvent;
            if (handler != null) handler(this, e);
        }


        void FileInputsPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "ParameterFilePath":
                    LoadParameters();
                    break;
                case "DatasetPath":
                    LoadRun(FileInputs.DatasetPath);
                    break;
                case "TargetsFilePath":
                    LoadResults(FileInputs.TargetsFilePath);
                    break;
            }

            if (IsAllDataReady)
            {
                OnAllDataLoadedAndReady(new EventArgs());
            }




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
                OnPropertyChanged("DatasetFilePath");
                OnPropertyChanged("RunStatusText");

                if (IsAllDataReady)
                {
                    OnAllDataLoadedAndReady(new EventArgs());
                }
            }
        }

        private string _runStatusText;
        public string RunStatusText
        {
            get
            {
                if (Run == null)
                {
                    return "Not loaded.";
                }
                else
                {
                    return "LOADED.";
                }
            }
        }


        public FileInputsViewModel FileInputs { get; private set; }



        public string DatasetFilePath
        {
            get
            {
                return FileInputs.DatasetPath;
            }

            set
            {
                FileInputs.DatasetPath = value;
            }

        }

        public ObservableCollection<SipperLcmsFeatureTargetedResultDTO> Results { get; set; }

        private SipperLcmsFeatureTargetedResultDTO _currentResult;
        public SipperLcmsFeatureTargetedResultDTO CurrentResult
        {
            get { return _currentResult; }
            set
            {
                _currentResult = value;
                OnPropertyChanged("CurrentResult");

            }
        }


        public string ChromTitleText
        {
            get
            {
                if (CurrentResult == null) return String.Empty;

                return "XIC m/z " + CurrentResult.MonoMZ.ToString("0.0000");

            }
        }




        private string _targetsFileStatusText;
        public string TargetsFileStatusText
        {
            get { return _targetsFileStatusText; }
            set
            {
                if (value == TargetsFileStatusText) return;
                _targetsFileStatusText = value;
                OnPropertyChanged("TargetsFileStatusText");
            }
        }


        private string _parameterFileStatusText;
        public string ParameterFileStatusText
        {
            get { return _parameterFileStatusText; }
            set
            {
                if (value == ParameterFileStatusText) return;
                _parameterFileStatusText = value;
                OnPropertyChanged("ParameterFileStatusText");

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
                OnPropertyChanged("Workflow");
            }
        }

        private string _generalStatusMessage;
        public string GeneralStatusMessage
        {
            get { return _generalStatusMessage; }
            set
            {
                _generalStatusMessage = value;
                { OnPropertyChanged("GeneralStatusMessage"); }

            }
        }


        private string _peptideSequence;
        public string PeptideSequence
        {
            get
            {
                if (Workflow != null && Workflow.Result != null)
                {
                    return Workflow.Result.Target.Code;
                }
                return String.Empty;

            }




        }


        private XYData _chromXYData;
        public XYData ChromXYData
        {
            get { return _chromXYData; }
            set
            {
                _chromXYData = value;
                OnPropertyChanged("ChromXYData");
            }
        }

        private XYData _massSpecXYData;
        public XYData MassSpecXYData
        {
            get { return _massSpecXYData; }
            set
            {
                _massSpecXYData = value;
                OnPropertyChanged("MassSpecXYData");
            }
        }


        private XYData _chromCorrXYData;
        public XYData ChromCorrXYData
        {
            get { return _chromCorrXYData; }
            set { _chromCorrXYData = value; }
        }


        public double ChromGraphMaxX { get; set; }

        public double ChromGraphMinX { get; set; }

        public double ChromGraphXWindowWidth { get; set; }



        public double MSGraphMaxX { get; set; }

        public double MSGraphMinX { get; set; }






        private XYData _theorProfileXYData;
        public XYData TheorProfileXYData
        {
            get { return _theorProfileXYData; }
            set
            {
                _theorProfileXYData = value;
                OnPropertyChanged("TheorProfileXYData");
            }
        }

        #endregion



        #region Public Methods

        public void ExecuteWorkflow()
        {
            GeneralStatusMessage = ".......";

            SetCurrentWorkflowTarget(CurrentResult);

            Workflow.Execute();

            UpdateGraphRelatedProperties();

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

            GeneralStatusMessage = "Updated.";

            OnPropertyChanged("PeptideSequence");
        }

        public void LoadRun(string fileOrFolderPath)
        {
            if (Run != null)
            {
                Run.Close();
                Run = null;
                GC.Collect();
            }

            try
            {

                Run = new RunFactory().CreateRun(fileOrFolderPath);
            }
            catch (Exception ex)
            {


                GeneralStatusMessage = ex.Message;
            }

            OnPropertyChanged("RunStatusText");
            OnPropertyChanged("DatasetFilePath");

            if (Run != null)
            {
                try
                {
                    LoadPeaks();
                }
                catch (Exception ex)
                {
                    GeneralStatusMessage = ex.Message;

                }

            }


        }

        private void LoadPeaks()
        {
            if (Run == null) return;

            string expectedPeaksFilename = this.Run.DataSetPath + "\\" + this.Run.DatasetName + "_peaks.txt";

            if (!File.Exists(expectedPeaksFilename))
            {
                throw new FileNotFoundException("The _peaks.txt file for the raw dataset could not be found. This can be created using the Autoprocessor.");
            }

            PeakImporterFromText peakImporter = new PeakImporterFromText(expectedPeaksFilename, _backgroundWorker);
            peakImporter.ImportPeaks(this.Run.ResultCollection.MSPeakResultList);

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

        public void LoadParameters()
        {
            IsParametersLoaded = false;

            if (String.IsNullOrEmpty(FileInputs.ParameterFilePath))
            {
                ParameterFileStatusText = "None loaded; using defaults";
            }
            else
            {
                FileInfo fileInfo = new FileInfo(FileInputs.ParameterFilePath);

                if (fileInfo.Exists)
                {
                    WorkflowParameters.LoadParameters(FileInputs.ParameterFilePath);
                    ParameterFileStatusText = fileInfo.Name + " LOADED";

                    IsParametersLoaded = true;
                }
                else
                {
                    WorkflowParameters = new SipperTargetedWorkflowParameters();
                    ParameterFileStatusText = "None loaded; using defaults";
                }

               
            }




        }


        protected bool IsParametersLoaded { get; set; }
        protected bool IsRunLoaded
        {
            get { return Run != null; }
        }

        protected bool IsResultsLoaded
        {
            get { return Results != null && Results.Count > 0; }
        }

        public bool IsAllDataReady
        {
            get { return (IsParametersLoaded && IsResultsLoaded && IsRunLoaded); }
        }


        public void LoadResults(string resultFile)
        {

            _resultRepositorySource.Results.Clear();

            FileInfo fileInfo = new FileInfo(resultFile);

            if (fileInfo.Exists)
            {
                SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFile);
                var tempResults = importer.Import();

                _resultRepositorySource.Results.AddRange(tempResults.Results);
            }

            SetResults();


        }

        public void SetResults()
        {


            var query = (from n in _resultRepositorySource.Results select (SipperLcmsFeatureTargetedResultDTO)n);

            Results.Clear();
            foreach (var resultDto in query)
            {
                Results.Add(resultDto);
            }

            TargetsFileStatusText = Results.Count + " loaded.";


        }

        public void SaveResults()
        {
            try
            {
                var exporter = new SipperResultToLcmsFeatureExporter(FileInputs.ResultsSaveFilePath);
                exporter.ExportResults(Results);
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = "Error saving results. Error message: " + ex.Message;
                throw;
            }

            GeneralStatusMessage = "Results saved to: " + Path.GetFileName(FileInputs.ResultsSaveFilePath);


        }



        public void CopyMSDataToClipboard()
        {
            if (MassSpecXYData == null || MassSpecXYData.Xvalues == null || MassSpecXYData.Xvalues.Length == 0) return;
            CopyXYDataToClipboard(MassSpecXYData.Xvalues, MassSpecXYData.Yvalues);
        }

        public void CopyChromatogramToClipboard()
        {
            if (ChromXYData == null || ChromXYData.Xvalues == null || ChromXYData.Xvalues.Length == 0) return;
            CopyXYDataToClipboard(ChromXYData.Xvalues, ChromXYData.Yvalues);
        }

        public void CopyTheorMSToClipboard()
        {
            if (TheorProfileXYData == null || TheorProfileXYData.Xvalues == null || TheorProfileXYData.Xvalues.Length == 0) return;
            CopyXYDataToClipboard(TheorProfileXYData.Xvalues, TheorProfileXYData.Yvalues);
        }



        #endregion

        #region Private Methods

        private void UpdateGraphRelatedProperties()
        {
            ChromXYData = new XYData();
            ChromXYData.Xvalues = Workflow.ChromatogramXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.ChromatogramXYData.Xvalues;
            ChromXYData.Yvalues = Workflow.ChromatogramXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.ChromatogramXYData.Yvalues;

            MassSpecXYData = new XYData();
            MassSpecXYData.Xvalues = Workflow.MassSpectrumXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.MassSpectrumXYData.Xvalues;
            MassSpecXYData.Yvalues = Workflow.MassSpectrumXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.MassSpectrumXYData.Yvalues;

            ChromCorrXYData = new XYData();
            ChromCorrXYData.Xvalues = Workflow.ChromCorrelationRSquaredVals == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.ChromCorrelationRSquaredVals.Xvalues;
            ChromCorrXYData.Yvalues = Workflow.ChromCorrelationRSquaredVals == null ? new double[] { 0, 0, 0, 0, 0 } : Workflow.ChromCorrelationRSquaredVals.Yvalues;


            if (CurrentResult != null)
            {
                MSGraphMinX = CurrentResult.MonoMZ - 1.75;
                MSGraphMaxX = CurrentResult.MonoMZ + 7;
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



        private void CopyXYDataToClipboard(double[] xvals, double[] yvals)
        {
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();

            int maxLength = 0;
            if (xvals.Length == 0 || yvals.Length == 0) return;

            if (xvals.Length >= yvals.Length) maxLength = yvals.Length;
            else maxLength = xvals.Length;

            for (int i = 0; i < maxLength; i++)
            {
                stringBuilder.Append(xvals[i]);
                stringBuilder.Append("\t");
                stringBuilder.Append(yvals[i]);
                stringBuilder.Append(Environment.NewLine);
            }

            if (stringBuilder.ToString().Length == 0) return;

            Clipboard.SetText(stringBuilder.ToString());
            GeneralStatusMessage = "Data copied to clipboard";

        }


        #endregion

    }
}
