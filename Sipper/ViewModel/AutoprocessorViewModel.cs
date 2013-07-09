using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Results;
using Sipper.Model;

namespace Sipper.ViewModel
{

    public delegate void CurrentResultChangedHandler(object sender, EventArgs e);

    public class AutoprocessorViewModel : ViewModelBase
    {
        private BasicTargetedWorkflowExecutor _workflowExecutor;
        private SipperTargetedWorkflow _sipperTargetedWorkflow;

        private BackgroundWorker _backgroundWorker;

        private TargetedResultRepository _resultRepository;


        #region Constructors

        public AutoprocessorViewModel(FileInputsInfo fileInputs = null)
        {
            ExecutorParameters = new SipperWorkflowExecutorParameters();
            ExecutorParameters.TargetType = Globals.TargetType.LcmsFeature;


            SipperWorkflowParameters = new SipperTargetedWorkflowParameters();
            StatusCollection = new ObservableCollection<string>();
            ProgressInfos = new ObservableCollection<TargetedWorkflowExecutorProgressInfo>();

            
            FileInputs = new FileInputsViewModel(fileInputs);
        }


        public AutoprocessorViewModel(TargetedResultRepository resultRepository, FileInputsInfo fileInputs = null)
            : this(fileInputs)
        {
            _resultRepository = resultRepository;
        }


        #endregion

        #region Properties

        public Run Run { get; set; }

        public FileInputsViewModel FileInputs { get; private set; }

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

        private TargetedWorkflowExecutorProgressInfo _currentResultInfo;
        

        public TargetedWorkflowExecutorProgressInfo CurrentResultInfo
        {
            get
            {
                return _currentResultInfo;
            }
            set
            {
                _currentResultInfo = value;
                OnPropertyChanged("CurrentResultInfo");
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
            ExecutorParameters.OutputFolderBase = GetOutputFolderPath();

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
           
            _workflowExecutor = new BasicTargetedWorkflowExecutor(ExecutorParameters, FileInputs.DatasetPath, worker);
            _workflowExecutor.RunIsDisposed = false;

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
                        CurrentResultInfo = info;
                        
                        //HACK: need to convert to the other Sipper result type in order to use the filter. 
                        var sipperResult = (SipperLcmsFeatureTargetedResultDTO)ResultDTOFactory.CreateTargetedResult(CurrentResultInfo.Result);
                        SipperFilters.ApplyAutoValidationCodeF2LooseFilter(sipperResult);
                        if (sipperResult.ValidationCode==ValidationCode.Yes)
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
            if (CurrentResultInfo == null || CurrentResultInfo.Result == null)
            {
                return String.Empty;
            }

            var sipperResult = (SipperLcmsTargetedResult)CurrentResultInfo.Result;

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
