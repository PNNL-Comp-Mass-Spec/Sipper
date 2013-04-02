using System.Collections.Generic;
using System.Linq;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    public class SipperFilters
    {

        [Category("Final")]
        public static List<SipperLcmsFeatureTargetedResultDTO> ApplyAutoValidationCodeF2LooseFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
        {
            foreach (var resultDto in resultList)
            {
                resultDto.ValidationCode = ValidationCode.None;
            }


            var peptidesPassingFilter = (from n in resultList
                                         where n.AreaUnderRatioCurveRevised >= 1 &&
                                               n.IScore <= 1 &&
                                               n.FitScoreLabeledProfile <= 0.6 &&
                                               n.PercentCarbonsLabelled >= 0 &&
                                               n.PercentPeptideLabelled >= 0
                                         select n).ToList();

            foreach (var resultDto in peptidesPassingFilter)
            {
                resultDto.ValidationCode = ValidationCode.Yes;
            }

            return resultList;

        }


        [Category("Final")]
        public static List<SipperLcmsFeatureTargetedResultDTO> ApplyAutoValidationCodeF1TightFilter(List<SipperLcmsFeatureTargetedResultDTO> resultList)
        {
            foreach (var resultDto in resultList)
            {
                resultDto.ValidationCode = ValidationCode.None;
            }

            //NOTE: these correspond to a FalsePositive rate of '3' 
            var peptidesPassingFilter = (from n in resultList
                                         where n.AreaUnderRatioCurveRevised >= 2 &&
                                               n.IScore <= 0.3 &&
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

    }
}
