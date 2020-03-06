using System.IO;
using PRISM;

namespace Sipper_Console
{
    public class SipperOptions
    {
        [Option("I", "Input", ArgPosition = 1, Required = true, HelpShowsDefault = false, IsInputFilePath = true,
            HelpText = "Dataset file (or directory) path")]
        public string DatasetFilePath { get; set; }

        [Option("P", "Param", ArgPosition = 2, Required = true, HelpShowsDefault = false, IsInputFilePath = true,
            HelpText = "Parameter file path")]
        public string ParameterFilePath { get; set; }

        [Option("T", "Targets", ArgPosition = 3, Required = true, HelpShowsDefault = false, IsInputFilePath = true,
            HelpText = "Targets file path")]
        public string TargetsFilePath { get; set; }

        [Option("O", "Output", Required = false, HelpShowsDefault = false,
            HelpText = "Output file path for saving results")]
        public string ResultsFilePath { get; set; }

        [Option("Plots", Required = false, HelpShowsDefault = false,
            HelpText = "Output directory for result images")]
        public string PlotDirectoryPath { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SipperOptions()
        {
            DatasetFilePath = string.Empty;
            ParameterFilePath = string.Empty;
            TargetsFilePath = string.Empty;
            ResultsFilePath = string.Empty;
            PlotDirectoryPath = string.Empty;
        }

        private void AutoDefineResultsFilePath()
        {
            if (string.IsNullOrWhiteSpace(TargetsFilePath))
                return;

            if (!string.IsNullOrWhiteSpace(ResultsFilePath))
                return;

            var targetsFile = new FileInfo(TargetsFilePath);

            var baseFileName = Path.GetFileNameWithoutExtension(targetsFile.Name);

            var workingDirectoryPath = targetsFile.Directory != null ? targetsFile.Directory.FullName : string.Empty;

            ResultsFilePath = Path.Combine(workingDirectoryPath, baseFileName + "_validated.txt");
        }

        public bool Validate()
        {
            if (!FileExists(DatasetFilePath, "Dataset file"))
            {
                return false;
            }

            if (!FileExists(ParameterFilePath, "Parameter file"))
            {
                return false;
            }

            if (!FileExists(TargetsFilePath, "Targets file"))
            {
                return false;
            }

            AutoDefineResultsFilePath();

            return true;
        }

        private bool FileExists(string filePath, string fileDescription)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                ConsoleMsgUtils.ShowWarning("ERROR: {0} must be provided and non-empty", fileDescription);
                return false;
            }

            var dataFile = new FileInfo(filePath);
            if (!dataFile.Exists)
            {
                ConsoleMsgUtils.ShowWarning("ERROR: {0} not found: {1}", fileDescription, dataFile.FullName);
                return false;
            }

            return true;
        }
    }
}