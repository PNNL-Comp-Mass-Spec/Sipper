using System.Collections.Generic;
using System.Linq;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    public class SipperFilters
    {

        [Category("Paper")]
        public static List<SipperLcmsFeatureTargetedResultDTO> ApplyAutoValidationCodeF2LooseFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
        {
            foreach (var resultDto in resultList)
            {
                resultDto.ValidationCode = ValidationCode.None;
            }


            var peptidesPassingFilter = (from n in resultList
                                         where n.AreaUnderRatioCurveRevised >= 2 &&
                                               n.IScore <= 0.9 &&
                                               n.FitScoreLabeledProfile <= 0.5 &&
                                               n.PercentCarbonsLabelled >= 0 &&
                                               n.PercentPeptideLabelled >= 0
                                         select n).ToList();

            foreach (var resultDto in peptidesPassingFilter)
            {
                resultDto.ValidationCode = ValidationCode.Yes;
            }

            return resultList;

        }


        [Category("Paper")]
        public static List<SipperLcmsFeatureTargetedResultDTO> ApplyAutoValidationCodeF1TightFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
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
                                               n.PercentCarbonsLabelled >= 0 &&
                                               n.PercentPeptideLabelled >= 0.5
                                         select n).ToList();

            foreach (var resultDto in peptidesPassingFilter)
            {
                resultDto.ValidationCode = ValidationCode.Yes;
            }

            return resultList;
        }


        [Category("Paper")]
        public static List<SipperLcmsFeatureTargetedResultDTO> ApplyAveragineBasedTightFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
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
                                               n.PercentCarbonsLabelled >= 0 &&
                                               n.PercentPeptideLabelled >= 0
                                         select n).ToList();

            foreach (var resultDto in peptidesPassingFilter)
            {
                resultDto.ValidationCode = ValidationCode.Yes;
            }

            return resultList;
        }


        [Category("Paper")]
        public static List<SipperLcmsFeatureTargetedResultDTO> ApplyAveragineBasedLooseFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
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
                                               n.PercentCarbonsLabelled >= 0 &&
                                               n.PercentPeptideLabelled >= 0
                                         select n).ToList();

            foreach (var resultDto in peptidesPassingFilter)
            {
                resultDto.ValidationCode = ValidationCode.Yes;
            }

            return resultList;
        }



    }
}
