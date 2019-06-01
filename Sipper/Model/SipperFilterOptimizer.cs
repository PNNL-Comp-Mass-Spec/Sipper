using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using PRISM.Logging;

namespace Sipper.Model
{
    public class SipperFilterOptimizer
    {

        #region Constructors
        public SipperFilterOptimizer()
        {
            LabelFitLower = 0.2;
            LabelFitUpper = 1.1;
            LabelFitStep = 0.2;

            SumOfRatiosLower = 0;
            SumOfRatiosUpper = 10.5;
            SumOfRatiosStep = 1;

            IscoreLower = 0;
            IscoreUpper = 1.0;
            IscoreStep = 0.1;

            ContigScoreLower = 0;
            ContigScoreUpper = 7;
            ContigScoreStep = 1;

            PercentIncorpLower = 0;
            PercentIncorpUpper = 2;
            PercentIncorpStep = 0.5;

            PercentPeptidePopulationLower = 0;
            PercentPeptidePopulationUpper = 2;
            PercentPeptidePopulationStep = 0.5;

            LabeledResults = new List<SipperLcmsFeatureTargetedResultDTO>();
            UnlabeledResults = new List<SipperLcmsFeatureTargetedResultDTO>();
        }

        #endregion

        #region Properties

        #endregion

        #region Public Methods

        //TODO:  this isn't working correctly
        public int GetNumCombinations()
        {
            var numCombos = (int)((LabelFitUpper - LabelFitLower)/LabelFitStep);
            numCombos *= (int)((SumOfRatiosUpper - SumOfRatiosLower) / SumOfRatiosStep);
            numCombos *= (int)((IscoreUpper - IscoreLower) / IscoreStep);
            numCombos *= (int)((ContigScoreUpper - ContigScoreLower +1) / ContigScoreStep);
            numCombos *= (int)((PercentIncorpUpper - PercentIncorpLower) / PercentIncorpStep);
            numCombos *= (int)((PercentPeptidePopulationUpper - PercentPeptidePopulationLower) / PercentPeptidePopulationStep);

            return numCombos;
        }


        public List<ParameterOptimizationResult>GetOptimizedFiltersByFalsePositiveRate(List<ParameterOptimizationResult>allOptimizationResults, double maxAllowedFalsePositiveRate = 0.1d)
        {
            var filteredParameters = (from n in allOptimizationResults
                                      where n.FalsePositiveRate <= maxAllowedFalsePositiveRate
                                      orderby n.NumLabeledPassingFilter descending
                                      select n).ToList();

            return filteredParameters;

        }


        public XYData GetRocCurve (List<ParameterOptimizationResult> allOptimizationResults)
        {

            var maxNumLabeled = allOptimizationResults.Select(p => p.NumLabeledPassingFilter).Max();

            var rocPoints = new Dictionary<int, int>();

            for (var i = 0; i < maxNumLabeled; i++)
            {
                var currentPoint = i;
                var parameterResultsForPoint = allOptimizationResults.Where(p => p.NumUnlabelledPassingFilter == currentPoint).ToList();

                var anyData = parameterResultsForPoint.Any();

                if (anyData)
                {
                    var maxTruePositives = parameterResultsForPoint.Max(p => p.NumLabeledPassingFilter);
                    rocPoints.Add(i, maxTruePositives);
                }
            }

            var rocCurve = new XYData
                                  {
                                      Xvalues = rocPoints.Keys.Select(p => (double) p).ToArray(),
                                      Yvalues = rocPoints.Values.Select(p => (double) p).ToArray()
                                  };

            return rocCurve;
        }


        public void LoadLabeledResults(string filePath)
        {
            var importer = new SipperResultFromTextImporter(filePath);
            LabeledResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

        }


        public void LoadUnlabeledResults(string filePath)
        {
            var importer = new SipperResultFromTextImporter(filePath);
            UnlabeledResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();
        }

        public List<SipperLcmsFeatureTargetedResultDTO> UnlabeledResults { get; set; }
        public List<SipperLcmsFeatureTargetedResultDTO> LabeledResults { get; set; }


        public List<ParameterOptimizationResult> DoCalculationsOnAllFilterCombinations(string outputFileName = null)
        {
            var isHeaderWritten = false;

            if (!string.IsNullOrEmpty(outputFileName) && File.Exists(outputFileName)) File.Delete(outputFileName);


            var parameterOptimizationResults = new List<ParameterOptimizationResult>();
            var numCombinations = GetNumCombinations();

            FileLogger.WriteLog(BaseLogger.LogLevels.INFO, "Filter optimizer - num combinations to analyze = "+ numCombinations);
            var sb = new StringBuilder();

            var comboCounter = 0;

            for (var fitScoreLabelled = LabelFitLower; fitScoreLabelled < LabelFitUpper; fitScoreLabelled = fitScoreLabelled + LabelFitStep)
            {
                sb.Clear();

                var levelOneC13Filter = (from n in LabeledResults
                                         where n.FitScoreLabeledProfile <= fitScoreLabelled
                                         select n).ToList();

                var levelOneC12Filter = (from n in UnlabeledResults
                                         where n.FitScoreLabeledProfile <= fitScoreLabelled
                                         select n).ToList();

                FileLogger.WriteLog(BaseLogger.LogLevels.INFO, "Current filter combo: " + comboCounter +" out of " + numCombinations);

                for (var area = SumOfRatiosLower; area < SumOfRatiosUpper; area = area + SumOfRatiosStep)
                {
                    var levelTwoC13Filter = (from n in levelOneC13Filter
                                             where n.AreaUnderRatioCurveRevised >= area
                                             select n).ToList();

                    var levelTwoC12Filter = (from n in levelOneC12Filter
                                             where n.AreaUnderRatioCurveRevised >= area
                                             select n).ToList();


                    for (var iscore = IscoreLower; iscore < IscoreUpper; iscore = iscore + IscoreStep)
                    {

                        var levelThreeC13Filter = (from n in levelTwoC13Filter
                                                 where n.IScore <= iscore
                                                 select n).ToList();

                        var levelThreeC12Filter = (from n in levelTwoC12Filter
                                                 where n.IScore <= iscore
                                                 select n).ToList();



                        for (var contigScore = ContigScoreLower; contigScore <= ContigScoreUpper; contigScore = contigScore + ContigScoreStep)
                        {
                            for (var percentIncorp = PercentIncorpLower; percentIncorp < PercentIncorpUpper; percentIncorp = percentIncorp + PercentIncorpStep)
                            {
                                for (var peptidePop = PercentPeptidePopulationLower; peptidePop < PercentPeptidePopulationUpper; peptidePop = peptidePop + PercentPeptidePopulationStep)
                                {
                                    var c13filteredResults = (from n in levelThreeC13Filter
                                                              where n.ContiguousnessScore >= contigScore
                                                             && n.PercentCarbonsLabelled >= percentIncorp
                                                             && n.PercentPeptideLabelled >= peptidePop
                                                              select n).ToList();

                                    var c12filteredResults = (from n in levelThreeC12Filter
                                                              where n.ContiguousnessScore >= contigScore
                                                             && n.PercentCarbonsLabelled >= percentIncorp
                                                             && n.PercentPeptideLabelled >= peptidePop
                                                              select n).ToList();


                                    var optimizationResult = new ParameterOptimizationResult();
                                    optimizationResult.FitScoreLabelled = fitScoreLabelled;
                                    optimizationResult.SumOfRatios = area;
                                    optimizationResult.Iscore = iscore;
                                    optimizationResult.ContigScore = contigScore;
                                    optimizationResult.PercentIncorp = percentIncorp;
                                    optimizationResult.PercentPeptidePopulation = peptidePop;

                                    optimizationResult.NumLabeledPassingFilter = c13filteredResults.Count;
                                    optimizationResult.NumUnlabelledPassingFilter = c12filteredResults.Count;

                                    sb.Append(optimizationResult.ToStringWithDetails());
                                    sb.Append(Environment.NewLine);

                                    parameterOptimizationResults.Add(optimizationResult);

                                    comboCounter++;

                                }
                            }


                        }
                    }
                }

                if (!string.IsNullOrEmpty(outputFileName))
                {
                    using (var sw = new StreamWriter(new FileStream(outputFileName, FileMode.Append, FileAccess.Write, FileShare.Read)))
                    {
                        sw.AutoFlush = true;

                        if (!isHeaderWritten)
                        {
                            sw.WriteLine("LabeledFit\tSumOfRatios\tIscore\tContigScore\tPercentIncorp\tPercentPeptideLabeled\tUnlabeledCount\tLabeledCount\tFalsePositiveRate");
                        }
                        sw.Write(sb.ToString());

                    }
                }

            }

            return parameterOptimizationResults;



        }



        #endregion

        #region Private Methods

        #endregion

        public string UnlabeledResultsFilePath { get; set; }

        public string LabeledResultsFilePath { get; set; }

        public double LabelFitLower { get; set; }

        public double LabelFitUpper { get; set; }

        public double LabelFitStep { get; set; }

        public double SumOfRatiosLower { get; set; }

        public double SumOfRatiosUpper { get; set; }

        public double SumOfRatiosStep { get; set; }

        public double IscoreLower { get; set; }

        public double IscoreUpper { get; set; }

        public double IscoreStep { get; set; }

        public int ContigScoreLower { get; set; }

        public int ContigScoreUpper { get; set; }

        public int ContigScoreStep { get; set; }

        public double PercentIncorpLower { get; set; }

        public double PercentIncorpUpper { get; set; }

        public double PercentIncorpStep { get; set; }

        public double PercentPeptidePopulationLower { get; set; }

        public double PercentPeptidePopulationUpper { get; set; }

        public double PercentPeptidePopulationStep { get; set; }


    }
}
