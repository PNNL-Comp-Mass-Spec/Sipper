using System;
using System.IO;
using PRISM;

namespace Sipper_Console
{
    /// -------------------------------------------------------------------------------
    /// This program runs SIPPER
    ///
    /// Written by Matthew Monroe for the Department of Energy (PNNL, Richland, WA)
    /// Copyright 2020, Battelle Memorial Institute.  All Rights Reserved.
    ///
    /// E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov
    /// Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics
    /// -------------------------------------------------------------------------------
    class Program
    {
        public const string PROGRAM_DATE = "June 2, 2020";

        private static DateTime mLastProgressTime;

        /// <summary>
        /// Main function
        /// </summary>
        /// <returns>0 if no error, error code if an error</returns>
        /// <remarks>The STAThread attribute is required for OxyPlot functionality</remarks>
        [STAThread]
        static int Main(string[] args)
        {
            var exeName = System.Reflection.Assembly.GetEntryAssembly()?.GetName().Name;
            var exePath = PRISM.FileProcessor.ProcessFilesOrDirectoriesBase.GetAppPath();
            var cmdLineParser = new CommandLineParser<SipperOptions>(exeName, GetAppVersion())
            {
                ProgramInfo = "This program runs SIPPER",
                ContactInfo = "Program written by Gordon Slysz and Matthew Monroe for PNNL (Richland, WA)" + Environment.NewLine +
                              "E-mail: matthew.monroe@pnnl.gov or proteomics@pnnl.gov" + Environment.NewLine +
                              "Website: https://github.com/PNNL-Comp-Mass-Spec/ or https://panomics.pnnl.gov/ or https://www.pnnl.gov/integrative-omics"
            };


            cmdLineParser.UsageExamples.Add("Program syntax:" + Environment.NewLine + Path.GetFileName(exePath) +
                                            " /I:DatasetFileOrDirectoryPath /P:ParameterFilePath /T:TargetsFilePath");

            var result = cmdLineParser.ParseArgs(args);
            var options = result.ParsedResults;
            if (!result.Success || !options.Validate())
            {
                // Delay for 750 msec in case the user double clicked this file from within Windows Explorer (or started the program via a shortcut)
                System.Threading.Thread.Sleep(750);
                return -1;
            }

            mLastProgressTime = DateTime.UtcNow;

            try
            {
                var processor = new SipperRunner(options);
                processor.DebugEvent += Processor_DebugEvent;
                processor.ErrorEvent += Processor_ErrorEvent;
                processor.WarningEvent += Processor_WarningEvent;
                processor.StatusEvent += Processor_MessageEvent;
                processor.ProgressUpdate += Processor_ProgressUpdate;

                var success = processor.RunSipper();

                if (success)
                    return 0;

                ConsoleMsgUtils.ShowWarning("RunSipper returned false");

                return 0;
            }
            catch (Exception ex)
            {
                ShowErrorMessage("Error occurred in Program->Main", ex);
                return -1;
            }

        }

        private static string GetAppVersion()
        {
            return PRISM.FileProcessor.ProcessFilesOrDirectoriesBase.GetAppVersion(PROGRAM_DATE);
        }

        private static void ShowErrorMessage(string message, Exception ex = null)
        {
            ConsoleMsgUtils.ShowError(message, ex);
        }


        private static void Processor_DebugEvent(string message)
        {
            ConsoleMsgUtils.ShowDebug(message);
        }

        private static void Processor_ErrorEvent(string message, Exception ex)
        {
            ConsoleMsgUtils.ShowErrorCustom(message, ex, false);
        }

        private static void Processor_MessageEvent(string message)
        {
            Console.WriteLine(message);
        }

        private static void Processor_ProgressUpdate(string progressMessage, float percentComplete)
        {
            if (DateTime.UtcNow.Subtract(mLastProgressTime).TotalSeconds < 5)
                return;

            Console.WriteLine();
            mLastProgressTime = DateTime.UtcNow;
            Processor_DebugEvent(percentComplete.ToString("0.0") + "%, " + progressMessage);
        }

        private static void Processor_WarningEvent(string message)
        {
            ConsoleMsgUtils.ShowWarning(message);
        }

    }
}
