using System.Text;
using DeconTools.Workflows.Backend.FileIO;
using Sipper.Backend.Results;

namespace Sipper.Backend.FileIO
{
    public class SipperResultToLcmsFeatureExporter : TargetedResultToTextExporter
    {

        #region Constructors
        public SipperResultToLcmsFeatureExporter(string filename)
            : base(filename)
        {
        }

       
        #endregion

        protected override string addAdditionalInfo(DeconTools.Workflows.Backend.Results.TargetedResultDTO result)
        {
            var sipperResult = (SipperTargetedResultDTO)result;

            StringBuilder sb = new StringBuilder();
            sb.Append(Delimiter);
            sb.Append(sipperResult.AreaUnderDifferenceCurve.ToString("0.000"));
            sb.Append(Delimiter);
            sb.Append(sipperResult.AreaUnderRatioCurve.ToString("0.000"));
         
            return sb.ToString();
        }


        protected override string buildHeaderLine()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(base.buildHeaderLine());
            sb.Append(Delimiter);
            sb.Append("AreaDifferenceCurve");
            sb.Append(Delimiter);
            sb.Append("AreaRatioCurve");
            
      

            return sb.ToString();
        }

        
    }
}
