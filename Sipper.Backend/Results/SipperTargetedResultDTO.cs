using System;
using DeconTools.Workflows.Backend.Results;

namespace Sipper.Backend.Results
{
    public class SipperTargetedResultDTO : UnlabelledTargetedResultDTO
    {
        public SipperTargetedResultDTO(SipperTargetedResult result)
        {

            WriteStandardInfoToResult(this, result);
            addAdditionalInfo(this, result);

        }

        
        //TODO: this is duplicated code.  See 'ResultDTOFactory' in DeconTools.Workflows
        private void WriteStandardInfoToResult(SipperTargetedResultDTO tr, SipperTargetedResult result)
        {
            if (result.Target == null)
            {
                throw new ArgumentNullException("Cannot create result object. MassTag is null.");
            }

            if (result.Run == null)
            {
                throw new ArgumentNullException("Cannot create result object. Run is null.");
            }


            tr.DatasetName = result.Run.DatasetName;
            tr.TargetID = result.Target.ID;
            tr.ChargeState = result.Target.ChargeState;

            tr.IndexOfMostAbundantPeak = result.IsotopicProfile == null ? (short)0 : (short)result.IsotopicProfile.GetIndexOfMostIntensePeak();
            tr.Intensity = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.IntensityAggregate;
            tr.IntensityI0 = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.GetMonoAbundance();
            tr.IntensityMostAbundantPeak = result.IsotopicProfile == null ? 0f : (float)result.IsotopicProfile.getMostIntensePeak().Height;
            tr.IScore = (float)result.InterferenceScore;
            tr.MonoMass = result.IsotopicProfile == null ? 0d : result.IsotopicProfile.MonoIsotopicMass;
            tr.MonoMZ = result.IsotopicProfile == null ? 0d : result.IsotopicProfile.MonoPeakMZ;
            tr.MassErrorInPPM = result.IsotopicProfile == null ? 0d : result.GetMassAlignmentErrorInPPM();
            tr.MonoMassCalibrated = result.IsotopicProfile == null ? 0d : -1 * ((result.Target.MonoIsotopicMass * tr.MassErrorInPPM / 1e6) - result.Target.MonoIsotopicMass);   // massError= (theorMZ-alignedObsMZ)/theorMZ * 1e6

            tr.ScanLC = result.GetScanNum();
            tr.NET = (float)result.GetNET();
            tr.NETError = result.Target.NormalizedElutionTime - tr.NET;


            tr.NumChromPeaksWithinTol = result.NumChromPeaksWithinTolerance;
            tr.NumQualityChromPeaksWithinTol = result.NumQualityChromPeaks;
            tr.FitScore = (float)result.Score;

            if (result.ChromPeakSelected != null)
            {
                double sigma = result.ChromPeakSelected.Width / 2.35;
                tr.ScanLCStart = (int)Math.Round(result.ChromPeakSelected.XValue - sigma);
                tr.ScanLCEnd = (int)Math.Round(result.ChromPeakSelected.XValue + sigma);
            }

            if (result.FailedResult)
            {
                tr.FailureType = result.FailureType.ToString();
            }
        }

        public double AreaUnderRatioCurve { get; set; }

        public double AreaUnderDifferenceCurve { get; set; }



        private static void addAdditionalInfo(SipperTargetedResultDTO tr, SipperTargetedResult result)
        {
            
            tr.AreaUnderDifferenceCurve = result.AreaUnderDifferenceCurve;
            tr.AreaUnderRatioCurve = result.AreaUnderRatioCurve;


        }
    }
}
