using System;
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
            catch (Exception)
            {
                throw;
            }

            if (isDir)
            {
                DatasetPath = fileOrFolderPath;
            }
            else
            {
                var fileExtension = Path.GetExtension(fileOrFolderPath);

                if (fileExtension != null)
                {
                    fileExtension = fileExtension.ToLower();

                    if (fileExtension == ".xml")
                    {
                        ParameterFilePath = fileOrFolderPath;
                    }
                    else if (fileExtension == ".txt")
                    {
                        TargetsFilePath = fileOrFolderPath;
                    }
                    else
                    {
                        DatasetPath = fileOrFolderPath;
                    }


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
