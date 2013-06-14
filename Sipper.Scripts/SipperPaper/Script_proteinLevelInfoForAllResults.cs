using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using GwsDMSUtilities;
using NUnit.Framework;
using Sipper.Model;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class Script_proteinLevelInfoForAllResults
    {

        [Test]
        public void FormatAndOutputAllResults()
        {
              string resultFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Results_2013_04_09";


            var dirInfo = new DirectoryInfo(resultFolder);
            var fileInfos = dirInfo.GetFiles("*_results.txt");


            fileInfos = (from n in fileInfos where n.Name.Contains("Yellow_C13") select n).ToArray();

            StringBuilder sb = new StringBuilder();
            List<SipperLcmsFeatureTargetedResultDTO> allResults = new List<SipperLcmsFeatureTargetedResultDTO>();

            string outputFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Results_2013_04_09\_allYellowC13Results.txt";

            if (File.Exists(outputFile)) File.Delete(outputFile);

            bool isFirstLine = true;

            int counter = 0;
            foreach (var file in fileInfos)
            {
                Console.WriteLine("Working on file " + counter++);
                SipperResultFromTextImporter importer = new SipperResultFromTextImporter(file.FullName);
                var results = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

                SipperFilters.ApplyAutoValidationCodeF1TightFilter(results);

                using (StreamWriter writer = File.AppendText(outputFile))
                {
                    foreach (var re in results)
                    {

                        int valCode = re.ValidationCode == ValidationCode.Yes ? 1 : 0;

                        if (isFirstLine)
                        {
                            writer.WriteLine(
                                "Dataset\tTargetID\tSequence\tMonoMz\tChargeState\tIntensity\tIScore\tContigScore\tSumOfRatios\tPercentPeptideLabeled\tPercentIncorp\tAutoDetected");
                            isFirstLine = false;
                        }

                        writer.WriteLine(re.DatasetName + "\t" + re.TargetID + "\t" + re.Code + "\t" + re.MonoMZ.ToString("0.00000") + "\t" +
                                         re.ChargeState + "\t" +re.Intensity+ "\t"  + re.IScore.ToString("0.000")+ "\t" +  re.ContiguousnessScore + "\t" + re.AreaUnderRatioCurveRevised.ToString("0") +
                                         "\t" + re.PercentPeptideLabelled.ToString("0.0") + "\t" + re.PercentCarbonsLabelled.ToString("0.00") + "\t" + valCode);
                    }

                }
                
                
                
                
                allResults.AddRange(results);

            }






        }




        [Test]
        public void GetProteinAndPeptideStatesOnResults()
        {

            string resultFolder =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Results_2013_04_09";


            var dirInfo = new DirectoryInfo(resultFolder);
            var fileInfos = dirInfo.GetFiles("*_results.txt");


            fileInfos = (from n in fileInfos where n.Name.Contains("Yellow_C13") select n).ToArray();

            StringBuilder sb = new StringBuilder();
            List<SipperLcmsFeatureTargetedResultDTO> allResults = new List<SipperLcmsFeatureTargetedResultDTO>();


            string fastaFile = @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\ID_002270_64B2CBBF.fasta";
            var fastaFileReader = new FastaFileReader(fastaFile);
            var fastaData = fastaFileReader.ImportFastaFile();

            List<SipperResultWithProteinInfo> allResultsAndProteinInfo = new List<SipperResultWithProteinInfo>();


            int counter = 0;
            foreach (var file in fileInfos)
            {
                Console.WriteLine("Working on file " + counter++);

                SipperResultFromTextImporter importer = new SipperResultFromTextImporter(file.FullName);
                var results = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();
                allResults.AddRange(results);

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

            }

            string outputResultsFileWithProteinInfo =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Output.txt";

            using (StreamWriter writer=new StreamWriter(outputResultsFileWithProteinInfo))
            {
                writer.WriteLine("Organism\tProtein\tFastaIDs\tNumPeptides\tPeptideLabelPercent\tStdDevPeptideLabelPercent\tPercentIncorp\tStdDevPercentIncorp");
                List<string> organismList = allResultsAndProteinInfo.Select(p => p.Organism).Distinct().OrderBy(p => p).ToList();
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

                        writer.WriteLine(organism + "\t" + protein + "\t" + fibroAnnoString + "\t" + numVals + "\t" + averagePercentPeptidesLabeled.ToString("0.00") + "\t" +
                                  stdevPeptideLabeledstring + "\t" + averageIncorp.ToString("0.00") + "\t" + stdevIncorpString);



                    }
                }

            }

        }



    }
}
