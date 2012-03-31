using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;

namespace Sipper.ViewModel
{
    public class AutoprocessorViewModel : ViewModelBase
    {
        private SipperWorkflowExecutor _sipperWorkflowExecutor;
        private SipperTargetedWorkflow _sipperTargetedWorkflow;

        private BackgroundWorker _backgroundWorker;


        #region Constructors

        public AutoprocessorViewModel()
        {
            ExecutorParameters = new SipperWorkflowExecutorParameters();
            SipperWorkflowParameters = new SipperTargetedWorkflowParameters();
            StatusCollection = new ObservableCollection<string>();

            StatusCollection.Add("This is a tester");

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
        public int PercentProgress
        {
            get { return _percentProgress; }
            set
            {
                _percentProgress = value;
                OnPropertyChanged("PercentProgress");

            }
        }

        #endregion

        #region Public Methods

        public void Execute(BackgroundWorker backgroundWorker)
        {

            ExecutorParameters = new SipperWorkflowExecutorParameters();
            ExecutorParameters.TargetsFilePath = TargetsFilePath;
            ExecutorParameters.WorkflowParameterFile = WorkflowParametersFilePath;
            ExecutorParameters.LoggingFolder = Run.DataSetPath;


            _backgroundWorker = new BackgroundWorker();
            _backgroundWorker.WorkerSupportsCancellation = true;
            _backgroundWorker.WorkerReportsProgress = true;

            _backgroundWorker.DoWork += new DoWorkEventHandler(_backgroundWorker_DoWork);
            _backgroundWorker.ProgressChanged += new ProgressChangedEventHandler(_backgroundWorker_ProgressChanged);
            _backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(_backgroundWorker_RunWorkerCompleted);
            _backgroundWorker.RunWorkerAsync();


        }

        void _backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            PercentProgress = 100;
            //do nothing for now
        }

        void _backgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            PercentProgress = e.ProgressPercentage;

            if (e.UserState != null)
            {
                if (e.UserState is TargetedWorkflowExecutorProgressInfo)
                {
                    var info = (TargetedWorkflowExecutorProgressInfo)e.UserState;

                }
            }



        }

        void _backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            _sipperWorkflowExecutor = new SipperWorkflowExecutor(ExecutorParameters, DatasetFilePath, _backgroundWorker);
            _sipperWorkflowExecutor.Execute();
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



    }
}
