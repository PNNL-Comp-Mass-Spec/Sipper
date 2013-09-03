using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Utilities;
using OxyPlot;
using OxyPlot.Axes;
using Sipper.Model;

namespace Sipper.ViewModel
{
    public class SipperParameterOptimizerViewModel : ViewModelBase
    {

        private SipperFilterOptimizer _filterOptimizer;

        #region Constructors

        public SipperParameterOptimizerViewModel()
        {
            _filterOptimizer = new SipperFilterOptimizer();
            MaxAllowedFalsePositiveRate = 0.1;
        }


        #endregion

        #region Properties

        private ParameterOptimizationResult _bestParameter;
        public ParameterOptimizationResult BestParameter
        {
            get { return FilteredParameters.FirstOrDefault(); }
            set
            {
                _bestParameter = value;

                OnPropertyChanged("BestParameter");
            }
        }

        private string _unlabeledFilePath;
        public string UnlabeledFilePath
        {
            get { return _unlabeledFilePath; }
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
            get { return _labeledFilePath; }
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
            get { return _outputFileName; }
            set
            {
                _outputFileName = value;
                OnPropertyChanged("OutputFileName");
            }
        }




        private void UpdateOutputFileName()
        {
            if (string.IsNullOrEmpty(OutputFileName))     //only update output file name if it is null or empty
            {
                if (!string.IsNullOrEmpty(UnlabeledFilePath))
                {
                    FileInfo fileInfo = new FileInfo(UnlabeledFilePath);

                    if (fileInfo.Exists)
                    {
                        if (fileInfo.Directory != null)
                        {
                            string outputPath = fileInfo.Directory.FullName;

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


        public double MaxAllowedFalsePositiveRate { get; set; }


        #endregion

        #region Public Methods

        public void CreateRocCurve()
        {
            var rocData = _filterOptimizer.GetRocCurve(AllParameterResults);
            string graphTitle = "ROC curve";

            PlotModel plotModel = new PlotModel(graphTitle);
            plotModel.TitleFontSize = 11;
            plotModel.Padding = new OxyThickness(0);
            plotModel.PlotMargins = new OxyThickness(0);
            plotModel.PlotAreaBorderThickness = 0;

            var series = new OxyPlot.Series.LineSeries();
            series.MarkerSize = 1;
            series.Color = OxyColors.Black;
            for (int i = 0; i < rocData.Xvalues.Length; i++)
            {
                series.Points.Add(new DataPoint(rocData.Xvalues[i], rocData.Yvalues[i]));
            }

            var xAxis = new LinearAxis(AxisPosition.Bottom, "labeled count");
            xAxis.Minimum = 0;

            var yAxis = new LinearAxis(AxisPosition.Left, "unlabeled count");
            yAxis.Minimum = 0;

            plotModel.Series.Add(series);
            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);


            RocPlot = plotModel;


        }

        private PlotModel _rocPlot;
        public PlotModel RocPlot
        {
            get { return _rocPlot; }
            set
            {
                _rocPlot = value;
                OnPropertyChanged("RocPlot");
            }
        }


        private List<ParameterOptimizationResult> _filteredParameters;
        public List<ParameterOptimizationResult> FilteredParameters
        {
            get { return _filteredParameters; }
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
                FilteredParameters = _filterOptimizer.GetOptimizedFiltersByFalsePositiveRate(AllParameterResults,
                                                                                             MaxAllowedFalsePositiveRate).Take(200).ToList();

                CreateRocCurve();
            }
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

    }
}
