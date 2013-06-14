using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FigureXX_QuantificationAnalysis
    {
        [Test]
        public void GetQuantStatsOnYellowC13_070()
        {
            string originalResultsFile =
  @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_ALL_validated.txt";

            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(originalResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();


            string officialResultsFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Results_2013_04_09\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";
            importer = new SipperResultFromTextImporter(officialResultsFile);
            var officalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var yesIDs = yesResultsOnly.Select(p => p.TargetID).ToList();
            var labeledResults = (from n in officalResults where yesIDs.Contains(n.TargetID) select n).ToList();

            string outputFilename =
               @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_QuantificationAnalysis\DataForQuantAnalysis.txt";
            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(outputFilename);
            exporter.ExportResults(labeledResults);


        }



        [Category("Paper")]
        [Test]
        public void GetFilterStatsOnAllResults()
        {



            var datasetnames = SipperDatasetUtilities.GetDatasetNames();   //.Where(p => p.Contains("Yellow_C13_070")).ToList();

            string resultFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Results_2013_04_09";

            StringBuilder sb = new StringBuilder();
            List<SipperLcmsFeatureTargetedResultDTO> allResults = new List<SipperLcmsFeatureTargetedResultDTO>();


            foreach (var datasetname in datasetnames)
            {
                string expectedResultFile = resultFolder + "\\" + datasetname + "_results.txt";

                FileInfo fileInfo = new FileInfo(expectedResultFile);

                sb.Append(datasetname);
                sb.Append("\t");

                if (fileInfo.Exists)
                {
                    SipperResultFromTextImporter importer = new SipperResultFromTextImporter(fileInfo.FullName);
                    var results = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

                    var tightFilterResults = SipperFilters.ApplyAutoValidationCodeF1TightFilter(results);

                    var numDetected = tightFilterResults.Count(p => p.ValidationCode == ValidationCode.Yes);
                    sb.Append(numDetected);

                    allResults.AddRange(tightFilterResults);
                }
                else
                {
                    sb.Append("");

                }
                sb.Append(Environment.NewLine);


            }

            string outputFilename =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_QuantificationAnalysis\DataForQuantAnalysis_allResults.txt";
            SipperResultToLcmsFeatureExporter exporter = new SipperResultToLcmsFeatureExporter(outputFilename);
            exporter.ExportResults(allResults.Where(p=>p.ValidationCode==ValidationCode.Yes));

            Console.WriteLine(sb.ToString());



        }


    }
}
