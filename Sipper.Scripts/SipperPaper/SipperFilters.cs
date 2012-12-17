using DeconTools.Workflows.Backend.Results;

namespace Sipper.Scripts.SipperPaper
{
    public class SipperFilters
    {
        public static bool PassesF1Filter(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.NumHighQualityProfilePeaks >= 2 && result.Intensity != 0)
            {
                return true;
            }

            return false;
        }

        public static bool PassesF2Filter(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.NumHighQualityProfilePeaks > 2 &&
                result.FitScore < 0.1 &&   // 2* stdev for fit score... see spreasheet 'Summary data for manual analysis of dataset'
                result.ChromCorrelationMedian > 0.70 &&   //3* stdev 
                result.IScore < 0.55)    //2 * stdev iscore
            {
                return true;
            }

            return false;
        }

        public static bool PassesF3Filter(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.RSquaredValForRatioCurve > 0.46
                && result.NumHighQualityProfilePeaks >= 4
                && result.AreaUnderRatioCurveRevised > 1)
            {
                return true;
            }

            return false;
        }


        public static bool PassesF4Filter(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (PassesF3Filter(result)
                && result.ChromCorrelationMedian>0.85
                && result.RSquaredValForRatioCurve>0.90
                //&& result.NumHighQualityProfilePeaks>=6
                && result.AreaUnderRatioCurveRevised>5
                && result.IScore<0.1)
            {
                return true;
            }

            return false;


        }


        public static bool PassesLabelTightFilterF1(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.RSquaredValForRatioCurve>=0.95 &&
                result.AreaUnderRatioCurveRevised>=3.5 &&
                result.ChromCorrelationMedian>=0.5 && 
                result.IScore<=0.2)
            {
                return true;
            }

            return false;

        }


        public static bool PassesLabelLooseFilterF2(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.RSquaredValForRatioCurve >= 0.84 &&
                result.AreaUnderRatioCurveRevised >= 1.5 &&
                result.ChromCorrelationMedian >= 0.5 &&
                result.IScore <= 0.44)
            {
                return true;
            }

            return false;

        }


        public static bool PassesNoLabelTightFilterF1(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.AreaUnderRatioCurveRevised <= 0 &&
                result.ChromCorrelationMedian >= 0.95 &&
                result.IScore <= 0)
            {
                return true;
            }

            return false;

        }


        public static bool PassesNoLabelLooseFilterF2(SipperLcmsFeatureTargetedResultDTO result)
        {
            if (result.AreaUnderRatioCurveRevised <= 0 &&
                result.ChromCorrelationMedian >= 0.85 &&
                result.IScore <= 0.25)
            {
                return true;
            }

            return false;

        }





        public static void ApplyFilterF1(SipperLcmsFeatureTargetedResultDTO result)
        {
            result.PassesFilter = PassesF1Filter(result);

        }

        public static void ApplyFilterF2(SipperLcmsFeatureTargetedResultDTO result)
        {
            result.PassesFilter = PassesF2Filter(result);

        }

        public static void ApplyFilterF3(SipperLcmsFeatureTargetedResultDTO result)
        {
            result.PassesFilter = PassesF3Filter(result);

        }
    }
}
