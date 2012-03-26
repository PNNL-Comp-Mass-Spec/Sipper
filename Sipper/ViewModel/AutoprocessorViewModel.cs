using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Runs;
using DeconTools.Workflows.Backend.Core;

namespace Sipper.ViewModel
{
    public class AutoprocessorViewModel : ViewModelBase
    {

        #region Constructors

        public AutoprocessorViewModel()
        {
            ExecutorParameters = new SipperWorkflowExecutorParameters();
            SipperWorkflowParameters = new SipperTargetedWorkflowParameters();
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

        public string WorkflowParametersFilePath
        {
            get { return ExecutorParameters.WorkflowParameterFile; }
            set
            {
                ExecutorParameters.WorkflowParameterFile = value;
                OnPropertyChanged("WorkflowParametersFilePath");
            }

        }

        public string TargetsFilePath
        {
            get { return ExecutorParameters.TargetsFilePath; }
            set
            {
                ExecutorParameters.TargetsFilePath = value;
                OnPropertyChanged("TargetsFilePath");
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
            }
        }

        #endregion

        #region Public Methods

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
