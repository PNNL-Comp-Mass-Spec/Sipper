using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.Backend.Workflows;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Sipper.Model;
using Globals = DeconTools.Backend.Globals;

namespace Sipper.ViewModel
{
    public delegate void AllDataLoadedAndReadyEventHandler(object sender, EventArgs e);

    public class ViewAndAnnotateViewModel : ViewModelBase
    {
        private const double DefaultMsPeakWidth = 0.01;
        private readonly ScanSetFactory _scanSetFactory = new ScanSetFactory();
        private int _currentLcScan;
        private readonly TargetedResultRepository _resultRepositorySource;
        private BackgroundWorker _backgroundWorker;
        private string _peaksFilename;
        private MSGenerator _msGenerator;

        #region Constructors

        public ViewAndAnnotateViewModel()
        {
            FileInputs = new FileInputsViewModel();
            _resultRepositorySource = new TargetedResultRepository();
            Results = new ObservableCollection<SipperLcmsFeatureTargetedResultDTO>();
            var workflowParameters = new SipperTargetedWorkflowParameters();
            Workflow = new SipperTargetedWorkflow(workflowParameters);
            ChromGraphXWindowWidth = 600;
            MsGraphMinX = 400;
            MsGraphMaxX = 1400;

            ShowFileAndResultsList = true;
            MassSpecVisibleWindowWidth = 15;
        }

        public ViewAndAnnotateViewModel(FileInputsInfo fileInputs)
            : this()
        {
            FileInputs = new FileInputsViewModel(fileInputs);
            FileInputs.PropertyChanged += FileInputsPropertyChanged;

            LoadParameters();
            UpdateGraphRelatedProperties();
        }

        public ViewAndAnnotateViewModel(TargetedResultRepository resultRepository, FileInputsInfo fileInputs = null)
            : this(fileInputs)
        {
            _resultRepositorySource = resultRepository;
            SetResults(_resultRepositorySource.Results);

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
            var handler = AllDataLoadedAndReadyEvent;
            handler?.Invoke(this, e);
        }

        private void OnYAxisChange(object sender, AxisChangedEventArgs e)
        {
            if (!(sender is LinearAxis yAxis))
                return;

            // No need to update anything if the minimum is already <= 0
            if (yAxis.ActualMinimum <= 0) return;

            // Set the minimum to 0 and refresh the plot
            yAxis.Zoom(0, yAxis.ActualMaximum);
            yAxis.PlotModel.InvalidatePlot(true);
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
            get => _run;
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

        public string RunStatusText
        {
            get
            {
                if (Run == null)
                {
                    return "Not loaded.";
                }

                return "LOADED.";
            }
        }

        private double _massSpecVisibleWindowWidth;
        public double MassSpecVisibleWindowWidth
        {
            get => _massSpecVisibleWindowWidth;
            set
            {
                _massSpecVisibleWindowWidth = value;
                OnPropertyChanged("MassSpecVisibleWindowWidth");
            }
        }

        public FileInputsViewModel FileInputs { get; }

        public ValidationCode CurrentResultValidationCode
        {
            get
            {
                if (_currentResult == null)
                {
                    return ValidationCode.None;
                }

                return _currentResult.ValidationCode;
            }
            set
            {
                if (_currentResult == null)
                {
                    return;
                }

                _currentResult.ValidationCode = value;
                OnPropertyChanged("CurrentResultValidationCode");
            }
        }

        public string DatasetFilePath
        {
            get => FileInputs.DatasetPath;

            set => FileInputs.DatasetPath = value;
        }

        public ObservableCollection<SipperLcmsFeatureTargetedResultDTO> Results { get; set; }

        private PlotModel _theorIsoPlot;
        public PlotModel TheorIsoPlot
        {
            get => _theorIsoPlot;
            set
            {
                _theorIsoPlot = value;
                OnPropertyChanged("TheorIsoPlot");
            }
        }

        private PlotModel _observedIsoPlot;
        public PlotModel ObservedIsoPlot
        {
            get => _observedIsoPlot;
            set
            {
                _observedIsoPlot = value;
                OnPropertyChanged("ObservedIsoPlot");
            }
        }

        private PlotModel _chromatogramPlot;
        public PlotModel ChromatogramPlot
        {
            get => _chromatogramPlot;
            set
            {
                _chromatogramPlot = value;
                OnPropertyChanged("ChromatogramPlot");
            }
        }

        private PlotModel _chromCorrelationPlot;
        public PlotModel ChromCorrelationPlot
        {
            get => _chromCorrelationPlot;
            set
            {
                _chromCorrelationPlot = value;
                OnPropertyChanged("ChromCorrelationPlot");
            }
        }

        private SipperLcmsFeatureTargetedResultDTO _currentResult;
        public SipperLcmsFeatureTargetedResultDTO CurrentResult
        {
            get => _currentResult;
            set
            {
                //check if we moved on to a different dataset

                _currentResult = value;
                OnPropertyChanged("CurrentResult");
                OnPropertyChanged("CurrentResultValidationCode");
            }
        }

        public string ChromTitleText
        {
            get
            {
                if (CurrentResult == null) return string.Empty;

                return "XIC m/z " + CurrentResult.MonoMZ.ToString("0.0000");
            }
        }

        private string _targetsFileStatusText;
        public string TargetsFileStatusText
        {
            get => _targetsFileStatusText;
            set
            {
                if (value == TargetsFileStatusText) return;
                _targetsFileStatusText = value;
                OnPropertyChanged("TargetsFileStatusText");
            }
        }

        private string _targetFilterString;
        public string TargetFilterString
        {
            get => _targetFilterString;
            set
            {
                _targetFilterString = value;

                FilterTargets();
                OnPropertyChanged("TargetFilterString");
            }
        }

        private string _parameterFileStatusText;
        public string ParameterFileStatusText
        {
            get => _parameterFileStatusText;
            set
            {
                if (value == ParameterFileStatusText) return;
                _parameterFileStatusText = value;
                OnPropertyChanged("ParameterFileStatusText");
            }
        }

        private SipperTargetedWorkflow _workflow;
        public SipperTargetedWorkflow Workflow
        {
            get => _workflow;
            set
            {
                _workflow = value;
                OnPropertyChanged("Workflow");
            }
        }

        private string _generalStatusMessage;
        public string GeneralStatusMessage
        {
            get => _generalStatusMessage;
            set
            {
                _generalStatusMessage = value;
                { OnPropertyChanged("GeneralStatusMessage"); }
            }
        }

        public string WorkflowStatusMessage
        {
            get
            {
                if (Workflow != null)
                {
                    return Workflow.WorkflowStatusMessage;
                }
                return string.Empty;
            }
        }

        public string PeptideSequence
        {
            get
            {
                if (Workflow?.Result != null)
                {
                    return Workflow.Result.Target.Code;
                }
                return string.Empty;
            }
        }

        private XYData _chromXyData;
        public XYData ChromXyData
        {
            get => _chromXyData;
            set
            {
                _chromXyData = value;
                OnPropertyChanged("ChromXyData");
            }
        }

        private XYData _massSpecXyData;
        public XYData MassSpecXyData
        {
            get => _massSpecXyData;
            set
            {
                _massSpecXyData = value;
                OnPropertyChanged("MassSpecXyData");
            }
        }

        public XYData SubtractedMassSpecXYData { get; set; }

        public XYData ChromCorrXYData { get; set; }

        public XYData RatioLogsXyData { get; set; }

        public XYData RatioXyData { get; set; }

        public double ChromGraphMaxX { get; set; }

        public double ChromGraphMinX { get; set; }

        public double ChromGraphXWindowWidth { get; set; }

        private double _msGraphMaxX;
        public double MsGraphMaxX
        {
            get => _msGraphMaxX;
            set
            {
                _msGraphMaxX = value;
                OnPropertyChanged("MsGraphMaxX");
            }
        }

        private double _msGraphMinX;
        public double MsGraphMinX
        {
            get => _msGraphMinX;
            set
            {
                _msGraphMinX = value;
                OnPropertyChanged("MsGraphMinX");
            }
        }

        public float MsGraphMaxY { get; set; }

        public int MinLcScan
        {
            get
            {
                if (Run == null) return 1;
                return Run.MinLCScan;
            }
        }

        public int MaxLcScan
        {
            get
            {
                if (Run == null) return 1;
                return Run.MaxLCScan;
            }
        }

        public int CurrentLcScan
        {
            get => _currentLcScan;
            set
            {
                _currentLcScan = value;
                OnPropertyChanged("CurrentLcScan");
            }
        }

        public XYData LabelDistributionXyData { get; set; }

        private XYData _theorProfileXyData;
        public XYData TheorProfileXyData
        {
            get => _theorProfileXyData;
            set
            {
                _theorProfileXyData = value;
                OnPropertyChanged("TheorProfileXyData");
            }
        }

        private int _percentProgress;
        /// <summary>
        /// Data for peak loading progress bar
        /// </summary>
        public int PercentProgress
        {
            get => _percentProgress;
            set
            {
                _percentProgress = value;
                OnPropertyChanged("PercentProgress");
            }
        }

        private bool _showFileAndResultsList;
        public bool ShowFileAndResultsList
        {
            get => _showFileAndResultsList;
            set
            {
                _showFileAndResultsList = value;
                OnPropertyChanged("ShowFileAndResultsList");
            }
        }

        protected bool IsParametersLoaded { get; set; }

        protected bool IsRunLoaded => Run?.PeakList != null && Run.PeakList.Count > 0;

        protected bool IsResultsLoaded => Results != null && Results.Count > 0;

        public bool IsAllDataReady => (IsParametersLoaded && IsResultsLoaded && IsRunLoaded);

        #endregion

        #region Public Methods

        public void NavigateToNextMs1MassSpectrum(Globals.ScanSelectionMode selectionMode = Globals.ScanSelectionMode.ASCENDING)
        {
            if (Run == null) return;

            if (Workflow == null) return;

            var workflowParameters = (TargetedWorkflowParameters)Workflow.WorkflowParameters;

            int nextPossibleMs;
            if (selectionMode == Globals.ScanSelectionMode.DESCENDING)
            {
                nextPossibleMs = CurrentLcScan - 1;
            }
            else
            {
                nextPossibleMs = CurrentLcScan + 1;
            }

            CurrentLcScan = Run.GetClosestMSScan(nextPossibleMs, selectionMode);

            if (_msGenerator == null)
            {
                _msGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
            }

            var currentScanSet = _scanSetFactory.CreateScanSet(Run, CurrentLcScan, workflowParameters.NumMSScansToSum);
            MassSpecXyData = _msGenerator.GenerateMS(Run, currentScanSet);

            MassSpecXyData = MassSpecXyData?.TrimData(MsGraphMinX - 20, MsGraphMaxX + 20);

            CreateMsPlotForScanByScanAnalysis(currentScanSet);
        }

        public void ExecuteWorkflow()
        {
            if (Run == null) return;

            GeneralStatusMessage = ".......";

            SetCurrentWorkflowTarget(CurrentResult);

            Workflow.Execute();

            MsGraphMinX = Workflow.Result.Target.MZ - 1.75;
            MsGraphMaxX = Workflow.Result.Target.MZ + MassSpecVisibleWindowWidth;

            CreateChromatogramPlot();
            CreateTheorIsotopicProfilePlot();
            CreateChromCorrPlot();
            CreateObservedIsotopicProfilePlot();

            SetupEventHandlersForObsAndTheor();

            if (Workflow.Success)
            {
                var workflowParameters = (TargetedWorkflowParameters)Workflow.WorkflowParameters;
                CurrentLcScan = Workflow.Result.GetScanNum();
            }

            //UpdateGraphRelatedProperties();
            OnPropertyChanged("WorkflowStatusMessage");
            OnPropertyChanged("PeptideSequence");
        }

        private void SetupEventHandlersForObsAndTheor()
        {
            var isInternalChange = false;

            var observedXAxis = ObservedIsoPlot.Axes[0];
            var theoreticalXAxis = TheorIsoPlot.Axes[0];

            theoreticalXAxis.AxisChanged += (s, e) =>
                {
                    if (isInternalChange)
                    {
                        return;
                    }
                    isInternalChange = true;
                    observedXAxis.Zoom(theoreticalXAxis.ActualMinimum,
                                       theoreticalXAxis.ActualMaximum);
                    ObservedIsoPlot.InvalidatePlot(false);
                    isInternalChange = false;
                };

            observedXAxis.AxisChanged += (s, e) =>
                {
                    if (isInternalChange)
                    {
                        return;
                    }
                    isInternalChange = true;
                    theoreticalXAxis.Zoom(observedXAxis.ActualMinimum, observedXAxis.ActualMaximum);
                    TheorIsoPlot.InvalidatePlot(false);
                    isInternalChange = false;
                };
        }

        public void LoadRun(string fileOrDirectoryPath)
        {
            if (Run != null)
            {
                Run.Close();
                Run = null;
                GC.Collect();
            }

            try
            {
                Run = new RunFactory().CreateRun(fileOrDirectoryPath);
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = ex.Message;
            }

            OnPropertyChanged("RunStatusText");
            OnPropertyChanged("DatasetFilePath");

            if (Run != null)
            {
                LoadPeaksUsingBackgroundWorker();
                FileInputs.DatasetParentFolder = Run.DatasetDirectoryPath;
            }
        }

        private void LoadPeaksUsingBackgroundWorker()
        {
            if (Run == null) return;

            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                GeneralStatusMessage = "Patience please. Already busy...";
                return;
            }

            _backgroundWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };
            _backgroundWorker.RunWorkerCompleted += BackgroundWorkerCompleted;
            _backgroundWorker.ProgressChanged += BackgroundWorkerProgressChanged;
            _backgroundWorker.DoWork += BackgroundWorkerDoWork;

            _backgroundWorker.RunWorkerAsync();
        }

        private void LoadPeaks()
        {
            try
            {
                _peaksFilename = Run.DatasetDirectoryPath + "\\" + Run.DatasetName + "_peaks.txt";

                if (!File.Exists(_peaksFilename))
                {
                    GeneralStatusMessage =
                        "Creating chromatogram data (_peaks.txt file); this is only done once. It takes 1 - 5 min .......";
                    var deconParam = (TargetedWorkflowParameters)Workflow.WorkflowParameters;

                    var peakCreationParameters = new PeakDetectAndExportWorkflowParameters
                    {
                        PeakBR = deconParam.ChromGenSourceDataPeakBR,
                        PeakFitType = Globals.PeakFitType.QUADRATIC,
                        SigNoiseThreshold = deconParam.ChromGenSourceDataSigNoise
                    };

                    var peakCreator = new PeakDetectAndExportWorkflow(Run, peakCreationParameters, _backgroundWorker);
                    peakCreator.Execute();
                }
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = ex.Message;
                return;
            }

            GeneralStatusMessage = "Loading chromatogram data (_peaks.txt file) .......";
            try
            {
                var peakImporter = new PeakImporterFromText(_peaksFilename, _backgroundWorker);
                peakImporter.ImportPeaks(Run.ResultCollection.MSPeakResultList);
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = ex.Message;
                return;
                //throw new ApplicationException("Peaks failed to load. Maybe the details below will help... \n\n" + ex.Message + "\nStacktrace: " + ex.StackTrace, ex);
            }

            if (Run.ResultCollection.MSPeakResultList != null && Run.ResultCollection.MSPeakResultList.Count > 0)
            {
                var numPeaksLoaded = Run.ResultCollection.MSPeakResultList.Count;
                GeneralStatusMessage = "Chromatogram data LOADED. (# peaks= " + numPeaksLoaded + ")";
            }
            else
            {
                GeneralStatusMessage = "No Chromatogram data!!! Check your _peaks.txt file for correct format.";
            }
        }

        void BackgroundWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            LoadPeaks();

            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }
        }

        private void BackgroundWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                GeneralStatusMessage = "Cancelled";
            }
            else if (e.Error != null)
            {
                GeneralStatusMessage = "Error loading peaks. Contact a good friend.";
            }
            else
            {
                PercentProgress = 100;
            }
        }

        private void BackgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PercentProgress = e.ProgressPercentage;
        }

        public void LoadParameters()
        {
            IsParametersLoaded = false;

            if (string.IsNullOrEmpty(FileInputs.ParameterFilePath))
            {
                ParameterFileStatusText = "None loaded; using defaults";

                Workflow.WorkflowParameters = new SipperTargetedWorkflowParameters();
            }
            else
            {
                var fileInfo = new FileInfo(FileInputs.ParameterFilePath);

                if (fileInfo.Exists)
                {
                    Workflow.WorkflowParameters.LoadParameters(FileInputs.ParameterFilePath);
                    ParameterFileStatusText = fileInfo.Name + " LOADED";

                    IsParametersLoaded = true;
                }
                else
                {
                    Workflow.WorkflowParameters = new SipperTargetedWorkflowParameters();
                    ParameterFileStatusText = "None loaded; using defaults";
                }
            }

            Workflow.IsWorkflowInitialized = false;    //important... forces workflow to be reinitialized with new parameters
        }

        public void LoadResults(string resultFile)
        {

            _resultRepositorySource.Results.Clear();

            var fileInfo = new FileInfo(resultFile);

            if (fileInfo.Exists)
            {
                var importer = new SipperResultFromTextImporter(resultFile);
                var tempResults = importer.Import();

                _resultRepositorySource.Results.AddRange(tempResults.Results);
            }

            SetResults(_resultRepositorySource.Results);
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

        public void CopyMsDataToClipboard()
        {
            if (MassSpecXyData?.Xvalues == null || MassSpecXyData.Xvalues.Length == 0) return;
            CopyXyDataToClipboard(MassSpecXyData.Xvalues, MassSpecXyData.Yvalues);
        }

        public void CopyChromatogramToClipboard()
        {
            if (ChromXyData?.Xvalues == null || ChromXyData.Xvalues.Length == 0) return;
            CopyXyDataToClipboard(ChromXyData.Xvalues, ChromXyData.Yvalues);
        }

        public void CopyTheorMsToClipboard()
        {
            if (TheorProfileXyData?.Xvalues == null || TheorProfileXyData.Xvalues.Length == 0) return;
            CopyXyDataToClipboard(TheorProfileXyData.Xvalues, TheorProfileXyData.Yvalues);
        }

        #endregion

        #region Private Methods

        private void CreateChromatogramPlot()
        {
            var centerScan = Workflow.Result.Target.ScanLCTarget;
            ChromGraphMinX = centerScan - ChromGraphXWindowWidth / 2;
            ChromGraphMaxX = centerScan + ChromGraphXWindowWidth / 2;

            var xyData = new XYData();
            if (Workflow.ChromatogramXYData == null)
            {
                xyData.Xvalues = Workflow.ChromatogramXYData == null ? new double[] { 1, Run.MaxLCScan } : Workflow.ChromatogramXYData.Xvalues;
                xyData.Yvalues = Workflow.ChromatogramXYData == null ? new double[] { 0, 0 } : Workflow.ChromatogramXYData.Yvalues;
            }
            else
            {
                xyData.Xvalues = Workflow.ChromatogramXYData.Xvalues;
                xyData.Yvalues = Workflow.ChromatogramXYData.Yvalues;
            }

            var graphTitle = "TargetID=" + Workflow.Result.Target.ID + "; m/z " +
                                  Workflow.Result.Target.MZ.ToString("0.0000") + "; z=" +
                                  Workflow.Result.Target.ChargeState;

            var plotModel = new PlotModel
            {
                Title = graphTitle,
                TitleFontSize = 11,
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0),
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var series = new LineSeries
            {
                MarkerSize = 1,
                Color = OxyColors.Black
            };

            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                series.Points.Add(new DataPoint(xyData.Xvalues[i], xyData.Yvalues[i]));
            }

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "scan",
                Minimum = ChromGraphMinX,
                Maximum = ChromGraphMaxX
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Intensity",
                Minimum = 0,
                AbsoluteMinimum = 0
            };

            var maxY = xyData.GetMaxY();
            yAxis.Maximum = maxY + maxY * 0.05;
            yAxis.AxisChanged += OnYAxisChange;

            xAxis.AxislineStyle = LineStyle.Solid;
            xAxis.AxislineThickness = 1;
            yAxis.AxislineStyle = LineStyle.Solid;
            yAxis.AxislineThickness = 1;

            plotModel.Series.Add(series);
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            ChromatogramPlot = plotModel;
        }

        private void CreateMsPlotForScanByScanAnalysis(ScanSet scanSet)
        {
            var xyData = new XYData
            {
                Xvalues = MassSpecXyData == null ? new double[] { 400, 1500 } : MassSpecXyData.Xvalues,
                Yvalues = MassSpecXyData == null ? new double[] { 0, 0 } : MassSpecXyData.Yvalues
            };

            var msGraphTitle = "Observed MS - Scan: " + scanSet;

            MsGraphMaxY = (float)xyData.GetMaxY(MsGraphMinX, MsGraphMaxX);

            var plotModel = new PlotModel
            {
                Title = msGraphTitle,
                TitleFontSize = 11,
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0),
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var series = new LineSeries
            {
                MarkerSize = 1,
                Color = OxyColors.Black
            };

            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                series.Points.Add(new DataPoint(xyData.Xvalues[i], xyData.Yvalues[i]));
            }

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "m/z",
                Minimum = MsGraphMinX,
                Maximum = MsGraphMaxX
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Intensity",
                Minimum = 0,
                AbsoluteMinimum = 0,
                Maximum = MsGraphMaxY + MsGraphMaxY * 0.05
            };

            //yAxis.Maximum = maxIntensity + (maxIntensity * .05);
            //yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
            yAxis.AxisChanged += OnYAxisChange;
            yAxis.StringFormat = "0.0E0";

            xAxis.AxislineStyle = LineStyle.Solid;
            xAxis.AxislineThickness = 1;
            yAxis.AxislineStyle = LineStyle.Solid;
            yAxis.AxislineThickness = 1;

            plotModel.Series.Add(series);

            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            ObservedIsoPlot = plotModel;
        }

        private void CreateObservedIsotopicProfilePlot()
        {
            XYData xyData;

            if (Workflow.MassSpectrumXYData == null)
            {
                xyData = new XYData
                {
                    Xvalues = Workflow.MassSpectrumXYData == null ? new double[] { 400, 1500 } : Workflow.MassSpectrumXYData.Xvalues,
                    Yvalues = Workflow.MassSpectrumXYData == null ? new double[] { 0, 0 } : Workflow.MassSpectrumXYData.Yvalues
                };
            }
            else
            {
                var xyDataSource = new XYData
                {
                    Xvalues = Workflow.MassSpectrumXYData.Xvalues,
                    Yvalues = Workflow.MassSpectrumXYData.Yvalues
                };

                xyData = xyDataSource.TrimData(Workflow.Result.Target.MZ - 100, Workflow.Result.Target.MZ + 100);
            }

            if (Workflow.Result.IsotopicProfile != null)
            {
                MsGraphMaxY = Workflow.Result.IsotopicProfile.getMostIntensePeak().Height;
            }
            else
            {
                MsGraphMaxY = (float)xyData.GetMaxY();
            }

            var msGraphTitle = Workflow.Result.Target.Code + "; m/z " +
                                  Workflow.Result.Target.MZ.ToString("0.0000") + "; z=" +
                                  Workflow.Result.Target.ChargeState;

            var plotModel = new PlotModel
            {
                Title = msGraphTitle,
                TitleFontSize = 11,
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0),
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var series = new LineSeries
            {
                MarkerSize = 1,
                Color = OxyColors.Black
            };

            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                series.Points.Add(new DataPoint(xyData.Xvalues[i], xyData.Yvalues[i]));
            }

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "m/z",
                Minimum = MsGraphMinX,
                Maximum = MsGraphMaxX
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Intensity",
                Minimum = 0,
                AbsoluteMinimum = 0,
                Maximum = MsGraphMaxY + MsGraphMaxY * 0.05,
                StringFormat = "0.0E0"
            };

            //yAxis.Maximum = maxIntensity + (maxIntensity * .05);
            //yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
            yAxis.AxisChanged += OnYAxisChange;

            xAxis.AxislineStyle = LineStyle.Solid;
            xAxis.AxislineThickness = 1;
            yAxis.AxislineStyle = LineStyle.Solid;
            yAxis.AxislineThickness = 1;

            plotModel.Series.Add(series);

            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            ObservedIsoPlot = plotModel;
        }

        private void CreateTheorIsotopicProfilePlot()
        {
            var theorProfileAligned = Workflow.Result.Target.IsotopicProfile.CloneIsotopicProfile();
            double fwhm;
            if (Workflow.Result.IsotopicProfile != null)
            {

                fwhm = Workflow.Result.IsotopicProfile.GetFWHM();
                IsotopicProfileUtilities.AlignTwoIsotopicProfiles(Workflow.Result.IsotopicProfile, theorProfileAligned);

                if (Workflow.SubtractedIso != null && Workflow.SubtractedIso.Peaklist.Count > 0)
                {
                    SubtractedMassSpecXYData = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(Workflow.SubtractedIso, fwhm);
                }
                else
                {
                    SubtractedMassSpecXYData = new XYData
                    {
                        Xvalues = new double[] { 400, 500, 600 },
                        Yvalues = new double[] { 0, 0, 0 }
                    };
                }
            }
            else
            {
                fwhm = DefaultMsPeakWidth;
            }

            TheorProfileXyData = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(Workflow.Result.Target.IsotopicProfile, fwhm);

            var xyData = new XYData
            {
                Xvalues = TheorProfileXyData.Xvalues,
                Yvalues = TheorProfileXyData.Yvalues
            };

            // Scale to 100;
            for (var i = 0; i < xyData.Yvalues.Length; i++)
            {
                xyData.Yvalues[i] = xyData.Yvalues[i] * 100;
            }

            var msGraphTitle = "Theoretical MS - m/z " +
                                  Workflow.Result.Target.MZ.ToString("0.0000") + "; z=" +
                                  Workflow.Result.Target.ChargeState;

            var plotModel = new PlotModel
            {
                Title = msGraphTitle,
                TitleFontSize = 11,
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(50, 0, 0, 0),
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var series = new LineSeries
            {
                MarkerSize = 1,
                Color = OxyColors.Black
            };

            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                series.Points.Add(new DataPoint(xyData.Xvalues[i], xyData.Yvalues[i]));
            }

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "m/z",
                Minimum = MsGraphMinX,
                Maximum = MsGraphMaxX
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Intensity",
                Minimum = 0,
                AbsoluteMinimum = 0,
                Maximum = 105,
                AbsoluteMaximum = 105,
                StringFormat = "0.0E0"
            };

            //yAxis.Maximum = maxIntensity + (maxIntensity * .05);
            //yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
            yAxis.AxisChanged += OnYAxisChange;

            xAxis.AxislineStyle = LineStyle.Solid;
            xAxis.AxislineThickness = 1;
            yAxis.AxislineStyle = LineStyle.Solid;
            yAxis.AxislineThickness = 1;

            plotModel.Series.Add(series);

            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            TheorIsoPlot = plotModel;
        }

        private void CreateChromCorrPlot()
        {
            ChromCorrXYData = new XYData
            {
                Xvalues = Workflow.ChromCorrelationRSquaredVals == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.ChromCorrelationRSquaredVals.Xvalues,
                Yvalues = Workflow.ChromCorrelationRSquaredVals == null ? new double[] { 0, 0, 0, 0, 0 } : Workflow.ChromCorrelationRSquaredVals.Yvalues
            };

            var xyData = new XYData
            {
                Xvalues = ChromCorrXYData.Xvalues,
                Yvalues = ChromCorrXYData.Yvalues
            };

            var graphTitle = "Isotope peak correlation data";
            var plotModel = new PlotModel
            {
                Title = graphTitle,
                TitleFontSize = 10,
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0),
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var series = new LineSeries
            {
                MarkerSize = 3,
                MarkerType = MarkerType.Square,
                MarkerStrokeThickness = 1,
                MarkerFill = OxyColors.DarkRed,
                MarkerStroke = OxyColors.Black,
                Color = OxyColors.Black
            };

            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                series.Points.Add(new DataPoint(xyData.Xvalues[i], xyData.Yvalues[i]));
            }

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "isotopic peak #"
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "correlation"
            };

            xAxis.AxislineStyle = LineStyle.Solid;
            xAxis.AxislineThickness = 1;
            yAxis.AxislineStyle = LineStyle.Solid;
            yAxis.AxislineThickness = 1;

            xAxis.FontSize = 8;
            xAxis.MajorStep = 1;
            xAxis.MinorTickSize = 0;
            xAxis.MinorStep = 1;
            yAxis.FontSize = 8;

            yAxis.Minimum = 0;
            yAxis.AbsoluteMinimum = 0;
            yAxis.Maximum = 1.02;
            yAxis.AbsoluteMaximum = 1.02;
            yAxis.AxisChanged += OnYAxisChange;

            plotModel.Series.Add(series);
            plotModel.Axes.Add(yAxis);
            plotModel.Axes.Add(xAxis);

            ChromCorrelationPlot = plotModel;
        }

        private void UpdateGraphRelatedProperties()
        {
            ChromXyData = new XYData
            {
                Xvalues = Workflow.ChromatogramXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.ChromatogramXYData.Xvalues,
                Yvalues = Workflow.ChromatogramXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.ChromatogramXYData.Yvalues
            };

            //MassSpecXYData = new XYData();
            //MassSpecXYData.Xvalues = Workflow.MassSpectrumXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.MassSpectrumXYData.Xvalues;
            //MassSpecXYData.Yvalues = Workflow.MassSpectrumXYData == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.MassSpectrumXYData.Yvalues;

            RatioXyData = new XYData
            {
                Xvalues = Workflow.RatioVals == null ? new double[] { 0, 1, 2, 3, 4 } : Workflow.RatioVals.Xvalues,
                Yvalues = Workflow.RatioVals == null ? new double[] { 0, 0, 0, 0, 0 } : Workflow.RatioVals.Yvalues
            };

            RatioLogsXyData = new XYData
            {
                Xvalues = new double[] { 0, 1, 2, 3, 4 },
                Yvalues = new double[] { 0, 0, 0, 0, 0 }
            };

            LabelDistributionXyData = new XYData();
            if (CurrentResult?.LabelDistributionVals != null && CurrentResult.LabelDistributionVals.Length > 0)
            {
                //var xValues = ratioData.PeakList.Select((p, i) => new { peak = p, index = i }).Select(n => (double)n.index).ToList();

                LabelDistributionXyData.Xvalues = CurrentResult.LabelDistributionVals
                    .Select((value, index) => new { index })
                    .Select(n => (double)n.index).ToArray();
                LabelDistributionXyData.Yvalues = CurrentResult.LabelDistributionVals;
            }
            else
            {
                LabelDistributionXyData.Xvalues = new double[] { 0, 1, 2, 3 };
                LabelDistributionXyData.Yvalues = new double[] { 0, 0, 0, 0 };
            }

            //if (CurrentResultInfo != null)
            //{
            //    MSGraphMinX = CurrentResultInfo.MonoMZ - 1.75;
            //    MSGraphMaxX = CurrentResultInfo.MonoMZ + MassSpecVisibleWindowWidth;
            //}
            //else
            //{
            //    MSGraphMinX = MassSpecXYData.Xvalues.Min();
            //    MSGraphMaxX = MassSpecXYData.Xvalues.Max();
            //}
        }

        private void SetCurrentWorkflowTarget(TargetedResultDTO result)
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

        private void CopyXyDataToClipboard(IReadOnlyList<double> xValues, IReadOnlyList<double> yValues)
        {
            var stringBuilder = new StringBuilder();

            if (xValues.Count == 0 || yValues.Count == 0)
                return;

            int maxLength;
            if (xValues.Count >= yValues.Count)
                maxLength = yValues.Count;
            else
                maxLength = xValues.Count;

            for (var i = 0; i < maxLength; i++)
            {
                stringBuilder.Append(xValues[i]);
                stringBuilder.Append("\t");
                stringBuilder.Append(yValues[i]);
                stringBuilder.Append(Environment.NewLine);
            }

            if (stringBuilder.ToString().Length == 0) return;

            Clipboard.SetText(stringBuilder.ToString());
            GeneralStatusMessage = "Data copied to clipboard";
        }

        private string DetermineDelimiterInString(string targetFilterString)
        {
            var delimitersToCheck = new[] { "\t", ",", " ", Environment.NewLine };
            var mostFrequentDelimiter = string.Empty;

            var maxCount = int.MinValue;

            foreach (var delimiter in delimitersToCheck)
            {

                var tempStringArray = new[] { delimiter };

                var count = targetFilterString.Split(tempStringArray, StringSplitOptions.RemoveEmptyEntries).Length - 1;

                if (count > maxCount)
                {
                    mostFrequentDelimiter = delimiter;
                    maxCount = count;
                }
            }

            return mostFrequentDelimiter;
        }

        private void SetResults(IEnumerable<TargetedResultDTO> resultsToSet)
        {
            var query = (from n in resultsToSet select (SipperLcmsFeatureTargetedResultDTO)n);

            Results.Clear();
            foreach (var resultDto in query)
            {
                Results.Add(resultDto);
            }

            TargetsFileStatusText = "Viewing " + Results.Count + " results/targets";
        }

        private void FilterTargets()
        {
            //determine delimiter of TargetFilterString, if any
            if (string.IsNullOrEmpty(TargetFilterString))
            {
                SetResults(_resultRepositorySource.Results);
                return;
            }

            var delimitersToCheck = new[] { '\t', ',', '\n', ' ' };

            var trimmedFilterString = TargetFilterString.Trim(delimitersToCheck);

            var delimiter = DetermineDelimiterInString(trimmedFilterString);

            var filterList = new List<string>();
            if (!string.IsNullOrEmpty(delimiter))
            {
                var parsedFilterStringArray = trimmedFilterString.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                filterList.AddRange(parsedFilterStringArray);
            }
            else
            {
                filterList.Add(trimmedFilterString);
            }

            var filteredResults = new List<TargetedResultDTO>();
            foreach (var filter in filterList)
            {
                var isNumerical = int.TryParse(filter, out var myInt);

                if (isNumerical)
                {
                    filteredResults.AddRange(_resultRepositorySource.Results.Where(p => p.TargetID.ToString().StartsWith(myInt.ToString())));
                }
                else
                {
                    filteredResults.AddRange(_resultRepositorySource.Results.Where(p => p.Code.Contains(filter)));
                }
            }

            SetResults(filteredResults);

            //split string

            //determine if number or letters

            //if number, filter based on targetID;  if letters, filter based on code
        }

        #endregion
    }
}
