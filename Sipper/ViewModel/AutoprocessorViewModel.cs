using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Results;
using Sipper.Model;

namespace Sipper.ViewModel
{

    public delegate void CurrentResultChangedHandler(object sender, EventArgs e);

    public class AutoprocessorViewModel : ViewModelBase
    {
        private SipperWorkflowExecutor _sipperWorkflowExecutor;
        private SipperTargetedWorkflow _sipperTargetedWorkflow;

        private BackgroundWorker _backgroundWorker;

        private ResultFilterCriteria _filterCriteria;

        private TargetedResultRepository _resultRepository;


        #region Constructors

        public AutoprocessorViewModel(FileInputsInfo fileInputs = null)
        {
            ExecutorParameters = new SipperWorkflowExecutorParameters();
            SipperWorkflowParameters = new SipperTargetedWorkflowParameters();
            StatusCollection = new ObservableCollection<string>();
            ProgressInfos = new ObservableCollection<TargetedWorkflowExecutorProgressInfo>();

            _filterCriteria = ResultFilterCriteria.GetFilterScheme1();
            FileInputs = new FileInputsViewModel(fileInputs);
        }


        public AutoprocessorViewModel(TargetedResultRepository resultRepository, FileInputsInfo fileInputs = null)
            : this(fileInputs)
        {
            _resultRepository = resultRepository;
        }


        #endregion

        #region Properties

        public FileInputsViewModel FileInputs { get; private set; }

        public Run Run { get; set; }

        public SipperWorkflowExecutorParameters ExecutorParameters { get; set; }

        public SipperTargetedWorkflowParameters SipperWorkflowParameters { get; set; }


        public int NumMSScansSummed
        {
            get { return SipperWorkflowParameters.NumMSScansToSum; }
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
            get { return _statusMessageGeneral; }
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
                else
                {
                    StatusCollection.Add(DateTime.Now + "\t" + "Autoprocessor setup not complete. Please check setup.");
                    return false;
                }


            }
        }


        private int _percentProgress;
        public int PercentProgress
        {
            get { return _percentProgress; }
            set
            {
                _percentProgress = value;
                OnPropertyChanged("PercentProgress");

            }
        }

        private TargetedWorkflowExecutorProgressInfo _currentResult;
        

        public TargetedWorkflowExecutorProgressInfo CurrentResult
        {
            get
            {
                return _currentResult;
            }
            set
            {
                _currentResult = value;
                OnPropertyChanged("CurrentResult");
                OnCurrentResultUpdated(new EventArgs());
            }
        }


        #endregion


        public event CurrentResultChangedHandler CurrentResultUpdated;

        public void OnCurrentResultUpdated(EventArgs e)
        {
            CurrentResultChangedHandler handler = CurrentResultUpdated;
            if (handler != null) handler(this, e);
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
            ExecutorParameters.LoggingFolder = GetOutputFolderPath();
            ExecutorParameters.ResultsFolder = GetOutputFolderPath();





            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.WorkerReportsProgress = true;

            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync();


        }

        private string GetOutputFolderPath()
        {
            if (!String.IsNullOrEmpty(FileInputs.DatasetPath))
            {
                return RunUtilities.GetDatasetParentFolder(FileInputs.DatasetPath);

            }

            return String.Empty;
        }

        void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            _sipperWorkflowExecutor = new SipperWorkflowExecutor(ExecutorParameters, FileInputs.DatasetPath, worker);
            _sipperWorkflowExecutor.Execute();

            _resultRepository.Results.Clear();
            _resultRepository.Results.AddRange(_sipperWorkflowExecutor.GetResults());


            if (worker.CancellationPending)
            {
                e.Cancel = true;
            }

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
                StatusMessageGeneral = "Processing COMPLETE.";
                PercentProgress = 100;
            }
        }

        void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PercentProgress = e.ProgressPercentage;

            if (e.UserState != null)
            {
                if (e.UserState is TargetedWorkflowExecutorProgressInfo)
                {
                    var info = (TargetedWorkflowExecutorProgressInfo)e.UserState;
                    if (info.IsGeneralProgress)
                    {

                        var infostrings = info.ProgressInfoString.Split(new string[] { Environment.NewLine },
                                                                        StringSplitOptions.RemoveEmptyEntries);

                        foreach (var infostring in infostrings)
                        {
                            if (!String.IsNullOrEmpty(infostring))
                            {
                                StatusCollection.Add(infostring);
                                StatusMessageGeneral = infostring;
                            }
                        }



                    }
                    else
                    {
                        CurrentResult = info;

                        if (ResultPassesFilterCriteria(CurrentResult.Result as SipperLcmsTargetedResult))
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

        private bool ResultPassesFilterCriteria(SipperLcmsTargetedResult sipperResult)
        {

            if (sipperResult.InterferenceScore >= _filterCriteria.IScoreMin
                && sipperResult.InterferenceScore <= _filterCriteria.IScoreMax
                && sipperResult.AreaUnderRatioCurveRevised >= _filterCriteria.AreaUnderRatioCurveRevisedMin
                && sipperResult.ChromCorrelationAverage >= _filterCriteria.ChromCorrelationAverageMin
                && sipperResult.ChromCorrelationMedian >= _filterCriteria.ChromCorrelationMedianMin
                && sipperResult.RSquaredValForRatioCurve >= _filterCriteria.RSquaredValForRatioCurveMin
                )
            {
                return true;
            }
            return false;
        }



        public string GetInfoStringOnCurrentResult()
        {
            if (CurrentResult == null || CurrentResult.Result == null)
            {
                return String.Empty;
            }

            var sipperResult = (SipperLcmsTargetedResult)CurrentResult.Result;

            StringBuilder stringBuilder = new StringBuilder();

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
