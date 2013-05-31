using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Core;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using GwsDMSUtilities;
using NUnit.Framework;
using Sipper.Model;

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

        [Category("Paper")]
        [Test]
        public void GetFilterStatsOnAllResults()
        {
            var datasetnames = SipperDatasetUtilities.GetDatasetNames();

            //string resultFolder =
            //    @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Results_2013_04_09";

            string resultFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results";


            resultFolder = @"D:\Data\Sipper\HLP_Ana\Results";


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
        public void EditFastaSoItDoesntHaveCarriageReturns()
        {
            string fastaFile = @"D:\Data\Sipper\YSIP_High\MSGFResults\ID_002270_64B2CBBF - Copy.fasta";

            string outputFile = fastaFile.Replace(".fasta", "_edited.fasta");

            using (StreamReader reader = new StreamReader(fastaFile))
            {
                using (StreamWriter writer = new StreamWriter(outputFile))
                {

                    bool isFirstLine = true;

                    while (reader.Peek() != -1)
                    {
                        string line = reader.ReadLine();

                        if (string.IsNullOrEmpty(line)) continue;


                        byte[] asciiBytes = Encoding.ASCII.GetBytes(line);

                        var firstCharacterAscii = asciiBytes[0];



                        if (firstCharacterAscii == 62)
                        {
                            if (!isFirstLine)
                            {
                                writer.WriteLine();
                            }

                            writer.WriteLine(line);

                            isFirstLine = false;
                        }
                        else
                        {
                            writer.Write(line);   //notice -  no carriage return
                        }

                    }

                }


            }


        }


        [Test]
        public void GetLabelIncorpStatsOnAllResults()
        {
            var datasetnames = SipperDatasetUtilities.GetDatasetNames().Where(p => p.Contains("Yellow_C13"));


            //datasetnames = (from n in datasetnames where n.Contains("Yellow_C13_70") select n).ToList();


            //string resultFolder =
            //    @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Results_2013_04_09";

            string resultFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Results_2013_04_09";



            StringBuilder sb = new StringBuilder();


            List<SipperLcmsFeatureTargetedResultDTO> allResults = new List<SipperLcmsFeatureTargetedResultDTO>();


            string fastaFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\ID_002270_64B2CBBF.fasta";
            GwsDMSUtilities.FastaFileReader fastaFileReader = new FastaFileReader(fastaFile);

            var fastaData = fastaFileReader.ImportFastaFile();

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

                    var tightFilterResults = SipperFilters.ApplyAutoValidationCodeF1TightFilter(results).Where(p => p.ValidationCode == ValidationCode.Yes).ToList();

                    int countTightFilterResults = tightFilterResults.Count(p => p.ValidationCode == ValidationCode.Yes);

                    var looseFilteredResults = SipperFilters.ApplyAutoValidationCodeF2LooseFilter(results).Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
                    int countLooseFilter = looseFilteredResults.Count(p => p.ValidationCode == ValidationCode.Yes);


                    allResults.AddRange(tightFilterResults);

                    sb.Append(tightFilterResults.Count);
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


            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine("------------label incorporation for all results -------------");



            sb.Clear();

            List<SipperResultWithProteinInfo> allResultsAndProteinInfo = new List<SipperResultWithProteinInfo>();

            foreach (var r in allResults)
            {

                var fastaDataForCurrentResult = (from n in fastaData where n.Sequence.Contains(r.Code) select n).ToList();

                foreach (var item in fastaDataForCurrentResult)
                {


                    var organismInfo = JvciPeptideInfo.Parse(item.SingleLineDescription);


                    string organism = organismInfo.Organism ?? "";
                    string proteinName = organismInfo.CommonName ?? "";
                    string fibroNum = organismInfo.FibroNum ?? "";


                    sb.Append(r.DatasetName + "\t" + r.TargetID + "\t" + r.Code + "\t" + r.PercentPeptideLabelled + "\t" + r.PercentCarbonsLabelled + "\t" + organism + "\t" + proteinName + "\t" + fibroNum + "\t" + item.SingleLineDescription + Environment.NewLine);


                    SipperResultWithProteinInfo resultWithProteinInfo = new SipperResultWithProteinInfo();
                    resultWithProteinInfo.DatasetName = r.DatasetName;
                    resultWithProteinInfo.FeatureID = (int)r.TargetID;
                    resultWithProteinInfo.FastaDesc = item.SingleLineDescription;
                    resultWithProteinInfo.FibroAnno = fibroNum;
                    resultWithProteinInfo.Organism = organism;
                    resultWithProteinInfo.PercentIncorporation = r.PercentCarbonsLabelled;
                    resultWithProteinInfo.PercentPeptideLabelled = r.PercentPeptideLabelled;
                    resultWithProteinInfo.ProteinDesc = proteinName;
                    resultWithProteinInfo.Sequence = r.Code;

                    allResultsAndProteinInfo.Add(resultWithProteinInfo);

                }
            }

            Console.WriteLine(sb.ToString());


            List<string> organismList = allResultsAndProteinInfo.Select(p => p.Organism).Distinct().OrderBy(p => p).ToList();


            Console.WriteLine();
            Console.WriteLine();
            Console.WriteLine();

            sb.Clear();



            //Now, report protein-level results... will roll up to protein level

            sb.Append(
                "Organism\tProtein\tFastaIDs\tNumPeptides\tPeptideLabelPercent\tStdDevPeptideLabelPercent\tPercentIncorp\tStdDevPercentIncorp\n");
            foreach (var organism in organismList)
            {
                string organism1 = organism;
                var organismResults = allResultsAndProteinInfo.Where(p => p.Organism == organism1).ToList();

                List<string> proteinList = organismResults.Select(p => p.ProteinDesc).Distinct().ToList();


                foreach (var protein in proteinList)
                {
                    string protein1 = protein;
                    var proteinResults = organismResults.Where(p => p.ProteinDesc == protein1).ToList();


                    var fibroAnnoList = proteinResults.Select(p => p.FibroAnno).Distinct().OrderBy(p => p).ToList();

                    string delimeter = "; ";
                    var fibroAnnoString = fibroAnnoList.Select(i => i).Aggregate((i, j) => i + delimeter + j);



                    List<string> peptideList = proteinResults.Select(p => p.Sequence).Distinct().ToList();
                    List<double> peptidePercentVals = new List<double>();
                    List<double> carbonIncorpVals = new List<double>();

                    //peptide is found in multiple datasets. Need to roll these up to peptide level
                    foreach (var peptide in peptideList)
                    {
                        string peptide1 = peptide;
                        var peptideResults = proteinResults.Where(p => p.Sequence == peptide1).ToList();
                        var percentPeptideVal = peptideResults.Average(p => p.PercentPeptideLabelled);
                        var carbonIncorp = peptideResults.Average(p => p.PercentIncorporation);

                        peptidePercentVals.Add(percentPeptideVal);
                        carbonIncorpVals.Add(carbonIncorp);
                    }


                    double averagePercentPeptidesLabeled = peptidePercentVals.Average();
                    double stdevPercentPeptidesLabeled = MathUtils.GetStDev(peptidePercentVals);

                    double averageIncorp = carbonIncorpVals.Average();
                    double stdevIncorp = MathUtils.GetStDev(carbonIncorpVals);

                    int numVals = peptidePercentVals.Count;


                    string stdevPeptideLabeledstring = double.IsNaN(stdevPercentPeptidesLabeled) ? "" : stdevPercentPeptidesLabeled.ToString("0.00");
                    string stdevIncorpString = double.IsNaN(stdevIncorp) ? "" : stdevIncorp.ToString("0.00");

                    sb.Append(organism + "\t" + protein + "\t" + fibroAnnoString + "\t" + numVals + "\t" + averagePercentPeptidesLabeled.ToString("0.00") + "\t" +
                              stdevPeptideLabeledstring + "\t" + averageIncorp.ToString("0.00") + "\t" + stdevIncorpString + Environment.NewLine);

                }
            }

            Console.WriteLine(sb.ToString());

        }






        [Test]
        public void ExecuteSipperOnSpecificTargets()
        {
            string paramFile =
              @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Parameters\ExecutorParameters1.xml";

            SipperWorkflowExecutorParameters parameters = new SipperWorkflowExecutorParameters();
            parameters.LoadParameters(paramFile);


            parameters.DbName = "";
            parameters.DbServer = "";
            parameters.DbTableName = "";

            parameters.TargetsBaseFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\Unidentified";

            parameters.FolderPathForCopiedRawDataset = @"D:\data\sipper";


            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_C12_060_7Jan10_Andromeda_09-10-16.raw";



            int testTarget = 8625;


            SipperWorkflowExecutor executor = new SipperWorkflowExecutor(parameters, testDataset);

            executor.Targets.TargetList = (from n in executor.Targets.TargetList where n.ID == testTarget select n).ToList();

            executor.Execute();



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
                        sb.Append(r.DatasetName + "\t" + r.TargetID + "\t" + r.MatchedMassTagID + "\t" + r.Intensity + "\t" + r.FitScoreLabeledProfile.ToString("0.000") + "\t" + r.PercentCarbonsLabelled.ToString("0.0") + "\t" + r.PercentPeptideLabelled.ToString("0.0") + "\n");

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


            var allPeptides = infoExtractor.GetPeptideInfo(allMassTags, false, true);
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
