using System;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;
using Sipper.Scripts.SipperPaper;

namespace Sipper.Scripts
{
    [TestFixture]
    public class EricHotLakesScripts
    {

        
        [Test]
        public void GetFilterStatsOnAllResults()
        {
            var datasetnames = SipperDatasetUtilities.GetDatasetNames();




            string resultFolder = @"\\protoapps\DataPkgs\Public\2013\788_Sipper_C13_Analysis_Hot_Lake_SNC_Ana_preliminary\Eric_Results";

            var dirinfo = new DirectoryInfo(resultFolder);

            var fileInfos = dirinfo.GetFiles("*_results.txt");

            StringBuilder sb = new StringBuilder();

            foreach (var fileInfo in fileInfos)
            {
           

                if (fileInfo.Exists)
                {

                    SipperResultFromTextImporter importer = new SipperResultFromTextImporter(fileInfo.FullName);
                    var results = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

                    var tightFilterResults = SipperFilters.ApplyAutoValidationCodeF1TightFilter(results);

                    int countTightFilterResults = tightFilterResults.Count(p => p.ValidationCode == ValidationCode.Yes);

                    var looseFilteredResults = SipperFilters.ApplyAutoValidationCodeF2LooseFilter(results);
                    int countLooseFilter = looseFilteredResults.Count(p => p.ValidationCode == ValidationCode.Yes);

                    sb.Append(fileInfo.Name);
                    sb.Append("\t");
                    sb.Append(tightFilterResults.Count);
                    sb.Append("\t");
                    sb.Append(countTightFilterResults);
                    sb.Append("\t");
                    sb.Append(countLooseFilter);
                }
                else
                {
                    sb.Append("");

                }
                sb.Append(Environment.NewLine);


            }
            Console.WriteLine(sb.ToString());


        }


        [Test]
        public void OutputFilteredResultsFiles_TightFilter()
        {

            string resultFolder = @"\\protoapps\DataPkgs\Public\2013\794_Sipper_C13_Analysis_Hot_Lake_SNC_Ana_preliminary_v2\Results\2013_05_13_results";

            var dirinfo = new DirectoryInfo(resultFolder);

            var fileInfos = dirinfo.GetFiles("*_results.txt");

            StringBuilder sb = new StringBuilder();

            foreach (var fileInfo in fileInfos)
            {
                if (fileInfo.Exists)
                {

                    SipperResultFromTextImporter importer = new SipperResultFromTextImporter(fileInfo.FullName);
                    var results = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

                    var tightFilterResults = SipperFilters.ApplyAutoValidationCodeF1TightFilter(results).Where(p=>p.ValidationCode==ValidationCode.Yes).ToList();

                    var looseFilterResults = SipperFilters.ApplyAutoValidationCodeF2LooseFilter(results).Where(p => p.ValidationCode == ValidationCode.Yes).ToList();

                    string outputFilename = fileInfo.FullName.Replace("_results.txt", "_tightFilter_results.txt");

                    SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(outputFilename);
                    exporter.ExportResults(tightFilterResults);

                    outputFilename = fileInfo.FullName.Replace("_results.txt", "_looseFilter_results.txt");
                    exporter = new SipperResultToLcmsFeatureExporter(outputFilename);
                    exporter.ExportResults(looseFilterResults);

                    sb.Append(fileInfo.Name);
                    sb.Append("\t");
                    sb.Append(tightFilterResults.Count);
                    sb.Append("\t");
                }
                else
                {
                    sb.Append("");

                }
                sb.Append(Environment.NewLine);


            }
            Console.WriteLine(sb.ToString());


        }


    }
}
