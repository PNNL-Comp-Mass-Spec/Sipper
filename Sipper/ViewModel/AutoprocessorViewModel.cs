﻿using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using DeconTools.Backend;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Results;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using Sipper.Model;
using Globals = DeconTools.Workflows.Backend.Globals;

namespace Sipper.ViewModel
{

    public delegate void CurrentResultChangedHandler(object sender, EventArgs e);

    public class AutoprocessorViewModel : ViewModelBase
    {
        private BasicTargetedWorkflowExecutor _workflowExecutor;

        // private SipperTargetedWorkflow _sipperTargetedWorkflow;

        private BackgroundWorker _backgroundWorker;

        private readonly TargetedResultRepository _resultRepository;

        #region Constructors

        public AutoprocessorViewModel()
        {
            ExecutorParameters = new SipperWorkflowExecutorParameters
            {
                TargetType = Globals.TargetType.LcmsFeature
            };

            SipperWorkflowParameters = new SipperTargetedWorkflowParameters();
            StatusCollection = new ObservableCollection<string>();
            ProgressInfos = new ObservableCollection<TargetedWorkflowExecutorProgressInfo>();

            FileInputs = new FileInputsViewModel(null);
        }

        public AutoprocessorViewModel(FileInputsInfo fileInputs) : this()
        {

            FileInputs = new FileInputsViewModel(fileInputs);
        }

        public AutoprocessorViewModel(TargetedResultRepository resultRepository, FileInputsInfo fileInputs = null)
            : this(fileInputs)
        {
            _resultRepository = resultRepository;
        }

        #endregion

        #region Properties

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

        public Run Run { get; set; }

        public FileInputsViewModel FileInputs { get; }

        public SipperWorkflowExecutorParameters ExecutorParameters { get; set; }

        public SipperTargetedWorkflowParameters SipperWorkflowParameters { get; set; }

        public int NumMSScansSummed
        {
            get => SipperWorkflowParameters.NumMSScansToSum;
            set
            {
                SipperWorkflowParameters.NumMSScansToSum = value;
                OnPropertyChanged("NumMSScansSummed");
            }
        }

        public ObservableCollection<string> StatusCollection { get; set; }

        public ObservableCollection<TargetedWorkflowExecutorProgressInfo> ProgressInfos { get; set; }

        private string _statusMessageGeneral;
        public string StatusMessageGeneral
        {
            get => _statusMessageGeneral;
            set
            {
                _statusMessageGeneral = value;
                OnPropertyChanged("StatusMessageGeneral");
            }
        }

        public bool CanExecutorBeExecuted
        {
            get
            {
                if (FileInputs.PathsAreValid())
                {
                    return true;
                }

                StatusCollection.Add(DateTime.Now + "\t" + "Autoprocessor setup not complete. Please check setup.");
                return false;
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

        private TargetedWorkflowExecutorProgressInfo _currentResultInfo;

        public TargetedWorkflowExecutorProgressInfo CurrentResultInfo
        {
            get => _currentResultInfo;
            set
            {
                _currentResultInfo = value;
                GetMassSpectrumForCurrentResult();
                OnPropertyChanged("CurrentResultInfo");
            }
        }

        private void GetMassSpectrumForCurrentResult()
        {

            if (ObservedIsoPlot == null)
            {
                ObservedIsoPlot = CreateObservedIsoPlot();
            }

            XYData xyData;

            if (CurrentResultInfo.MassSpectrumXYData == null)
            {
                xyData = new XYData
                {
                    Xvalues = CurrentResultInfo.MassSpectrumXYData == null ? new double[] { 400, 1500 } : CurrentResultInfo.MassSpectrumXYData.Xvalues,
                    Yvalues = CurrentResultInfo.MassSpectrumXYData == null ? new double[] { 0, 0 } : CurrentResultInfo.MassSpectrumXYData.Yvalues
                };
            }
            else
            {
                var xyDataSource = new XYData
                {
                    Xvalues = CurrentResultInfo.MassSpectrumXYData.Xvalues,
                    Yvalues = CurrentResultInfo.MassSpectrumXYData.Yvalues
                };

                xyData = xyDataSource.TrimData(CurrentResultInfo.Result.Target.MZ - 2, CurrentResultInfo.Result.Target.MZ + 8);
            }

            double msGraphMaxY;
            if (CurrentResultInfo.Result.IsotopicProfile != null)
            {
                msGraphMaxY = CurrentResultInfo.Result.IsotopicProfile.getMostIntensePeak().Height;
            }
            else
            {
                msGraphMaxY = (float)xyData.GetMaxY();
            }

            // ReSharper disable once UnusedVariable
            var msGraphTitle = "TargetID= " + CurrentResultInfo.Result.Target.ID +
                               "; m/z " + CurrentResultInfo.Result.Target.MZ.ToString("0.0000") +
                               "; z=" + CurrentResultInfo.Result.Target.ChargeState +
                               "; Scan= " + CurrentResultInfo.Result.ScanSet ?? "[No scan selected]";

            ObservedIsoPlot.Series.Clear();

            var series = new LineSeries
            {
                MarkerSize = 1,
                Color = OxyColors.Black
            };

            for (var i = 0; i < xyData.Xvalues.Length; i++)
            {
                series.Points.Add(new DataPoint(xyData.Xvalues[i], xyData.Yvalues[i]));
            }

            ObservedIsoPlot.Axes[1].Maximum = msGraphMaxY + msGraphMaxY * 0.05;
            ObservedIsoPlot.Series.Add(series);
        }

        private PlotModel CreateObservedIsoPlot()
        {
            var plotModel = new PlotModel
            {
                TitleFontSize = 11,
                Padding = new OxyThickness(0),
                PlotMargins = new OxyThickness(0),
                PlotAreaBorderThickness = new OxyThickness(0)
            };

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "m/z"
            };
            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Intensity",
                Minimum = 0,
                AbsoluteMinimum = 0
            };

            xAxis.AxislineStyle = LineStyle.Solid;
            xAxis.AxislineThickness = 1;
            yAxis.AxislineStyle = LineStyle.Solid;
            yAxis.AxislineThickness = 1;

            plotModel.Axes.Add(xAxis);
            plotModel.Axes.Add(yAxis);

            return plotModel;
        }

        #endregion

        public event CurrentResultChangedHandler CurrentResultUpdated;

        public void OnCurrentResultUpdated(EventArgs e)
        {
            CurrentResultUpdated?.Invoke(this, e);
        }

        #region Public Methods

        public void Execute()
        {

            if (_backgroundWorker != null && _backgroundWorker.IsBusy)
            {
                StatusMessageGeneral = "Already processing.... please wait or click 'Cancel'";
                return;
            }

            if (!FileInputs.PathsAreValid())
            {
                StatusMessageGeneral = "Failed to execute. There is a problem with the file inputs. Check paths.";
                return;
            }

            ProgressInfos.Clear();

            ExecutorParameters.TargetsFilePath = FileInputs.TargetsFilePath;
            ExecutorParameters.WorkflowParameterFile = FileInputs.ParameterFilePath;
            ExecutorParameters.OutputDirectoryBase = GetOutputFolderPath();

            _backgroundWorker = new BackgroundWorker
            {
                WorkerSupportsCancellation = true,
                WorkerReportsProgress = true
            };

            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync();
        }

        private string GetOutputFolderPath()
        {
            if (!string.IsNullOrEmpty(FileInputs.DatasetPath))
            {
                return RunUtilities.GetDatasetParentDirectory(FileInputs.DatasetPath);
            }

            return string.Empty;
        }

        void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            _workflowExecutor = new BasicTargetedWorkflowExecutor(ExecutorParameters, FileInputs.DatasetPath, worker)
            {
                RunIsDisposed = false
            };

            _workflowExecutor.Execute();

            _resultRepository.Results.Clear();
            _resultRepository.Results.AddRange(_workflowExecutor.GetResults());

            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }

            Run = _workflowExecutor.Run;
        }

        void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                StatusMessageGeneral = "Cancelled";
            }
            else if (e.Error != null)
            {
                StatusMessageGeneral = "Error - check log file or results output";
            }
            else
            {
                StatusMessageGeneral = "Processing COMPLETE. #chromatograms extracted=?";
                PercentProgress = 100;
            }
        }

        void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PercentProgress = e.ProgressPercentage;

            if (e.UserState != null)
            {
                if (e.UserState is TargetedWorkflowExecutorProgressInfo info)
                {
                    if (info.IsGeneralProgress)
                    {

                        var infoStrings = info.ProgressInfoString.Split(new[] { Environment.NewLine },
                                                                        StringSplitOptions.RemoveEmptyEntries);

                        foreach (var infoString in infoStrings)
                        {
                            if (!string.IsNullOrEmpty(infoString))
                            {
                                StatusCollection.Add(infoString);
                                StatusMessageGeneral = infoString;
                            }
                        }
                    }
                    else
                    {
                        CurrentResultInfo = info;

                        //HACK: need to convert to the other Sipper result type in order to use the filter.
                        var sipperResult = (SipperLcmsFeatureTargetedResultDTO)ResultDTOFactory.CreateTargetedResult(CurrentResultInfo.Result);
                        SipperFilters.ApplyAutoValidationCodeF2LooseFilter(sipperResult);
                        if (sipperResult.ValidationCode == ValidationCode.Yes)
                        {
                            ProgressInfos.Add(info);
                        }
                    }
                }
                else
                {
                    Console.WriteLine(e.UserState);
                }
            }
        }

        public string GetInfoStringOnCurrentResult()
        {
            if (CurrentResultInfo?.Result == null)
            {
                return string.Empty;
            }

            var sipperResult = (SipperLcmsTargetedResult)CurrentResultInfo.Result;

            var stringBuilder = new StringBuilder();

            stringBuilder.Append("Target= ");
            stringBuilder.Append(sipperResult.Target.ID);

            stringBuilder.Append("; massTag= ");
            stringBuilder.Append(((LcmsFeatureTarget)sipperResult.Target).FeatureToMassTagID);

            stringBuilder.Append("; m/z= ");
            stringBuilder.Append(sipperResult.IsotopicProfile == null
                                     ? "-.---"
                                     : sipperResult.IsotopicProfile.MonoPeakMZ.ToString("0.0000"));

            stringBuilder.Append("; z=");
            stringBuilder.Append(sipperResult.IsotopicProfile == null
                                     ? "--"
                                     : sipperResult.IsotopicProfile.ChargeState.ToString("0"));

            return stringBuilder.ToString();
        }

        #endregion

        #region Private Methods

        #endregion

        public void CancelProcessing()
        {
            if (_backgroundWorker != null)
            {
                _backgroundWorker.CancelAsync();
                StatusMessageGeneral = "Cancelled processing.";
            }
        }
    }
}
