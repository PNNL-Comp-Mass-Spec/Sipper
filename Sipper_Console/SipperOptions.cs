using System.IO;
using PRISM;

namespace Sipper_Console
{
    public class SipperOptions
    {
        [Option("Input", "Dataset", "I", ArgPosition = 1, Required = true, HelpShowsDefault = false, IsInputFilePath = true,
            HelpText = "Dataset file (or directory) path")]
        public string DatasetFilePath { get; set; }

        [Option("Param", "P", ArgPosition = 2, Required = true, HelpShowsDefault = false, IsInputFilePath = true,
            HelpText = "SIPPER parameter file path")]
        public string ParameterFilePath { get; set; }

        [Option("Targets", "T", ArgPosition = 3, Required = true, HelpShowsDefault = false, IsInputFilePath = true,
            HelpText = "Targets file path")]
        public string TargetsFilePath { get; set; }

        [Option("Output", "O", Required = false, HelpShowsDefault = false,
            HelpText = "Output directory for saving results")]
        public string OutputDirectoryPath { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public SipperOptions()
        {
            DatasetFilePath = string.Empty;
            ParameterFilePath = string.Empty;
            TargetsFilePath = string.Empty;
            OutputDirectoryPath = string.Empty;
        }

        private void AutoDefineBaseOutputDirectory()
        {
            if (string.IsNullOrWhiteSpace(TargetsFilePath))
                return;

            if (!string.IsNullOrWhiteSpace(OutputDirectoryPath))
                return;

            var targetsFile = new FileInfo(TargetsFilePath);

            OutputDirectoryPath = targetsFile.Directory != null ? targetsFile.Directory.FullName : string.Empty;
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

            AutoDefineBaseOutputDirectory();

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