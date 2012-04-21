using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using Sipper.Model;

namespace Sipper.ViewModel
{
    public class ManualViewingWithoutRawDataViewModel:ViewModelBase
    {

        private TargetedResultRepository _resultRepositorySource;

        #region Constructors

        public ManualViewingWithoutRawDataViewModel()
        {
            Results = new ObservableCollection<ResultWithImageInfo>();
            FileInputs = new FileInputsViewModel(new FileInputsInfo());
        }

        public ManualViewingWithoutRawDataViewModel(TargetedResultRepository resultRepository)
            : this()
        {
            _resultRepositorySource = resultRepository;

        }


        #endregion

        #region Properties
        public ObservableCollection<ResultWithImageInfo> Results { get; set; }

       


        private string _targetsFileStatusText;
        public string TargetsFileStatusText
        {
            get { return _targetsFileStatusText; }
            set
            {
                if (value == TargetsFileStatusText) return;
                _targetsFileStatusText = value;
                OnPropertyChanged("TargetsFileStatusText");
            }
        }


        private string _generalStatusMessage;
        public string GeneralStatusMessage
        {
            get { return _generalStatusMessage; }
            set
            {
                _generalStatusMessage = value;
                { OnPropertyChanged("GeneralStatusMessage"); }

            }
        }

        public FileInputsViewModel FileInputs { get; private set; }


        private string _resultImagesFolderPath;
        private List<string> _imageFilePaths;

        public string ResultImagesFolderPath
        {
            get { return _resultImagesFolderPath; }
            set
            {
                if (value == _resultImagesFolderPath) return;
                
                _resultImagesFolderPath = value;

                GetImageFileReferences(_resultImagesFolderPath);
                OnPropertyChanged("ResultImagesFolderPath");
            }
        }

        private void GetImageFileReferences(string resultImagesFolderPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(resultImagesFolderPath);
            if (!directoryInfo.Exists) return;


            _imageFilePaths = directoryInfo.GetFiles("*.png").Select(p => p.FullName).ToList();

        }

        #endregion

        #region Public Methods

        public void LoadResults(string resultFile)
        {
            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(resultFile);
            _resultRepositorySource = importer.Import();

            SetResults();
            TargetsFileStatusText = Results.Count + " loaded.";

        }

        public void SetResults()
        {
            var query = (from n in _resultRepositorySource.Results select (SipperLcmsFeatureTargetedResultDTO)n);

            Results.Clear();
            foreach (var resultDto in query)
            {
                

                ResultWithImageInfo resultWithImageInfo = new ResultWithImageInfo(resultDto);

                MapResultToImage(resultWithImageInfo);

                Results.Add(resultWithImageInfo);
            }

        }

        private void MapResultToImage(ResultWithImageInfo resultWithImageInfo)
        {

            string baseFileName = _resultImagesFolderPath + Path.DirectorySeparatorChar +
                                  resultWithImageInfo.Result.DatasetName + "_ID" + resultWithImageInfo.Result.TargetID;

            string expectedMSImageFilename = baseFileName + "_MS.png";
            string expectedChromImageFilename = baseFileName + "_chrom.png";
            string expectedTheorMSImageFilename = baseFileName + "_theorMS.png";


            resultWithImageInfo.MSImageFilePath = expectedMSImageFilename;
            resultWithImageInfo.ChromImageFilePath = expectedChromImageFilename;
            resultWithImageInfo.TheorMSImageFilePath = expectedTheorMSImageFilename;
        }

        public void SaveResults()
        {



            try
            {
                var exporter = new SipperResultToLcmsFeatureExporter(FileInputs.ResultsSaveFilePath);
                exporter.ExportResults(_resultRepositorySource.Results);
            }
            catch (Exception ex)
            {
                GeneralStatusMessage = "Error saving results. Error message: " + ex.Message;
                throw;
            }

            GeneralStatusMessage = "Results saved to: " + Path.GetFileName(FileInputs.ResultsSaveFilePath);


        }

     

        #endregion

        #region Private Methods

        #endregion

    }
}
