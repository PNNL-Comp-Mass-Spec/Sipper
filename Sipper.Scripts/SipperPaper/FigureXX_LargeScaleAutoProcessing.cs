using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.FileIO;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using GwsDMSUtilities;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FigureXX_LargeScaleAutoProcessing
    {

        [Category("Final")]
        [Test]
        public void CheckForResults()
        {
            var datasetnames = SipperDatasetUtilities.GetDatasetNames();

            string resultFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results";

            StringBuilder sb = new StringBuilder();

            foreach (var datasetname in datasetnames)
            {
                string expectedResultFile = resultFolder + "\\" + datasetname + "_results.txt";

                FileInfo fileInfo = new FileInfo(expectedResultFile);
                sb.Append(datasetname);
                sb.Append("\t");

                if (fileInfo.Exists)
                {
                    sb.Append("TRUE");
                    sb.Append("\t");
                    sb.Append((double)fileInfo.Length / 1000);
                    sb.Append("\t");
                    sb.Append(fileInfo.LastWriteTime.ToShortDateString());
                }
                else
                {
                    sb.Append("FALSE");

                }
                sb.Append(Environment.NewLine);


            }
            Console.WriteLine(sb.ToString());

        }

        [Category("Final")]
        [Test]
        public void GetFilterStatsOnAllResults()
        {
            var datasetnames = SipperDatasetUtilities.GetDatasetNames();

            string resultFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results";

            StringBuilder sb = new StringBuilder();

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

                    int countTightFilterResults = tightFilterResults.Count(p => p.ValidationCode == ValidationCode.Yes);

                    var looseFilteredResults = SipperFilters.ApplyAutoValidationCodeF2LooseFilter(results);
                    int countLooseFilter = looseFilteredResults.Count(p => p.ValidationCode == ValidationCode.Yes);

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
        public void GetMassTagsForTightFilteredPeptides()
        {
            var datasetnames = SipperDatasetUtilities.GetDatasetNames();

            string resultFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Original";

            StringBuilder sb = new StringBuilder();

            List<int> enrichedMassTags = new List<int>();

            foreach (var datasetname in datasetnames)
            {
                string expectedResultFile = resultFolder + "\\" + datasetname + "_results.txt";

                FileInfo fileInfo = new FileInfo(expectedResultFile);


                if (fileInfo.Exists)
                {

                    SipperResultFromTextImporter importer = new SipperResultFromTextImporter(fileInfo.FullName);
                    var results = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

                    var tightFilteredResults =
                        SipperFilters.ApplyAutoValidationCodeF1TightFilter(results).Where(p => p.ValidationCode == ValidationCode.Yes).
                            ToList();


                    var massTagIDs = tightFilteredResults.Select(p => p.MatchedMassTagID).ToList();

                    enrichedMassTags.AddRange(massTagIDs);

                    foreach (var r in tightFilteredResults)
                    {
                        sb.Append(r.DatasetName + "\t" + r.TargetID + "\t" + r.MatchedMassTagID + "\t" + r.Intensity + "\t" + r.FitScoreLabeledProfile.ToString("0.000") + "\t" +  r.PercentCarbonsLabelled.ToString("0.0") + "\t" + r.PercentPeptideLabelled.ToString("0.0") + "\n");

                    }


                }
                else
                {


                }



            }
            Console.WriteLine(sb.ToString());


            string baseFolder = @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_largeScaleAutomatedAnalysis";

            string outputMassTagFilename = baseFolder + "\\" + "enriched_withProteinInfo_massTags.txt";

            string dbname = "MT_Yellowstone_Communities_P627";
            string serverName = "pogo";
            var infoExtractor = new PeptideInfoExtractor(serverName, dbname);

            var enrichedpeptides = infoExtractor.GetPeptideInfo(enrichedMassTags, true, true);

            DeconTools.Backend.FileIO.MassTagTextFileExporter exporter = new MassTagTextFileExporter(outputMassTagFilename);
            exporter.ExportResults(enrichedpeptides);

            MassTagUtilities massTagUtilities = new MassTagUtilities();
            massTagUtilities.ExportMassTagsWithJCVIInformation(outputMassTagFilename);


            outputMassTagFilename = baseFolder + "\\" + "enriched_unique_withProteinInfo_massTags.txt";
            enrichedpeptides = enrichedpeptides.Where(p => p.MultipleProteinCount == 0).ToList();
            exporter = new MassTagTextFileExporter(outputMassTagFilename);
            exporter.ExportResults(enrichedpeptides);
            massTagUtilities.ExportMassTagsWithJCVIInformation(outputMassTagFilename);


            Script_GetAminoAcidFrequencies.DisplayAminoAcidFrequencies(enrichedpeptides.Select(p => p.Code));

        }


        [Test]
        public void ExportAllMassTagsMatched()
        {
            //Gather all results from all datasets
            //


            var datasetnames = SipperDatasetUtilities.GetDatasetNames();

            string resultFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Original";

            StringBuilder sb = new StringBuilder();

            List<int> enrichedMassTags = new List<int>();
            List<int> allMassTags = new List<int>();

            foreach (var datasetname in datasetnames)
            {
                string expectedResultFile = resultFolder + "\\" + datasetname + "_results.txt";
                FileInfo fileInfo = new FileInfo(expectedResultFile);

                if (fileInfo.Exists)
                {
                    SipperResultFromTextImporter importer = new SipperResultFromTextImporter(fileInfo.FullName);
                    var results = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();
                    var tightFilteredResults = SipperFilters.ApplyAutoValidationCodeF1TightFilter(results).Where(p => p.ValidationCode == ValidationCode.Yes);

                    var enrichedMassTagIDs = tightFilteredResults.Select(p => p.MatchedMassTagID).ToList();
                    var massTagIDs = results.Select(p => p.MatchedMassTagID).ToList();

                    enrichedMassTags.AddRange(enrichedMassTagIDs);
                    allMassTags.AddRange(massTagIDs);
                }


            }

            string dbname = "MT_Yellowstone_Communities_P627";
            string serverName = "pogo";
            var infoExtractor = new PeptideInfoExtractor(serverName, dbname);

            //Console.WriteLine("----------------- enriched peptides -----------------");
            //var enrichedPeptides = infoExtractor.GetPeptideInfo(enrichedMassTags, true, true);
            //enrichedPeptides = enrichedPeptides.Where(p => p.MultipleProteinCount == 0).ToList();
            //Script_GetAminoAcidFrequencies.DisplayAminoAcidFrequencies(enrichedPeptides.Select(p => p.Code));

            
            var allPeptides = infoExtractor.GetPeptideInfo(allMassTags, false,true);
            string baseFolder = @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_largeScaleAutomatedAnalysis";
            string outputMassTagFilename = baseFolder + "\\" + "all_withProteinInfo_massTags.txt";



           



            DeconTools.Backend.FileIO.MassTagTextFileExporter exporter = new MassTagTextFileExporter(outputMassTagFilename);
            exporter.ExportResults(allPeptides);


            MassTagUtilities massTagUtilities = new MassTagUtilities();
            massTagUtilities.ExportMassTagsWithJCVIInformation(outputMassTagFilename);



            Console.WriteLine("----------------- all unique peptides -----------------");
            allPeptides = allPeptides.Where(p => p.MultipleProteinCount == 0).ToList();
            //Script_GetAminoAcidFrequencies.DisplayAminoAcidFrequencies(allPeptides.Select(p => p.Code));

            outputMassTagFilename = baseFolder + "\\" + "all_unique_withProteinInfo_massTags.txt";
            exporter = new MassTagTextFileExporter(outputMassTagFilename);
            exporter.ExportResults(allPeptides);

            massTagUtilities = new MassTagUtilities();
            massTagUtilities.ExportMassTagsWithJCVIInformation(outputMassTagFilename);

        }



        [Test]
        public void getOrganismInfo()
        {
            string outputMassTagFilename =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_largeScaleAutomatedAnalysis\all_MassTags.txt";

            MassTagUtilities massTagUtilities = new MassTagUtilities();
            massTagUtilities.ExportMassTagsWithJCVIInformation(outputMassTagFilename);
        }



        [Test]
        public void GetAminoAcidFrequecies_All_vs_enriched()
        {
            string allUniquePeptidesFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_largeScaleAutomatedAnalysis\unique_peptideSequencesOnly.txt";

            string enrichedUniquePeptidesFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_largeScaleAutomatedAnalysis\enriched_unique_peptideSequencesOnly.txt";


            Script_GetAminoAcidFrequencies.DisplayAminoAcidFrequencies(allUniquePeptidesFile);

            Script_GetAminoAcidFrequencies.DisplayAminoAcidFrequencies(enrichedUniquePeptidesFile);

        }



    }
}
