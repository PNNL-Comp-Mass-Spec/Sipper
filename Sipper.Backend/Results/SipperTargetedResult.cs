using System.Text;
using DeconTools.Backend.Core;

namespace Sipper.Backend.Results
{
    public class SipperTargetedResult : TargetedResultBase
    {

        #region Constructors
        public SipperTargetedResult() { }

        public SipperTargetedResult(TargetBase target) : base(target) { }

        #endregion

        #region Properties

        public double AreaUnderRatioCurve { get; set; }

        public double AreaUnderDifferenceCurve { get; set; }
        
        //TODO: add props


        #endregion


        public override string ToString()
        {
            if (IsotopicProfile == null)
                return base.ToString();

            string delim = "; ";

            var sb = new StringBuilder();
            sb.Append(((LcmsFeatureTarget)Target).FeatureToMassTagID);
            sb.Append(delim);
            sb.Append(ScanSet.PrimaryScanNumber);
            sb.Append(delim);
            sb.Append(IsotopicProfile.MonoIsotopicMass.ToString("0.0000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile.MonoPeakMZ.ToString("0.00000"));
            sb.Append(delim);
            sb.Append(IsotopicProfile.ChargeState);
            sb.Append(delim);
            sb.Append(IsotopicProfile.IntensityAggregate);
            sb.Append(delim);
            sb.Append(Score.ToString("0.0000"));
            sb.Append(delim);
            sb.Append(InterferenceScore.ToString("0.0000"));
            sb.Append(delim);
            sb.Append(AreaUnderDifferenceCurve.ToString("0.0000"));
            sb.Append(delim);
            sb.Append(AreaUnderRatioCurve.ToString("0.00"));
            sb.Append(delim);
            sb.Append(IsotopicProfile.Peaklist.Count);

            return sb.ToString();
        }

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion

    }
}
