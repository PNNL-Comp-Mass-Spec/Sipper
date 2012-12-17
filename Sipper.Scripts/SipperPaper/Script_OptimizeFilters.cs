using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend;
using DeconTools.Workflows.Backend.FileIO;
using DeconTools.Workflows.Backend.Results;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class Script_OptimizeFilters
    {
        [Test]
        public void ParameterOptimization_forLabeledPeptides()
        {

            string manualResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_ALL_validated.txt";


            string autoResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_AutomatedAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";


            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(manualResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(autoResultsFile);
            var autoResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
            var noResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.No).ToList();
            var maybeResults = originalResults.Where(p => p.ValidationCode == ValidationCode.Maybe).ToList();

            var manualResults = yesResultsOnly;

            double maxRatio = 0;


            StringBuilder sb = new StringBuilder();

            List<ParameterOptimizationDataItem> optimizationData = new List<ParameterOptimizationDataItem>();

            sb.Append("rsquared\tarea\tiscore\tshared\tuniqueToAuto\tratio\n");

            for (double rsquaredVal = 0.55; rsquaredVal < 1.1; rsquaredVal = rsquaredVal + 0.01)
            {
                for (double area = 0; area < 10.5; area = area + 0.5)
                {
                    for (double iscore = 0; iscore < 0.50; iscore = iscore + 0.02)
                    {
                        for (double chromCorr = 0.5; chromCorr <= 1.0; chromCorr = chromCorr + 0.05)
                        {

                            var filteredResults = (from n in autoResults
                                                     where n.IScore <= iscore &&
                                                           n.AreaUnderRatioCurveRevised >= area &&
                                                           n.RSquaredValForRatioCurve >= rsquaredVal &&
                                                           n.ChromCorrelationMedian >= chromCorr
                                                     select n).ToList();

                            var intersectResults = filteredResults.Select(p => p.TargetID).Intersect(manualResults.Select(p => p.TargetID)).ToList();
                            var uniqueToAuto = filteredResults.Select(p => p.TargetID).Except(manualResults.Select(p => p.TargetID)).ToList();
                            var ratio = intersectResults.Count / (double)uniqueToAuto.Count;


                            ParameterOptimizationDataItem optimizationDataItem = new ParameterOptimizationDataItem(rsquaredVal, area, iscore, chromCorr,
                                                                  intersectResults.Count, uniqueToAuto.Count);


                            if (double.IsNaN(ratio))
                            {
                                ratio = -1;
                            }

                            optimizationData.Add(optimizationDataItem);

                            sb.Append(rsquaredVal + "\t" + area + "\t" + iscore + "\t" + chromCorr + "\t" + intersectResults.Count + "\t" + uniqueToAuto.Count + "\t" + ratio + "\n");


                        }
                    }



                }

            }


            int maxNumUniqueToAuto = optimizationData.Select(p => p.UniqueToAuto).Max();


            for (int i = 0; i <= maxNumUniqueToAuto; i++)
            {
                var maxSharedVal = optimizationData.Where(p => p.UniqueToAuto == i).Select(p => p.SharedCount).Max();

                Console.WriteLine(i + "\t" + maxSharedVal);


            }


            Console.WriteLine(sb.ToString());
        }


        [Test]
        public void ParameterOptimization_forLabeledPeptides_testing()
        {

            string manualResultsFile =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ManualAnalysisOfDataset\Yellow_C13_070_23Mar10_Griffin_10-01-28_results_ALL_validated.txt";


            string autoResultsFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Yellow_C13_070_23Mar10_Griffin_10-01-28_results.txt";

            autoResultsFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Results\Yellow_C13_070_23Mar10_Griffin_10-01-28_rsquared1_results.txt";



            SipperResultFromTextImporter importer = new SipperResultFromTextImporter(manualResultsFile);
            var originalResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            importer = new SipperResultFromTextImporter(autoResultsFile);
            var autoResults = (from SipperLcmsFeatureTargetedResultDTO n in importer.Import().Results select n).ToList();

            var yesResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.Yes).ToList();
            var noResultsOnly = originalResults.Where(p => p.ValidationCode == ValidationCode.No).ToList();
            var maybeResults = originalResults.Where(p => p.ValidationCode == ValidationCode.Maybe).ToList();

            var manualResults = yesResultsOnly;

            double maxRatio = 0;


            StringBuilder sb = new StringBuilder();

            List<ParameterOptimizationDataItem> optimizationData = new List<ParameterOptimizationDataItem>();

            sb.Append("rsquared\tarea\tiscore\tshared\tuniqueToAuto\tratio\n");

            for (double rsquaredVal = 0.55; rsquaredVal < 1.1; rsquaredVal = rsquaredVal + 0.01)
            {
                for (double area = 0; area < 10.5; area = area + 0.5)
                {
                    for (double iscore = 0; iscore < 0.50; iscore = iscore + 0.02)
                    {
                        for (double chromCorr = 0.5; chromCorr <= 1.0; chromCorr = chromCorr + 0.05)
                        {

                            var filteredResults = (from n in autoResults
                                                   where n.IScore <= iscore &&
                                                         n.AreaUnderRatioCurveRevised >= area &&
                                                         n.RSquaredValForRatioCurve >= rsquaredVal &&
                                                         n.ChromCorrelationMedian >= chromCorr
                                                   select n).ToList();

                            var intersectResults = filteredResults.Select(p => p.TargetID).Intersect(manualResults.Select(p => p.TargetID)).ToList();
                            var uniqueToAuto = filteredResults.Select(p => p.TargetID).Except(manualResults.Select(p => p.TargetID)).ToList();
                            var ratio = intersectResults.Count / (double)uniqueToAuto.Count;


                            ParameterOptimizationDataItem optimizationDataItem = new ParameterOptimizationDataItem(rsquaredVal, area, iscore, chromCorr,
                                                                  intersectResults.Count, uniqueToAuto.Count);


                            if (double.IsNaN(ratio))
                            {
                                ratio = -1;
                            }

                            optimizationData.Add(optimizationDataItem);

                            sb.Append(rsquaredVal + "\t" + area + "\t" + iscore + "\t" + chromCorr + "\t" + intersectResults.Count + "\t" + uniqueToAuto.Count + "\t" + ratio + "\n");


                        }
                    }



                }

            }


            int maxNumUniqueToAuto = optimizationData.Select(p => p.UniqueToAuto).Max();


            for (int i = 0; i <= maxNumUniqueToAuto; i++)
            {
                var maxSharedVal = optimizationData.Where(p => p.UniqueToAuto == i).Select(p => p.SharedCount).Max();

                Console.WriteLine(i + "\t" + maxSharedVal);


            }


            Console.WriteLine(sb.ToString());
        }



    }
}
