using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using OxyPlot;
using OxyPlot.Axes;
using PRISM.Logging;
using Sipper.Model;

namespace Sipper.ViewModel
{
    public class SipperParameterOptimizerViewModel : ViewModelBase
    {

        private readonly SipperFilterOptimizer _filterOptimizer;

        #region Constructors

        public SipperParameterOptimizerViewModel()
        {
            _filterOptimizer = new SipperFilterOptimizer();
            AllParameterResults = new List<ParameterOptimizationResult>();
            MaxAllowedFalsePositiveRate = 0.1;

            BaseLogger.TimestampFormat = LogMessage.TimestampFormatMode.YearMonthDay24hr;

            FileLogger.ChangeLogFileBaseName(@"Logs\Sipper", appendDateToBaseName: true);
        }

        #endregion

        #region Properties

        private ParameterOptimizationResult _bestParameter;
        public ParameterOptimizationResult BestParameter
        {
            get => FilteredParameters.FirstOrDefault();
            set
            {
                _bestParameter = value;

                OnPropertyChanged("BestParameter");
            }
        }

        private string _unlabeledFilePath;
        public string UnlabeledFilePath
        {
            get => _unlabeledFilePath;
            set
            {
                _unlabeledFilePath = value;
                OnPropertyChanged("UnlabeledFilePath");

                TryLoadUnlabeledResults(_unlabeledFilePath);

                UpdateOutputFileName();
            }
        }

        private string _labeledFilePath;
        public string LabeledFilePath
        {
            get => _labeledFilePath;
            set
            {
                _labeledFilePath = value;
                OnPropertyChanged("LabeledFilePath");

                TryLoadLabeledResults(_labeledFilePath);
            }
        }

        private string _outputFileName;
        public string OutputFileName
        {
            get => _outputFileName;
            set
            {
                _outputFileName = value;
                OnPropertyChanged("OutputFileName");
            }
        }

        public XYData RocCurve { get; set; }

        private void UpdateOutputFileName()
        {
            if (string.IsNullOrEmpty(OutputFileName))     //only update output file name if it is null or empty
            {
                if (!string.IsNullOrEmpty(UnlabeledFilePath))
                {
                    var fileInfo = new FileInfo(UnlabeledFilePath);

                    if (fileInfo.Exists)
                    {
                        if (fileInfo.Directory != null)
                        {
                            var outputPath = fileInfo.Directory.FullName;

                            OutputFileName = outputPath + Path.DirectorySeparatorChar +
                                             "SipperFilterOptimizationOutput.txt";
                        }
                    }
                }
            }
        }

        private void TryLoadUnlabeledResults(string unlabeledFilePath)
        {
            if (File.Exists(unlabeledFilePath))
            {
                _filterOptimizer.LoadUnlabeledResults(unlabeledFilePath);
            }
        }

        private void TryLoadLabeledResults(string labeledFilePath)
        {
            if (File.Exists(labeledFilePath))
            {
                _filterOptimizer.LoadLabeledResults(labeledFilePath);
            }
        }

        private double _maxAllowedFalsePositiveRate;
        public double MaxAllowedFalsePositiveRate
        {
            get => _maxAllowedFalsePositiveRate;
            set
            {
                _maxAllowedFalsePositiveRate = value;
                OnPropertyChanged("MaxAllowedFalsePositiveRate");

                UpdateFilteredParameters();
            }
        }

        private ParameterOptimizationResult _selectedFilterParameter;
        /// <summary>
        /// This is parameter set that represents the best filter set and will be used in the autoprocessor and/or the viewer.
        /// </summary>
        public ParameterOptimizationResult SelectedFilterParameter
        {
            get => _selectedFilterParameter;
            set
            {
                _selectedFilterParameter = value;
                OnPropertyChanged("SelectedFilterParameter");

                SelectedFilterReportString = GenerateFilterReportString(SelectedFilterParameter);
            }
        }

        private string _selectedFilterReportString;
        public string SelectedFilterReportString
        {
            get => _selectedFilterReportString;
            set
            {
                _selectedFilterReportString = value;
                OnPropertyChanged("SelectedFilterReportString");
            }
        }

        private ParameterOptimizationResult _currentFilterParameter;
        /// <summary>
        /// This is the currently selected FilterParameter when a user selects a parameter set from the table in the UI
        /// </summary>
        public ParameterOptimizationResult CurrentFilterParameter
        {
            get => _currentFilterParameter;
            set
            {
                _currentFilterParameter = value;
                OnPropertyChanged("CurrentFilterParameter");

                CurrentFilterReportString = GenerateFilterReportString(CurrentFilterParameter);
            }
        }

        private string _currentFilterReportString;
        public string CurrentFilterReportString
        {
            get => _currentFilterReportString;
            set
            {
                _currentFilterReportString = value;

                OnPropertyChanged("CurrentFilterReportString");
            }
        }

        #endregion

        #region Public Methods

        public void CreateRocCurve()
        {
            RocCurve = _filterOptimizer.GetRocCurve(AllParameterResults);
            var graphTitle = "ROC curve";

            var plotModel = new PlotModel
            {
                Title = graphTitle,
                TitleFontSize = 11,
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0),
                PlotAreaBorderThickness = new OxyThickness(0),
            };

            var series = new OxyPlot.Series.LineSeries {
                MarkerSize = 1, Color = OxyColors.Black
            };

            for (var i = 0; i < RocCurve.Xvalues.Length; i++)
            {
                series.Points.Add(new DataPoint(RocCurve.Xvalues[i], RocCurve.Yvalues[i]));
            }

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "unlabeled count",
                Minimum = 0
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "labeled count",
                Minimum = 0
            };

            plotModel.Series.Add(series);
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            RocPlot = plotModel;
        }

        public void SaveRocCurve(string fileName)
        {
            using (var writer=new StreamWriter(fileName))
            {
                var header = "numUnlabeled\tnumLabeled";
                writer.WriteLine(header);

                var xyData = new XYData();

                if (RocCurve?.Xvalues == null || RocCurve.Xvalues.Length==0)
                {
                    xyData.Xvalues=new double[0];
                    xyData.Yvalues=new double[0];
                }
                else
                {
                    xyData.Xvalues = RocCurve.Xvalues;
                    xyData.Yvalues = RocCurve.Yvalues;
                }

                for (var i = 0; i < xyData.Xvalues.Length; i++)
                {
                    writer.WriteLine(xyData.Xvalues[i] + "\t" + xyData.Yvalues[i]);
                }
            }
        }

        private PlotModel _rocPlot;
        public PlotModel RocPlot
        {
            get => _rocPlot;
            set
            {
                _rocPlot = value;
                OnPropertyChanged("RocPlot");
            }
        }

        private List<ParameterOptimizationResult> _filteredParameters;
        public List<ParameterOptimizationResult> FilteredParameters
        {
            get => _filteredParameters;
            set
            {
                _filteredParameters = value;
                OnPropertyChanged("FilteredParameters");
            }
        }

        public void DoCalculationsOnAllParameterCombinations()
        {
            if (CanExecuteMainCalculation())
            {
                AllParameterResults = _filterOptimizer.DoCalculationsOnAllFilterCombinations(OutputFileName);
                UpdateFilteredParameters();

                CreateRocCurve();
            }
        }

        private void UpdateFilteredParameters()
        {
            FilteredParameters = _filterOptimizer.GetOptimizedFiltersByFalsePositiveRate(AllParameterResults,
                                                                                         MaxAllowedFalsePositiveRate).Take(200).ToList();
        }

        protected List<ParameterOptimizationResult> AllParameterResults { get; set; }

        public bool CanExecuteMainCalculation()
        {
            //TODO: add logic
            return true;
        }

        #endregion

        #region Private Methods

        #endregion

        public string GenerateFilterReportString(ParameterOptimizationResult selectedResult)
        {
            if (selectedResult == null) return string.Empty;

            var sb = new StringBuilder();
            sb.Append("Labeled fit <= " + selectedResult.FitScoreLabeled.ToString("0.####"));
            sb.Append(Environment.NewLine);
            sb.Append("IScore <= " + selectedResult.InterferenceScore.ToString("0.####"));
            sb.Append(Environment.NewLine);
            sb.Append("SumOfRatios >= " + selectedResult.SumOfRatios.ToString("0.#"));
            sb.Append(Environment.NewLine);
            sb.Append("ContigScore >= " + selectedResult.ContigScore.ToString("0"));
            sb.Append(Environment.NewLine);
            sb.Append("PercentIncorp >= " + selectedResult.PercentIncorporated.ToString("0.####"));
            sb.Append(Environment.NewLine);
            sb.Append("PercentPeptide >= " + selectedResult.PercentPeptidePopulation.ToString("0.##"));
            sb.Append(Environment.NewLine);
            sb.Append("Num unlabeled results at this filter= " + selectedResult.NumUnlabeledPassingFilter);
            sb.Append(Environment.NewLine);
            sb.Append("Num labeled results at this filter= " + selectedResult.NumLabeledPassingFilter);
            sb.Append(Environment.NewLine);
            sb.Append("False positive rate = " + selectedResult.FalsePositiveRate.ToString("0.###"));

            return sb.ToString();
        }
    }
}
