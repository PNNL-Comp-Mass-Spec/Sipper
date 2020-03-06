using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Sipper.Model;

namespace Sipper.ViewModel
{
    public class FileInputsViewModel : ViewModelBase
    {
        private readonly FileInputsInfo _fileInputsInfo;

        #region Constructors

        public FileInputsViewModel()
        {
            _fileInputsInfo = new FileInputsInfo();
        }

        public FileInputsViewModel(FileInputsInfo fileInputsInfo) : this()
        {
            _fileInputsInfo = fileInputsInfo ?? new FileInputsInfo();
        }
        #endregion

        #region Properties

        public ObservableCollection<string> DatasetPathCollection => _fileInputsInfo.DatasetPathsList as ObservableCollection<string>;

        public string DatasetParentFolder
        {
            get => _fileInputsInfo.DatasetDirectory;
            set
            {
                if (value == _fileInputsInfo.DatasetDirectory) return;
                _fileInputsInfo.DatasetDirectory = value;

                OnPropertyChanged("DatasetParentFolder");
            }
        }

        public string DatasetPath
        {
            get => _fileInputsInfo.DatasetPath;
            set
            {
                if (value == _fileInputsInfo.DatasetPath)
                    return;

                _fileInputsInfo.DatasetPath = value;

                OnPropertyChanged("DatasetPath");
            }
        }

        public string TargetsFilePath
        {
            get => _fileInputsInfo.TargetsFilePath;
            set
            {
                if (value == _fileInputsInfo.TargetsFilePath)
                    return;

                _fileInputsInfo.TargetsFilePath = value;

                SetSaveFilePath();

                OnPropertyChanged("TargetsFilePath");
            }
        }

        public string ResultImagesFolderPath
        {
            get => _fileInputsInfo.ResultImagesFolderPath;
            set
            {
                if (value == _fileInputsInfo.ResultImagesFolderPath)
                    return;

                _fileInputsInfo.ResultImagesFolderPath = value;
                OnPropertyChanged("ResultImagesFolderPath");
            }
        }

        public string ResultsSaveFilePath
        {
            get => _fileInputsInfo.ResultsSaveFilePath;
            set
            {
                if (value == _fileInputsInfo.ResultsSaveFilePath)
                    return;

                _fileInputsInfo.ResultsSaveFilePath = value;

                OnPropertyChanged("ResultsSaveFilePath");
            }
        }

        public string ParameterFilePath
        {
            get => _fileInputsInfo.ParameterFilePath;
            set
            {
                if (value == _fileInputsInfo.ParameterFilePath)
                    return;

                _fileInputsInfo.ParameterFilePath = value;
                OnPropertyChanged("ParameterFilePath");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Define the input files
        /// </summary>
        /// <param name="filePaths">List of input files</param>
        /// <remarks>
        /// Input files are typically:
        /// - a parameter file (.xml)
        /// - the targets file (.txt)
        /// - the dataset file (whatever is not .xml or .txt)
        /// </remarks>
        public void CreateFileLinkages(List<string> filePaths)
        {
            if (filePaths == null || !filePaths.Any())
            {
                return;
            }

            //pull out .xml file first
            var xmlFileNames = (from n in filePaths
                                where Path.GetExtension(n) != null && Path.GetExtension(n).ToLower() == ".xml"
                                select n).ToList();

            if (xmlFileNames.Any())
            {
                CreateFileLinkage(xmlFileNames.First());

                //remove all xml files from inputs
                filePaths = filePaths.Except(xmlFileNames).ToList();
            }

            var txtFileNames = (from n in filePaths
                                where Path.GetExtension(n) != null && Path.GetExtension(n) == ".txt"
                                select n).ToList();

            if (txtFileNames.Any())
            {
                CreateFileLinkage(txtFileNames.First());

                //remove all text files from inputs
                filePaths = filePaths.Except(txtFileNames).ToList();
            }

            if (filePaths.Any())
            {
                CreateFileLinkage(filePaths.First());
            }
        }

        /// <summary>
        /// Define an input file (based on the file extension)
        /// </summary>
        /// <param name="fileOrDirectoryPath"></param>
        /// <remarks>
        /// Parameter file extension: .xml
        /// Targets file extension:   .txt
        /// Dataset file:             anything else (can also be a directory)

        /// </remarks>
        public void CreateFileLinkage(string fileOrDirectoryPath)
        {

            var isDir = (File.GetAttributes(fileOrDirectoryPath) & FileAttributes.Directory)
                         == FileAttributes.Directory;

            if (isDir)
            {
                DatasetPath = fileOrDirectoryPath;
            }
            else
            {
                var fileExtension = Path.GetExtension(fileOrDirectoryPath).ToLower();

                if (fileExtension == ".xml")
                {
                    ParameterFilePath = fileOrDirectoryPath;
                }
                else if (fileExtension == ".txt")
                {
                    TargetsFilePath = fileOrDirectoryPath;
                }
                else
                {
                    DatasetPath = fileOrDirectoryPath;
                }
            }
        }

        public bool PathsAreValid()
        {

            bool datasetPathIsOK;
            if (Directory.Exists(DatasetPath))
            {
                datasetPathIsOK = true;
            }
            else if (File.Exists(DatasetPath))
            {
                datasetPathIsOK = true;
            }
            else
            {
                datasetPathIsOK = false;
            }

            var parameterFilePathIsOK = File.Exists(ParameterFilePath);

            var targetsFilePathIsOK = File.Exists(TargetsFilePath);

            if (datasetPathIsOK && parameterFilePathIsOK && targetsFilePathIsOK)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region Private Methods

        private void SetSaveFilePath()
        {
            if (string.IsNullOrEmpty(_fileInputsInfo.TargetsFilePath)) return;

            var sourceResultsFileName = Path.GetFileNameWithoutExtension(_fileInputsInfo.TargetsFilePath);
            var path = Path.GetDirectoryName(_fileInputsInfo.TargetsFilePath);

            var resultsSaveFileName = path + Path.DirectorySeparatorChar + sourceResultsFileName + "_validated.txt";
            ResultsSaveFilePath = resultsSaveFileName;
        }

        #endregion
    }
}
