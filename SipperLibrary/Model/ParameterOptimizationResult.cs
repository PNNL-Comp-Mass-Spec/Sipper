using System.Text;

namespace Sipper.Model
{
    public class ParameterOptimizationResult
    {

        #region Constructors

        #endregion

        #region Properties

        public double FitScoreLabeled { get; set; }
        public double SumOfRatios { get; set; }
        public double InterferenceScore { get; set; }
        public int ContigScore { get; set; }
        public double PercentIncorporated { get; set; }
        public double PercentPeptidePopulation { get; set; }
        public int NumUnlabeledPassingFilter { get; set; }
        public int NumLabeledPassingFilter { get; set; }

        public double FalsePositiveRate
        {
            get
            {
                if (NumLabeledPassingFilter > 0 || NumUnlabeledPassingFilter > 0)
                {
                    return (NumUnlabeledPassingFilter / (double)(NumUnlabeledPassingFilter + NumLabeledPassingFilter));
                }

                return double.NaN;
            }
        }

        public string ToStringWithDetails(char delimiter = '\t')
        {
            var sb = new StringBuilder();
            sb.Append(FitScoreLabeled);
            sb.Append(delimiter);
            sb.Append(SumOfRatios);
            sb.Append(delimiter);
            sb.Append(InterferenceScore);
            sb.Append(delimiter);
            sb.Append(ContigScore);
            sb.Append(delimiter);
            sb.Append(PercentIncorporated);
            sb.Append(delimiter);
            sb.Append(PercentPeptidePopulation);
            sb.Append(delimiter);
            sb.Append(NumUnlabeledPassingFilter);
            sb.Append(delimiter);
            sb.Append(NumLabeledPassingFilter);
            sb.Append(delimiter);
            sb.Append(double.IsNaN(FalsePositiveRate) ? "1.0" : FalsePositiveRate.ToString("0.###"));

            return sb.ToString();
        }

        #endregion

        #region Public Methods

        #endregion

        #region Private Methods

        #endregion
    }
}
