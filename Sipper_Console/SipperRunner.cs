﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.Results;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PRISM;
using Sipper.Model;
using Globals = DeconTools.Workflows.Backend.Globals;

namespace Sipper_Console
{

    public class SipperRunner : PRISM.EventNotifier
    {
        #region "Member variables"

        private BasicTargetedWorkflowExecutor mWorkflowExecutor;

        private readonly Project mSipperProject;

        #endregion

        #region "Properties"

        private SipperWorkflowExecutorParameters ExecutorParameters { get; }

        private int PercentProgress { get; set; }

        public List<TargetedWorkflowExecutorProgressInfo> ProgressInfos { get; }

        private SipperOptions Options { get; }

        private Run Run { get; set; }

        private SipperTargetedWorkflowParameters SipperWorkflowParameters { get; }

        private string StatusMessage { get; set; } = string.Empty;

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        public SipperRunner(SipperOptions options, Project sipperProject = null)
        {
            Options = options;

            //private readonly FileInputsInfo mFileInputsInfo;
            //mFileInputsInfo = new FileInputsInfo(options.DatasetFilePath, options.ParameterFilePath, options.TargetsFilePath)
            //{
            //    ResultsSaveFilePath = options.ResultsFilePath, ResultImagesFolderPath = options.PlotDirectoryPath
            //};

            ExecutorParameters = new SipperWorkflowExecutorParameters
            {
                TargetType = Globals.TargetType.LcmsFeature
            };

            SipperWorkflowParameters = new SipperTargetedWorkflowParameters();

            ProgressInfos = new List<TargetedWorkflowExecutorProgressInfo>();

            if (sipperProject == null)
            {
                sipperProject = new Project();
            }

            mSipperProject = sipperProject;

            if (mSipperProject.ResultRepository == null)
            {
                mSipperProject.ResultRepository = new TargetedResultRepository();
            }
            else
            {
                mSipperProject.ResultRepository.Clear();
            }
        }

        public bool RunSipper()
        {
            try
            {

                ExecutorParameters.TargetsFilePath = Options.TargetsFilePath;
                ExecutorParameters.WorkflowParameterFile = Options.ParameterFilePath;

                if (!string.IsNullOrEmpty(Options.DatasetFilePath))
                {
                    ExecutorParameters.OutputDirectoryBase = RunUtilities.GetDatasetParentDirectory(Options.DatasetFilePath);
                }
                else
                {
                    ExecutorParameters.OutputDirectoryBase = string.Empty;
                }

                PercentProgress = 0;
                ProgressInfos.Clear();

                // Uncomment to use threading
                //var success = RunSipperThreaded();

                var success = RunSipperNonThreaded();

                return success;
            }
            catch (Exception ex)
            {
                OnErrorEvent("Error running SIPPER", ex);
                return false;
            }
        }

        private bool RunSipperNonThreaded()
        {

            try
            {
                var worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = false,
                    WorkerReportsProgress = true
                };

                worker.ProgressChanged += BackgroundWorker_ProgressChanged;
                worker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

                mWorkflowExecutor = new BasicTargetedWorkflowExecutor(ExecutorParameters, Options.DatasetFilePath, worker)
                {
                    RunIsDisposed = false
                };

                mWorkflowExecutor.Execute();

                mSipperProject.ResultRepository.Results.Clear();

                var results = mWorkflowExecutor.GetResults();

                mSipperProject.ResultRepository.Results.AddRange(results);

                Run = mWorkflowExecutor.Run;

                return true;
            }
            catch (Exception ex)
            {
                OnErrorEvent("Error in RunSipperNonThreaded", ex);
                return false;
            }

        }

        private bool RunSipperThreaded()
        {
            try
            {
                var worker = new BackgroundWorker
                {
                    WorkerSupportsCancellation = true,
                    WorkerReportsProgress = true
                };

                worker.DoWork += BackgroundWorker_DoWork;
                worker.ProgressChanged += BackgroundWorker_ProgressChanged;
                worker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;
                worker.RunWorkerAsync();

                while (PercentProgress < 100)
                {
                    System.Threading.Thread.Sleep(250);
                }

                return true;
            }
            catch (Exception ex)
            {
                OnErrorEvent("Error in RunSipperNonThreaded", ex);
                return false;
            }
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;

            try
            {
                mWorkflowExecutor = new BasicTargetedWorkflowExecutor(ExecutorParameters, Options.DatasetFilePath, worker)
                {
                    RunIsDisposed = false
                };

                mWorkflowExecutor.Execute();

                mSipperProject.ResultRepository.Results.Clear();

                var results = mWorkflowExecutor.GetResults();

                mSipperProject.ResultRepository.Results.AddRange(results);

                if (worker.CancellationPending)
                {
                    e.Cancel = true;
                }

                Run = mWorkflowExecutor.Run;
            }
            catch (Exception ex)
            {
                OnErrorEvent("Error in BackgroundWorker_DoWork", ex);
            }

        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                StatusMessage = "Cancelled";
            }
            else if (e.Error != null)
            {
                StatusMessage = "Error - check log file or results output";
            }
            else
            {
                StatusMessage = "Processing COMPLETE. #chromatograms extracted=?";
                PercentProgress = 100;
            }
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
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
                                OnStatusEvent(infoString);
                                StatusMessage = infoString;
                            }
                        }
                    }
                    else
                    {
                        var sipperResult = (SipperLcmsFeatureTargetedResultDTO)ResultDTOFactory.CreateTargetedResult(info.Result);
                        SipperFilters.ApplyAutoValidationCodeF2LooseFilter(sipperResult);
                        if (sipperResult.ValidationCode == ValidationCode.Yes)
                        {
                            ProgressInfos.Add(info);
                            OnProgressUpdate(info.ProgressInfoString, 0);
                        }
                    }
                }
                else if (e.UserState is PeakProgressInfo peakProgress)
                {

                    OnStatusEvent(string.Format("{0}: {1}%", peakProgress.ProgressInfoString.Trim(), e.ProgressPercentage));
                }
                else
                {
                    Console.WriteLine(e.UserState);
                }
            }
        }

    }
}
