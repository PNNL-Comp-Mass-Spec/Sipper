using System.Collections.Generic;
using System.Linq;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Results;

namespace Sipper.Model
{
    public class SipperFilters
    {
        //Some history here... I reran the parameter optimization on 9/11/2013 (after getting feedback from reviewers)
        //So these are updated as of that date. See the 'OldApplyAutoValidationCodeF2LooseFilter' for the older settings

        public static void ApplyAutoValidationCodeF2LooseFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
        {
            foreach (var resultDto in resultList)
            {
                resultDto.ValidationCode = ValidationCode.None;
            }

            var peptidesPassingFilter = (from n in resultList
                                         where n.FitScoreLabeledProfile <= 0.8 &&
                                               n.IScore <= 0.6 &&
                                               n.AreaUnderRatioCurveRevised >= 0 &&
                                               n.ContiguousnessScore >=0 &&
                                               n.PercentCarbonsLabeled>=0.5 &&
                                               n.PercentPeptideLabeled>=0.5
                                         select n).ToList();

            foreach (var resultDto in peptidesPassingFilter)
            {
                resultDto.ValidationCode = ValidationCode.Yes;
            }
        }


        public static void OldApplyAutoValidationCodeF2LooseFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
        {
            foreach (var resultDto in resultList)
            {
                resultDto.ValidationCode = ValidationCode.None;
            }

            var peptidesPassingFilter = (from n in resultList
                                         where n.AreaUnderRatioCurveRevised >= 2 &&
                                               n.IScore <= 0.9 &&
                                               n.FitScoreLabeledProfile <= 0.5 &&
                                               n.PercentCarbonsLabeled >= 0 &&
                                               n.PercentPeptideLabeled >= 0
                                         select n).ToList();

            foreach (var resultDto in peptidesPassingFilter)
            {
                resultDto.ValidationCode = ValidationCode.Yes;
            }
        }


        public static void ApplyAutoValidationCodeF2LooseFilter(SipperLcmsFeatureTargetedResultDTO result)
        {
            var resultList = new List<SipperLcmsFeatureTargetedResultDTO> { result };
            ApplyAutoValidationCodeF2LooseFilter(resultList);
        }

        public static void ApplyAutoValidationCodeF1TightFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
        {
            foreach (var resultDto in resultList)
            {
                resultDto.ValidationCode = ValidationCode.None;
            }

            //NOTE: these are the same as the SIPPER paper, table 1.
            var peptidesPassingFilter = (from n in resultList
                                         where n.FitScoreLabeledProfile<=0.4 &&
                                         n.IScore <=0.4 &&
                                         n.AreaUnderRatioCurveRevised >=2.0 &&
                                         n.ContiguousnessScore >=3 &&
                                         n.PercentCarbonsLabeled >=0 &&
                                         n.PercentPeptideLabeled >=0
                                         select n).ToList();

            foreach (var resultDto in peptidesPassingFilter)
            {
                resultDto.ValidationCode = ValidationCode.Yes;
            }
        }


        public static void OldApplyAutoValidationCodeF1TightFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
        {
            foreach (var resultDto in resultList)
            {
                resultDto.ValidationCode = ValidationCode.None;
            }

            //NOTE: these correspond to a FalsePositive rate of '0'
            var peptidesPassingFilter = (from n in resultList
                                         where n.AreaUnderRatioCurveRevised >= 1 &&
                                               n.IScore <= 0.2 &&
                                               n.FitScoreLabeledProfile <= 0.5 &&
                                               n.PercentCarbonsLabeled >= 0 &&
                                               n.PercentPeptideLabeled >= 0.5
                                         select n).ToList();

            foreach (var resultDto in peptidesPassingFilter)
            {
                resultDto.ValidationCode = ValidationCode.Yes;
            }
        }


        public static void ApplyAutoValidationCodeF1TightFilter(SipperLcmsFeatureTargetedResultDTO result)
        {
            var resultList = new List<SipperLcmsFeatureTargetedResultDTO>{result};
            ApplyAutoValidationCodeF1TightFilter(resultList);
        }

        public static void ApplyAveragineBasedTightFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
        {
            foreach (var resultDto in resultList)
            {
                resultDto.ValidationCode = ValidationCode.None;
            }

            //NOTE: these correspond to a FalsePositive rate of '0'
            var peptidesPassingFilter = (from n in resultList
                                         where n.AreaUnderRatioCurveRevised >= 1 &&
                                               n.IScore <= 0 &&
                                               n.FitScoreLabeledProfile <= 1.1 &&
                                               n.ContiguousnessScore >= 0 &&
                                               n.PercentCarbonsLabeled >= 0 &&
                                               n.PercentPeptideLabeled >= 0
                                         select n).ToList();

            foreach (var resultDto in peptidesPassingFilter)
            {
                resultDto.ValidationCode = ValidationCode.Yes;
            }
        }

        public static void ApplyAveragineBasedTightFilter(SipperLcmsFeatureTargetedResultDTO result)
        {
            var resultList = new List<SipperLcmsFeatureTargetedResultDTO> { result };
            ApplyAveragineBasedTightFilter(resultList);
        }



        public static void ApplyAveragineBasedLooseFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
        {
            foreach (var resultDto in resultList)
            {
                resultDto.ValidationCode = ValidationCode.None;
            }

            //NOTE: these correspond to a FalsePositive rate of '0'
            var peptidesPassingFilter = (from n in resultList
                                         where n.AreaUnderRatioCurveRevised >= 1 &&
                                               n.IScore <= 0.4 &&
                                               n.FitScoreLabeledProfile <= 1.1 &&
                                               n.ContiguousnessScore>=2 &&
                                               n.PercentCarbonsLabeled >= 0 &&
                                               n.PercentPeptideLabeled >= 0
                                         select n).ToList();

            foreach (var resultDto in peptidesPassingFilter)
            {
                resultDto.ValidationCode = ValidationCode.Yes;
            }

        }

        public static void ApplyAveragineBasedLooseFilter(SipperLcmsFeatureTargetedResultDTO result)
        {
            var resultList = new List<SipperLcmsFeatureTargetedResultDTO> { result };
            ApplyAveragineBasedLooseFilter(resultList);
        }


    }
}
