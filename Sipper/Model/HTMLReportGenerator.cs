using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.UI;
using DeconTools.Utilities;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Results;

namespace Sipper.Model
{
    public class HTMLReportGenerator
    {

        private TargetedResultRepository _resultRepository;
        private FileInputsInfo _fileInputsInfo;
        private List<string> _imageFilePaths;

        private List<ResultWithImageInfo> _resultsWithImageInfo = new List<ResultWithImageInfo>();
        private const int MSImageWidth = 400;
        private const int MSImageHeight = 350;

        private const int TheorMSImageWidth = 400;
        private const int TheorMSImageHeight = 350;

        private const int ChromImageWidth = 275;
        private const int ChromImageHeight = 350;


        #region Constructors

        public HTMLReportGenerator(TargetedResultRepository resultRepository, FileInputsInfo fileInputs)
        {
            _resultRepository = resultRepository;
            _fileInputsInfo = fileInputs;

            Check.Require(_fileInputsInfo != null, "FileInputs object is null");
            Check.Require(_resultRepository != null, "Results repository object is null");

            MapResultsToImages();

        }


        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public void GenerateHTMLReport()
        {
            StringBuilder stringBuilder=new StringBuilder();

            stringBuilder.Append(CreateHeaderHTML());
            

            foreach (var resultWithImageInfo in _resultsWithImageInfo)
            {
                stringBuilder.Append(getHTMLForTableRow(resultWithImageInfo));
            }

            stringBuilder.Append(CloseHTMLTags());

            string outputHTMLFilename = _fileInputsInfo.ResultImagesFolderPath + Path.DirectorySeparatorChar + "0_index.html";
            
            using (StreamWriter sw = new StreamWriter(outputHTMLFilename))
            {
                sw.Write(stringBuilder.ToString());

            }

        }


        #endregion

        #region Private Methods
        private string CreateHeaderHTML()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\">");

            sb.Append("<html>");
            sb.Append("<body>");
            sb.Append("<table>");

            return sb.ToString();
        }

        private string CloseHTMLTags()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("</table>");
            sb.Append("</body>");
            sb.Append("</html>");
            return sb.ToString();
        }


        private string getHTMLForTableRow(ResultWithImageInfo resultWithImageInfo)
        {
            StringWriter stringWriter = new StringWriter();
            using (HtmlTextWriter writer = new HtmlTextWriter(stringWriter))
            {
                writer.RenderBeginTag(HtmlTextWriterTag.Tr);

                //add feature info table to a cell within the table

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                addResultInfoTable(writer, resultWithImageInfo);
                writer.RenderEndTag();

                addHTMLForChromImage(writer, resultWithImageInfo);

                addHTMLForTheorMSImage(writer, resultWithImageInfo);

                addHTMLForMSImage(writer,resultWithImageInfo);

                writer.RenderBeginTag(HtmlTextWriterTag.Td);
                addAnnotationTable(writer, resultWithImageInfo);
                writer.RenderEndTag();

                //this is a hook so that sub-classes can add additional stuff here
                //addHTMLForOtherData(writer);
                writer.RenderEndTag();

            }
            return stringWriter.ToString();
        }

        private void addAnnotationTable(HtmlTextWriter writer, ResultWithImageInfo resultWithImageInfo)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Style, "color:black; font-style:bold; font-family:Calibri; font-size:125%");
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "100");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (resultWithImageInfo.Result.ValidationCode== ValidationCode.None)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "2");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, "Black");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "Solid");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "White");

            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("None");
            writer.RenderEndTag();
            writer.RenderEndTag();


            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (resultWithImageInfo.Result.ValidationCode == ValidationCode.Yes)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "2");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, "Black");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "Solid");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "LightGreen");

            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Yes");
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (resultWithImageInfo.Result.ValidationCode == ValidationCode.No)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "2");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, "Black");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "Solid");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "Tomato");

            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("No");
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            if (resultWithImageInfo.Result.ValidationCode == ValidationCode.Maybe)
            {
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderWidth, "2");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderColor, "Black");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BorderStyle, "Solid");
                writer.AddStyleAttribute(HtmlTextWriterStyle.BackgroundColor, "LightYellow");

            }
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Maybe");
            writer.RenderEndTag();
            writer.RenderEndTag();


            writer.RenderEndTag();



        }


        private void addResultInfoTable(HtmlTextWriter writer, ResultWithImageInfo resultWithImageInfo)
        {
            writer.AddAttribute(HtmlTextWriterAttribute.Style, "color:firebrick; font-style:bold; font-family:Calibri; font-size:100%");
            writer.AddAttribute(HtmlTextWriterAttribute.Width, "200");
            writer.RenderBeginTag(HtmlTextWriterTag.Table);

            
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);

            


            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("TargetID");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.TargetID);
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("MassTagID");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.MatchedMassTagID);
            writer.RenderEndTag();
            writer.RenderEndTag();

   

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Scan");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.ScanLC);
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Intensity");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.Intensity);
            writer.RenderEndTag();
            writer.RenderEndTag();


            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("MonoMass");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.MonoMass.ToString("0.0000"));
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("MonoMZ");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.MonoMZ.ToString("0.0000"));
            writer.RenderEndTag();
            writer.RenderEndTag();

           
            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("z");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.ChargeState);
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("i_score");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.IScore);
            writer.RenderEndTag();
            writer.RenderEndTag();

            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("RawAreaMetric");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.AreaUnderRatioCurve.ToString("0.0"));
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("RevisedArea");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.AreaUnderRatioCurveRevised.ToString("0.0"));
            writer.RenderEndTag();
            writer.RenderEndTag();


            writer.RenderBeginTag(HtmlTextWriterTag.Tr);
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("Linearity");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.RSquaredValForRatioCurve.ToString("0.0"));
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write("ChromCorr_Med");
            writer.RenderEndTag();
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.Write(resultWithImageInfo.Result.ChromCorrelationMedian.ToString("0.000"));
            writer.RenderEndTag();
            writer.RenderEndTag();
            
            writer.RenderEndTag();
        }


        protected void addHTMLForMSImage(HtmlTextWriter writer, ResultWithImageInfo resultWithImageInfo)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, Path.GetFileName(resultWithImageInfo.MSImageFilePath));
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.AddAttribute(HtmlTextWriterAttribute.Src, Path.GetFileName(resultWithImageInfo.MSImageFilePath));
            writer.AddAttribute(HtmlTextWriterAttribute.Width, MSImageWidth.ToString("0"));
            writer.AddAttribute(HtmlTextWriterAttribute.Height, MSImageHeight.ToString("0"));
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }


        protected void addHTMLForChromImage(HtmlTextWriter writer, ResultWithImageInfo resultWithImageInfo)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, Path.GetFileName(resultWithImageInfo.ChromImageFilePath));
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.AddAttribute(HtmlTextWriterAttribute.Src, Path.GetFileName(resultWithImageInfo.ChromImageFilePath));
            writer.AddAttribute(HtmlTextWriterAttribute.Width, ChromImageWidth.ToString("0"));
            writer.AddAttribute(HtmlTextWriterAttribute.Height, ChromImageHeight.ToString("0"));
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }


        protected void addHTMLForTheorMSImage(HtmlTextWriter writer, ResultWithImageInfo resultWithImageInfo)
        {
            writer.RenderBeginTag(HtmlTextWriterTag.Td);
            writer.AddAttribute(HtmlTextWriterAttribute.Href, Path.GetFileName(resultWithImageInfo.TheorMSImageFilePath));
            writer.RenderBeginTag(HtmlTextWriterTag.A);
            writer.AddAttribute(HtmlTextWriterAttribute.Src, Path.GetFileName(resultWithImageInfo.TheorMSImageFilePath));
            writer.AddAttribute(HtmlTextWriterAttribute.Width, TheorMSImageWidth.ToString("0"));
            writer.AddAttribute(HtmlTextWriterAttribute.Height, TheorMSImageHeight.ToString("0"));
            writer.RenderBeginTag(HtmlTextWriterTag.Img);
            writer.RenderEndTag();
            writer.RenderEndTag();
            writer.RenderEndTag();
        }




        private void MapResultsToImages()
        {
            var query = (from n in _resultRepository.Results select (SipperLcmsFeatureTargetedResultDTO)n);

            _resultsWithImageInfo.Clear();
            foreach (var resultDto in query)
            {
                ResultWithImageInfo resultWithImageInfo = new ResultWithImageInfo(resultDto);
                _resultsWithImageInfo.Add(resultWithImageInfo);
            }

            foreach (var result in _resultsWithImageInfo)
            {
                string baseFileName = _fileInputsInfo.ResultImagesFolderPath + Path.DirectorySeparatorChar +
                                 result.Result.DatasetName + "_ID" + result.Result.TargetID;

                string expectedMSImageFilename = baseFileName + "_MS.png";
                string expectedChromImageFilename = baseFileName + "_chrom.png";
                string expectedTheorMSImageFilename = baseFileName + "_theorMS.png";

                result.MSImageFilePath = expectedMSImageFilename;
                result.ChromImageFilePath = expectedChromImageFilename;
                result.TheorMSImageFilePath = expectedTheorMSImageFilename;
            }


        }


        private void GetImageFileReferences()
        {
            Check.Require(!string.IsNullOrEmpty(_fileInputsInfo.ResultImagesFolderPath),
                          "Result images folder path is null");


            DirectoryInfo directoryInfo = new DirectoryInfo(_fileInputsInfo.ResultImagesFolderPath);

            if (directoryInfo.Exists)
            {
                _imageFilePaths = directoryInfo.GetFiles("*.png").Select(p => p.FullName).ToList();
            }
            else
            {
                throw new DirectoryNotFoundException("ResultImages folder not found.");
            }








        }

        #endregion

    }
}
