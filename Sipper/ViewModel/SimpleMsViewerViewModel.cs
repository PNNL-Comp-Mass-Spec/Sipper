using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.ProcessingTasks.PeakDetectors;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Workflows;
using OxyPlot;
using OxyPlot.Axes;

namespace Sipper.ViewModel
{
    public class SimpleMsViewerViewModel : ViewModelBase
    {
        private bool _xAxisIsChangedInternally;

        private MSGenerator _msGenerator;
        private readonly ScanSetFactory _scanSetFactory = new ScanSetFactory();
        private BackgroundWorker _backgroundWorker;
        private string _peaksFilename;
        readonly PeakChromatogramGenerator _peakChromatogramGenerator;
        private bool _recreatePeaksFile;

        #region Constructors

        public SimpleMsViewerViewModel()
            : this(null)
        {
        }

        public SimpleMsViewerViewModel(Run run)
        {
            Run = run;

            PeakDetector = new DeconToolsPeakDetectorV2();
            _peakChromatogramGenerator = new PeakChromatogramGenerator();
            Peaks = new List<Peak>();

            //the order matters here. See the properties.
            MSGraphMaxX = 1500;
            MSGraphMinX = 400;
            ChromToleranceInPpm = 20;

            ChromSourcePeakDetectorSigNoise = 2;
            ChromSourcePeakDetectorPeakBr = 3;

            NumMSScansToSum = 1;
            ShowMsMsSpectra = false;

            NavigateToNextMS1MassSpectrum();
        }

        #endregion

        #region Properties

        public double ChromToleranceInPpm { get; set; }

        private bool _showMsMsSpectra;
        public bool ShowMsMsSpectra
        {
            get => _showMsMsSpectra;
            set
            {
                _showMsMsSpectra = value;
                OnPropertyChanged("ShowMsMsSpectra");
            }
        }

        public DeconToolsPeakDetectorV2 PeakDetector { get; set; }

        private double _chromSourcePeakDetectorPeakBr;
        public double ChromSourcePeakDetectorPeakBr
        {
            get => _chromSourcePeakDetectorPeakBr;
            set
            {
                _chromSourcePeakDetectorPeakBr = value;
                OnPropertyChanged("ChromSourcePeakDetectorPeakBr");
            }
        }

        private double _chromSourcePeakDetectorSigNoise;
        public double ChromSourcePeakDetectorSigNoise
        {
            get => _chromSourcePeakDetectorSigNoise;
            set
            {
                _chromSourcePeakDetectorSigNoise = value;
                OnPropertyChanged("ChromSourcePeakDetectorSigNoise");
            }
        }

        private List<Peak> _peaks;
        public List<Peak> Peaks
        {
            get => _peaks;
            set
            {
                _peaks = value;
                OnPropertyChanged("Peaks");
            }
        }

        private Run _run;
        public Run Run
        {
            get => _run;
            set => _run = value;
        }

        int _currentLcScan;
        public int CurrentLcScan
        {
            get => _currentLcScan;
            set
            {
                _currentLcScan = value;
                OnPropertyChanged("CurrentLcScan");
            }
        }

        private ScanSet _currentScanSet;
        public ScanSet CurrentScanSet
        {
            get => _currentScanSet;
            set => _currentScanSet = value;
        }

        private Peak _selectedPeak;
        public Peak SelectedPeak
        {
            get => _selectedPeak;
            set
            {
                _selectedPeak = value;

                CreateChromatogram();
            }
        }

        public int MinLcScan
        {
            get
            {
                if (_run == null) return 1;
                return _run.MinLCScan;
            }
        }

        public int MaxLcScan
        {
            get
            {
                if (_run == null) return 1;
                return _run.MaxLCScan;
            }
        }

        private XYData _massSpecXYData;
        public XYData MassSpecXYData
        {
            get => _massSpecXYData;
            set
            {
                _massSpecXYData = value;
                OnPropertyChanged("MassSpecXYData");
            }
        }

        private int _numMsScansToSum;
        public int NumMSScansToSum
        {
            get => _numMsScansToSum;
            set
            {
                var isEvenNumber = value % 2 == 0;
                if (isEvenNumber)
                {
                    var tryingToSumMore = value > _numMsScansToSum;
                    if (tryingToSumMore)
                    {
                        value++;
                    }
                    else
                    {
                        value--;
                    }
                }

                if (value < 1)
                {
                    value = 1;
                }

                _numMsScansToSum = value;
                OnPropertyChanged("NumMSScansToSum");

                if (!_xAxisIsChangedInternally)
                {
                    NavigateToNextMS1MassSpectrum(Globals.ScanSelectionMode.CLOSEST);
                }
            }
        }

        private double _msGraphMaxX;
        public double MSGraphMaxX
        {
            get => _msGraphMaxX;
            set
            {
                if (value <= _msGraphMinX)
                {
                    value = _msGraphMinX + 0.001;
                }

                _msGraphMaxX = value;
                OnPropertyChanged("MSGraphMaxX");

                if (!_xAxisIsChangedInternally)
                {
                    NavigateToNextMS1MassSpectrum(Globals.ScanSelectionMode.CLOSEST);
                }
            }
        }

        private double _msGraphMinX;
        public double MSGraphMinX
        {
            get => _msGraphMinX;
            set
            {
                if (value >= _msGraphMaxX)
                {
                    value = _msGraphMaxX - 0.001;
                }

                _msGraphMinX = value;
                OnPropertyChanged("MSGraphMinX");

                if (!_xAxisIsChangedInternally)
                {
                    NavigateToNextMS1MassSpectrum(Globals.ScanSelectionMode.CLOSEST);
                }
            }
        }

        public string DatasetName
        {
            get
            {
                if (Run == null) return "";
                return Run.DatasetName;
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

        string _generalStatusMessage;
        public string GeneralStatusMessage
        {
            get => _generalStatusMessage;
            set
            {
                _generalStatusMessage = value;
                OnPropertyChanged("GeneralStatusMessage");
            }
        }

        private int _percentProgress;
        public int PercentProgress
        {
            get => _percentProgress;
            set
            {
                _percentProgress = value;
                OnPropertyChanged("PercentProgress");
            }
        }

        private bool _isProgressVisible;
        public bool IsProgressVisible
        {
            get => _isProgressVisible;
            set
            {
                _isProgressVisible = value;
                OnPropertyChanged("IsProgressVisible");
            }
        }

        protected XYData ChromXyData { get; set; }

        #endregion

        #region Public Methods

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

            if (Run != null)
            {
                LoadPeaksUsingBackgroundWorker();
            }

            if (Run != null)
            {
                NavigateToNextMS1MassSpectrum();
            }

            OnPropertyChanged("DatasetName");
        }

        public void LoadPeaksUsingBackgroundWorker(bool recreatePeaksFile = false)
        {

            _recreatePeaksFile = recreatePeaksFile;

            if (Run == null) return;

            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                GeneralStatusMessage = "Busy...";
                return;
            }

            _backgroundWorker = new BackgroundWorker {
                WorkerSupportsCancellation = true, WorkerReportsProgress = true
            };
            _backgroundWorker.RunWorkerCompleted += BackgroundWorkerCompleted;
            _backgroundWorker.ProgressChanged += BackgroundWorkerProgressChanged;
            _backgroundWorker.DoWork += BackgroundWorkerDoWork;

            _backgroundWorker.RunWorkerAsync();
        }

        public void NavigateToNextMS1MassSpectrum(Globals.ScanSelectionMode selectionMode = Globals.ScanSelectionMode.ASCENDING)
        {
            if (Run == null) return;

            int nextPossibleMs;
            if (selectionMode == Globals.ScanSelectionMode.DESCENDING)
            {
                nextPossibleMs = CurrentLcScan - 1;
            }
            else if (selectionMode == Globals.ScanSelectionMode.ASCENDING)
            {
                nextPossibleMs = CurrentLcScan + 1;
            }
            else
            {
                nextPossibleMs = CurrentLcScan;
            }

            if (!ShowMsMsSpectra)
            {
                CurrentLcScan = Run.GetClosestMSScan(nextPossibleMs, selectionMode);
            }
            else
            {
                CurrentLcScan = nextPossibleMs;
            }

            if (_msGenerator == null)
            {
                _msGenerator = MSGeneratorFactory.CreateMSGenerator(Run.MSFileType);
            }

            CurrentScanSet = _scanSetFactory.CreateScanSet(Run, CurrentLcScan, NumMSScansToSum);
            MassSpecXYData = _msGenerator.GenerateMS(Run, CurrentScanSet);

            Peaks = new List<Peak>();
            if (MassSpecXYData != null)
            {
                if (Run.IsDataCentroided(CurrentLcScan))
                {

                    MassSpecXYData = ZeroFillCentroidData(MassSpecXYData);
                }

                //Trim the viewable mass spectrum, but leave some data so user can pan to the right and left
                MassSpecXYData = MassSpecXYData.TrimData(MSGraphMinX - 20, MSGraphMaxX + 20);

                //Use only the data within the viewing area for peak detection
                var xyDataForPeakDetector = MassSpecXYData.TrimData(MSGraphMinX, MSGraphMaxX);
                Peaks = PeakDetector.FindPeaks(xyDataForPeakDetector.Xvalues, xyDataForPeakDetector.Yvalues);
            }

            CreateMSPlotForScanByScanAnalysis();

            // This triggers an XIC
            SelectedPeak = Peaks.OrderByDescending(p => p.Height).FirstOrDefault();
        }

        private XYData ZeroFillCentroidData(XYData massSpecXyData)
        {
            var newXValues = new List<double>();
            var newYValues = new List<double>();

            for (var i = 0; i < massSpecXyData.Xvalues.Length; i++)
            {
                var currentXVal = massSpecXyData.Xvalues[i];
                var currentYVal = massSpecXyData.Yvalues[i];

                var zeroFillDistance=0.005;
                var newXValBefore = currentXVal - zeroFillDistance;
                var newXValAfter = currentXVal + zeroFillDistance;

                newXValues.Add(newXValBefore);
                newYValues.Add(0);

                newXValues.Add(currentXVal);
                newYValues.Add(currentYVal);

                newXValues.Add(newXValAfter);
                newYValues.Add(0);
            }

            return new XYData {Xvalues = newXValues.ToArray(), Yvalues = newYValues.ToArray()};
        }

        private void CreateChromatogram()
        {
            var canGenerateChrom = Run?.ResultCollection.MSPeakResultList != null &&
                                   Run.ResultCollection.MSPeakResultList.Count > 0 &&
                                   Peaks != null &&
                                   Peaks.Count > 0 &&
                                   SelectedPeak != null;

            if (!canGenerateChrom) return;

            double scanWindowWidth = 600;
            var lowerScan = (int)Math.Round(Math.Max(MinLcScan, CurrentLcScan - scanWindowWidth / 2));
            var upperScan = (int)Math.Round(Math.Min(MaxLcScan, CurrentLcScan + scanWindowWidth / 2));

            ChromXyData = _peakChromatogramGenerator.GenerateChromatogram(Run, lowerScan, upperScan,
                                                                          SelectedPeak.XValue, ChromToleranceInPpm);

            if (ChromXyData == null)
            {
                ChromXyData = new XYData {
                    Xvalues = new double[] {lowerScan, upperScan},
                    Yvalues = new double[] {0, 0}
                };
            }

            var maxY = (float)ChromXyData.GetMaxY();

            var graphTitle = "XIC for most intense peak (m/z " + SelectedPeak.XValue.ToString("0.000") + ")";

            var plotModel = new PlotModel
            {
                Title = graphTitle,
                TitleFontSize = 9,
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0),
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var series = new OxyPlot.Series.LineSeries {
                MarkerSize = 1, Color = OxyColors.Black
            };

            for (var i = 0; i < ChromXyData.Xvalues.Length; i++)
            {
                series.Points.Add(new DataPoint(ChromXyData.Xvalues[i], ChromXyData.Yvalues[i]));
            }

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "scan",
                Minimum = lowerScan,
                Maximum = upperScan
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Intensity",
                Minimum = 0,
                AbsoluteMinimum=0,
                Maximum = maxY + maxY * 0.05
            };
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

        #endregion

        #region Private Methods

        private void CreateMSPlotForScanByScanAnalysis()
        {
            var xyData = new XYData
            {
                Xvalues = MassSpecXYData == null ? new double[] {400, 1500} : MassSpecXYData.Xvalues,
                Yvalues = MassSpecXYData == null ? new double[] {0, 0} : MassSpecXYData.Yvalues
            };

            var msGraphTitle = "Observed MS - Scan: " + (CurrentScanSet == null ? "" : CurrentScanSet.ToString());

            var maxY = (float)xyData.GetMaxY(MSGraphMinX, MSGraphMaxX);

            var plotModel = new PlotModel
            {
                Title = msGraphTitle,
                TitleFontSize = 11,
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0),
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            plotModel.MouseDown += MouseButtonDown;

            var series = new OxyPlot.Series.LineSeries {
                MarkerSize = 1, Color = OxyColors.Black
            };

            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                series.Points.Add(new DataPoint(xyData.Xvalues[i], xyData.Yvalues[i]));
            }

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "m/z",
                Minimum = MSGraphMinX,
                Maximum = MSGraphMaxX
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Intensity",
                Minimum = 0,
                AbsoluteMinimum = 0,
                Maximum = maxY + maxY * 0.05
            };

            //yAxis.Maximum = maxIntensity + (maxIntensity * .05);
            //yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
            yAxis.AxisChanged += OnYAxisChange;

            xAxis.AxisChanged += OnXAxisChange;

            xAxis.AxislineStyle = LineStyle.Solid;
            xAxis.AxislineThickness = 1;
            yAxis.AxislineStyle = LineStyle.Solid;
            yAxis.AxislineThickness = 1;

            plotModel.Series.Add(series);
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            ObservedIsoPlot = plotModel;
        }

        private void LoadPeaks()
        {
            try
            {
                _peaksFilename = Path.Combine(this.Run.DatasetDirectoryPath, this.Run.DatasetName + "_peaks.txt");
                var fiPeaksFile = new FileInfo(_peaksFilename);
                if (!fiPeaksFile.Exists)
                {
                    var alternatePeaksFilePath = Path.Combine(System.IO.Path.GetTempPath(), this.Run.DatasetName + "_peaks.txt");
                    if (File.Exists(alternatePeaksFilePath))
                    {
                        _peaksFilename = alternatePeaksFilePath;
                        fiPeaksFile = new FileInfo(_peaksFilename);
                    }
                }

                if (_recreatePeaksFile || !fiPeaksFile.Exists)
                {
                    _recreatePeaksFile = false;

                    // Make sure we have write access to the folder with the dataset file
                    try
                    {
                        using (var peaksFileWriter = new StreamWriter(new FileStream(fiPeaksFile.FullName, FileMode.Create, FileAccess.Write)))
                        {
                            peaksFileWriter.WriteLine("Test");
                        }
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Create the _peaks.txt file in the user's temporary folder
                        _peaksFilename = Path.Combine(System.IO.Path.GetTempPath(), this.Run.DatasetName + "_peaks.txt");
                        fiPeaksFile = new FileInfo(_peaksFilename);
                    }

                    fiPeaksFile.Refresh();
                    if (fiPeaksFile.Exists)
                        fiPeaksFile.Delete();

                    GeneralStatusMessage =
                        "Creating chromatogram data (_peaks.txt file); this is only done once. It takes 1 - 5 min .......";


                    var peakCreationParameters = new PeakDetectAndExportWorkflowParameters
                    {
                        PeakBR = ChromSourcePeakDetectorPeakBr,
                        PeakFitType = Globals.PeakFitType.QUADRATIC,
                        SigNoiseThreshold = ChromSourcePeakDetectorSigNoise,
                        OutputDirectory = fiPeaksFile.Directory.FullName
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
                peakImporter.ImportPeaks(this.Run.ResultCollection.MSPeakResultList);
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
                IsProgressVisible = false;
            }
        }

        private void BackgroundWorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (!IsProgressVisible)
                IsProgressVisible = true;

            PercentProgress = e.ProgressPercentage;
        }

        private void OnXAxisChange(object sender, AxisChangedEventArgs e)
        {
            if (!(sender is LinearAxis xAxis))
                return;

            _xAxisIsChangedInternally = true;

            MSGraphMinX = xAxis.ActualMinimum;
            MSGraphMaxX = xAxis.ActualMaximum;

            _xAxisIsChangedInternally = false;
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

        private void MouseButtonDown(object sender, OxyMouseEventArgs e)
        {
            var plot = ObservedIsoPlot;

            // var mouseButton = e.ChangedButton;
            var mouseButton = OxyMouseButton.Left;

            if (mouseButton == OxyMouseButton.Left)
            {
                var position = e.Position;

                var series = plot.GetSeriesFromPoint(position, 10);
                var hitResult = series?.GetNearestPoint(position, true);

                if (hitResult != null && hitResult.DataPoint.IsDefined())
                {
                    var dataPoint = hitResult.DataPoint;

                    SelectedPeak = new Peak(dataPoint.X, (float)dataPoint.Y, 0);

                    GeneralStatusMessage = "Selected point = " +
                                           dataPoint.X.ToString("0.000") + ", " +
                                           dataPoint.Y.ToString("0.000");
                }
            }
        }

        #endregion
    }
}
