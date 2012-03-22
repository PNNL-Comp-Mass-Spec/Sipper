using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using DeconTools.Utilities;
using Sipper.Backend.Results;

namespace Sipper.Backend
{
    public class SipperQuantifier:Task
    {

        public override void Execute(ResultCollection resultColl)
        {
            Check.Require(resultColl.CurrentTargetedResult is SipperTargetedResult,"Sipper Quantifier only works on Sipper-type result objects");

            SipperTargetedResult result = (SipperTargetedResult) resultColl.CurrentTargetedResult;
            var unlabeledIso = resultColl.Run.CurrentMassTag.IsotopicProfile.CloneIsotopicProfile();
            IsotopicProfileUtilities.NormalizeIsotopicProfile(unlabeledIso);

            //PeakUtilities.TrimIsotopicProfile(unlabeledIso, 0.001);


            var subtractedIso = result.IsotopicProfile.CloneIsotopicProfile();
            IsotopicProfileUtilities.NormalizeIsotopicProfile(subtractedIso);

            int numTheoPeaks = unlabeledIso.Peaklist.Count;

            subtractedIso.Peaklist = subtractedIso.Peaklist.Take(numTheoPeaks).ToList();

            for (int i = 0; i < subtractedIso.Peaklist.Count; i++)
            {
                subtractedIso.Peaklist[i].Height = subtractedIso.Peaklist[i].Height -
                                                   unlabeledIso.Peaklist[i].Height;
            }

            subtractedIso.Peaklist = subtractedIso.Peaklist.Take(numTheoPeaks).ToList();

            var xvals =
                subtractedIso.Peaklist.Select((p, i) => new { peak = p, index = i }).Select(n => (double)n.index).ToList();

            var yvals = subtractedIso.Peaklist.Select(p => (double)p.Height).ToList();

            result.AreaUnderDifferenceCurve = CalculateAreaUnderCubicSplineFit(xvals, yvals);

            for (int i = 0; i < subtractedIso.Peaklist.Count; i++)
            {
                subtractedIso.Peaklist[i].Height = (subtractedIso.Peaklist[i].Height / unlabeledIso.Peaklist[i].Height);
            }

            yvals = subtractedIso.Peaklist.Select(p => (double)p.Height).ToList();

            result.AreaUnderRatioCurve = CalculateAreaUnderCubicSplineFit(xvals, yvals);

        }


        private double CalculateAreaUnderCubicSplineFit(List<double> xvals, List<double> yvals)
        {
            // var interp = Interpolate.RationalWithoutPoles(xvals, yvals);

            var interp = new MathNet.Numerics.Interpolation.Algorithms.CubicSplineInterpolation(xvals, yvals);

            double area = interp.Integrate(xvals.Max());

            return area;
        }

    }
}
