using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;
using Sipper.Model;

namespace Sipper.ViewModel
{

    public delegate void CurrentResultChangedHandler(object sender, EventArgs e);

    public class AutoprocessorViewModel : ViewModelBase
    {
        private SipperWorkflowExecutor _sipperWorkflowExecutor;
        private SipperTargetedWorkflow _sipperTargetedWorkflow;

        //private BackgroundWorker _backgroundWorker;

        private ResultFilterCriteria _filterCriteria;


        #region Constructors

        public AutoprocessorViewModel()
        {
            ExecutorParameters = new SipperWorkflowExecutorParameters();
            SipperWorkflowParameters = new SipperTargetedWorkflowParameters();
            StatusCollection = new ObservableCollection<string>();
            ProgressInfos = new ObservableCollection<TargetedWorkflowExecutorProgressInfo>();

            _filterCriteria = ResultFilterCriteria.GetFilterScheme1();

            Run = null;

        }

        #endregion

        #region Properties

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

        public string DatasetFilePath
        {
            get
            {

                if (Run == null)
                {
                    return "[Not selected yet]";
                }

                return Run.Filename;
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


        private string _workflowParametersFilePath;
        public string WorkflowParametersFilePath
        {
            get { return _workflowParametersFilePath; }
            set
            {
                _workflowParametersFilePath = value;
                OnPropertyChanged("WorkflowParametersFilePath");
            }

        }

        private string _targetsFilePath;
        public string TargetsFilePath
        {
            get { return _targetsFilePath; }
            set
            {
                _targetsFilePath = value;
                OnPropertyChanged("TargetsFilePath");
                
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
                if (!String.IsNullOrEmpty(DatasetFilePath) && !String.IsNullOrEmpty(WorkflowParametersFilePath) && !String.IsNullOrEmpty(TargetsFilePath))
                {
                    if (File.Exists(WorkflowParametersFilePath) && File.Exists(TargetsFilePath))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    StatusCollection.Add(DateTime.Now + "\t" + "Autoprocessor setup not complete. Please check setup.");
                    return false;
                }


            }
        }


        private Run _run;
        public Run Run
        {
            get { return _run; }
            set
            {
                _run = value;
                OnPropertyChanged("DatasetFilePath");
                OnPropertyChanged("RunStatusText");
            }
        }


        private int _percentProgress;
        
        private BackgroundWorker _backgroundWorker;

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


        public event PropertyChangedEventHandler PropertyChanged;

        private void OnCurrentResultChanged()
        {
            throw new NotImplementedException();
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
            }

            ProgressInfos.Clear();

            ExecutorParameters.TargetsFilePath = TargetsFilePath;
            ExecutorParameters.WorkflowParameterFile = WorkflowParametersFilePath;
            ExecutorParameters.LoggingFolder = Run.DataSetPath;
            ExecutorParameters.ResultsFolder = Run.DataSetPath;




            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.WorkerReportsProgress = true;

            _backgroundWorker.DoWork += _backgroundWorker_DoWork;
            _backgroundWorker.ProgressChanged += _backgroundWorker_ProgressChanged;
            _backgroundWorker.RunWorkerCompleted += _backgroundWorker_RunWorkerCompleted;
            _backgroundWorker.RunWorkerAsync();


        }

        void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = (BackgroundWorker)sender;
            _sipperWorkflowExecutor = new SipperWorkflowExecutor(ExecutorParameters, DatasetFilePath, worker);
            _sipperWorkflowExecutor.Execute();


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

                        var infostrings = info.ProgressInfoString.Split(new string[] {Environment.NewLine},
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
            if (CurrentResult==null || CurrentResult.Result==null)
            {
                return String.Empty;
            }

            var sipperResult = (SipperLcmsTargetedResult) CurrentResult.Result;

            StringBuilder stringBuilder=new StringBuilder();

            stringBuilder.Append("Target= ");
            stringBuilder.Append(sipperResult.Target.ID);

            stringBuilder.Append("; massTag= ");
            stringBuilder.Append(((LcmsFeatureTarget) sipperResult.Target).FeatureToMassTagID);


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


        public void CreateFileLinkages(IEnumerable<string> fileNames)
        {


            if (fileNames == null || !fileNames.Any())
            {
                return;
            }

            //pull out .xml file first
            var xmlFileNames = (from n in fileNames where Path.GetExtension(n) != null && Path.GetExtension(n).ToLower() == ".xml" select n);

            if (xmlFileNames.Any())
            {

                CreateFileLinkage(xmlFileNames.First());

                //remove all xml files from inputs
                if (fileNames != null) fileNames = fileNames.Except(xmlFileNames);


            }

            var txtFileNames = (from n in fileNames where Path.GetExtension(n) != null && Path.GetExtension(n) == ".txt" select n);

            if (txtFileNames.Any())
            {
                CreateFileLinkage(txtFileNames.First());

                //remove all text files from inputs
                if (fileNames != null) fileNames = fileNames.Except(txtFileNames);


            }


            if (fileNames.Any())
            {
                CreateFileLinkage(fileNames.First());

            }

        }

        public void CreateFileLinkage(string fileOrFolderPath)
        {

            bool isDir;
            try
            {

                isDir = (File.GetAttributes(fileOrFolderPath) & FileAttributes.Directory)
                        == FileAttributes.Directory;
            }
            catch (Exception ex)
            {

                throw;
            }


            if (isDir)
            {
                AttemptToInitializeRun(fileOrFolderPath);
            }
            else
            {
                string fileExtension = Path.GetExtension(fileOrFolderPath);

                if (fileExtension != null)
                {
                    fileExtension = fileExtension.ToLower();

                    if (fileExtension == ".xml")
                    {
                        WorkflowParametersFilePath = fileOrFolderPath;
                    }
                    else if (fileExtension == ".txt")
                    {
                        TargetsFilePath = fileOrFolderPath;
                    }
                    else
                    {
                        AttemptToInitializeRun(fileOrFolderPath);
                    }


                }


            }


        }
        #endregion

        #region Private Methods



        private void AttemptToInitializeRun(string fileOrFolderPath)
        {
            Run = new RunFactory().CreateRun(fileOrFolderPath);
        }
        #endregion

        public void CancelProcessing()
        {
            if (_backgroundWorker!=null)
            {
                _backgroundWorker.CancelAsync();
                StatusMessageGeneral = "Cancelled processing.";
            }
        }
    }
}
