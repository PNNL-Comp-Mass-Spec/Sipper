using System;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Runs;
using OxyPlot;
using OxyPlot.Axes;

namespace Sipper.ViewModel
{
    public class SimpleMsViewerViewModel : ViewModelBase
    {

        private MSGenerator _msGenerator;
        private ScanSetFactory _scanSetFactory = new ScanSetFactory();


        #region Constructors


        public SimpleMsViewerViewModel():this(null)
        {
            
        }


        public SimpleMsViewerViewModel(Run run)
        {
            this.Run = run;
            MSGraphMinX = 400;
            MSGraphMaxX = 1500;
            NumMSScansToSum = 1;

            NavigateToNextMS1MassSpectrum();

        }


        #endregion

        #region Properties

        private DeconTools.Backend.Core.Run _run;
        public DeconTools.Backend.Core.Run Run
        {
            get { return _run; }
            set
            {
                _run = value;
                
            }
        }

        int _currentLcScan;
        public int CurrentLcScan
        {
            get
            {
                return _currentLcScan;
            }
            set
            {
                _currentLcScan = value;
                OnPropertyChanged("CurrentLcScan");
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
            get { return _massSpecXYData; }
            set
            {
                _massSpecXYData = value;
                OnPropertyChanged("MassSpecXYData");
            }
        }

        private int _numMsScansToSum;
        public int NumMSScansToSum
        {
            get
            {
                return _numMsScansToSum;
            }
            set
            {
                _numMsScansToSum = value;
                OnPropertyChanged("NumMSScansToSum");
            }
        }


        private double _msGraphMaxX;
        public double MSGraphMaxX
        {
            get { return _msGraphMaxX; }
            set
            {
                _msGraphMaxX = value;
                OnPropertyChanged("MSGraphMaxX");
            }
        }

        private double _msGraphMinX;
        public double MSGraphMinX
        {
            get { return _msGraphMinX; }
            set
            {
                _msGraphMinX = value;
                OnPropertyChanged("MSGraphMinX");
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
            get { return _observedIsoPlot; }
            set
            {
                _observedIsoPlot = value;
                OnPropertyChanged("ObservedIsoPlot");
            }
        }

        string _generalStatusMessage;
        public string GeneralStatusMessage
        {
            get
            {
                return _generalStatusMessage;
            }
            set
            {
                _generalStatusMessage = value;
                OnPropertyChanged("GeneralStatusMessage");
            }
        }



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
                NavigateToNextMS1MassSpectrum();
            }

            OnPropertyChanged("DatasetName");
        }



    

        public void NavigateToNextMS1MassSpectrum(Globals.ScanSelectionMode selectionMode = Globals.ScanSelectionMode.ASCENDING)
        {
            if (Run == null) return;

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


            var currentScanSet = _scanSetFactory.CreateScanSet(Run, CurrentLcScan, NumMSScansToSum);
            MassSpecXYData = _msGenerator.GenerateMS(Run, currentScanSet);

            if (MassSpecXYData != null)
            {
                MassSpecXYData = MassSpecXYData.TrimData(MSGraphMinX - 20, MSGraphMaxX + 20);
            }

            CreateMSPlotForScanByScanAnalysis(currentScanSet);

            int numPoints = MassSpecXYData == null ? 0 : MassSpecXYData.Xvalues.Length;
            GeneralStatusMessage = "Showing scan " + currentScanSet.PrimaryScanNumber; 



        }





        #endregion

        #region Private Methods


        private void CreateMSPlotForScanByScanAnalysis(ScanSet scanSet)
        {
            XYData xydata = new XYData();
            xydata.Xvalues = MassSpecXYData == null ? new double[] { 400, 1500 } : MassSpecXYData.Xvalues;
            xydata.Yvalues = MassSpecXYData == null ? new double[] { 0, 0 } : MassSpecXYData.Yvalues;

            string msGraphTitle = "Observed MS - Scan: " + scanSet;

            var maxY = (float)xydata.getMaxY(MSGraphMinX, MSGraphMaxX);


            PlotModel plotModel = new PlotModel(msGraphTitle);
            plotModel.TitleFontSize = 11;
            plotModel.Padding = new OxyThickness(0);
            plotModel.PlotMargins = new OxyThickness(0);
            plotModel.PlotAreaBorderThickness = 0;



            var series = new OxyPlot.Series.LineSeries();
            series.MarkerSize = 1;
            series.Color = OxyColors.Black;
            for (int i = 0; i < xydata.Xvalues.Length; i++)
            {
                series.Points.Add(new DataPoint(xydata.Xvalues[i], xydata.Yvalues[i]));
            }

            var xAxis = new LinearAxis(AxisPosition.Bottom, "m/z");
            xAxis.Minimum = MSGraphMinX;
            xAxis.Maximum = MSGraphMaxX;

            var yAxis = new LinearAxis(AxisPosition.Left, "Intensity");
            yAxis.Minimum = 0;
            yAxis.AbsoluteMinimum = 0;
            yAxis.Maximum = maxY + maxY * 0.05;
            //yAxis.Maximum = maxIntensity + (maxIntensity * .05);
            //yAxis.AbsoluteMaximum = maxIntensity + (maxIntensity * .05);
            yAxis.AxisChanged += OnYAxisChange;



            xAxis.AxislineStyle = LineStyle.Solid;
            xAxis.AxislineThickness = 1;
            yAxis.AxislineStyle = LineStyle.Solid;
            yAxis.AxislineThickness = 1;

            plotModel.Series.Add(series);
            plotModel.Axes.Add(yAxis);
            plotModel.Axes.Add(xAxis);

            ObservedIsoPlot = plotModel;


        }

        private void OnYAxisChange(object sender, AxisChangedEventArgs e)
        {
            LinearAxis yAxis = sender as LinearAxis;

            // No need to update anything if the minimum is already <= 0
            if (yAxis.ActualMinimum <= 0) return;

            // Set the minimum to 0 and refresh the plot
            yAxis.Zoom(0, yAxis.ActualMaximum);
            yAxis.PlotModel.RefreshPlot(true);
        }



        #endregion

    }
}
