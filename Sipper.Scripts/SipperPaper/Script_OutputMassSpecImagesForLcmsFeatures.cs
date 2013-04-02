using System;
using System.IO;
using System.Linq;
using DeconTools.Backend.Core;
using DeconTools.Backend.Data;
using DeconTools.Backend.FileIO;
using DeconTools.Backend.ProcessingTasks;
using DeconTools.Backend.ProcessingTasks.MSGenerators;
using DeconTools.Backend.Runs;
using GWSGraphLibrary;
using NUnit.Framework;
using ZedGraph;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class Script_OutputMassSpecImagesForLcmsFeatures
    {
        [Category("Paper")]
        [Test]
        public void FilterForUnidentifiedFeaturesAndOutput()
        {
            var datasetNames = SipperDatasetUtilities.GetDatasetNames();

            string targetsFolder = @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\STAC_Filtered";

            string originalLCMSFeaturesFolder = @"F:\Yellowstone\IQTargets";

            string unidentifiedTargetsOutputFolder = @"F:\Yellowstone\IQTargets\UnidentifiedTargets";

            string outputFolder = @"F:\Yellowstone\LcmsFeatureMassSpectraSamples";

            //datasetNames = datasetNames.Where(p => p.Contains("Yellow_C13_070")).ToList();


            string rawdataFile = @"D:\Data\Sipper\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            string unidentifiedFeaturesFile =
                @"F:\Yellowstone\IQTargets\UnidentifiedTargets\Yellow_C12_070_21Feb10_Griffin_09-11-40_targets.txt";

            string identifiedFeaturesFile =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\Targets\STAC_Filtered\Yellow_C13_070_23Mar10_Griffin_10-01-28_targets.txt";


            DeconTools.Backend.FileIO.LcmsTargetFromFeaturesFileImporter importer = new LcmsTargetFromFeaturesFileImporter(identifiedFeaturesFile);
            var identifiedLcmsFeatures = importer.Import().TargetList;

            importer = new LcmsTargetFromFeaturesFileImporter(identifiedFeaturesFile);
            var unidentifiedLcmsFeatures = importer.Import().TargetList;


            string umcFile = @"F:\Yellowstone\IQTargets\Yellow_C13_070_23Mar10_Griffin_10-01-28_UMCs.txt";
            DeconTools.Backend.Data.UMCFileImporter umcFileImporter = new UMCFileImporter(umcFile, '\t');



            var idsIdentified = identifiedLcmsFeatures.Select(p => p.ID).ToList();
            var idsUnidentified = unidentifiedLcmsFeatures.Select(p => p.ID).ToList();


            var umcs = umcFileImporter.Import().UMCList;


            var filteredUmcs = (from n in umcs where n.UMCMonoMW > 1000 select n).OrderByDescending(p => p.UMCAbundance).ToList().Take(3000);


            ScanSetFactory scanSetFactory = new ScanSetFactory();
            Run run = new RunFactory().CreateRun(rawdataFile);


            MSGenerator msgen = MSGeneratorFactory.CreateMSGenerator(run.MSFileType);

            var graphGenerator = new BasicGraphControl();
            graphGenerator.GraphWidth = 900;
            graphGenerator.GraphHeight = 600;


            int binCounter = 0;


            int featureCounter = 0;

            int binSize = 500;


            foreach (UMC filteredUmc in filteredUmcs)
            {
                

                featureCounter++;

                if (featureCounter>=binSize)
                {
                    binCounter++;
                    featureCounter = 0;
                }

                Console.WriteLine("bin " + binCounter + " feature " + featureCounter);


                ScanSet scan = scanSetFactory.CreateScanSet(run, filteredUmc.ScanClassRep, 5);

                var xydata = msgen.GenerateMS(run, scan);

                double targetMZ = filteredUmc.UMCMZForChargeBasis;

                xydata = xydata.TrimData(targetMZ - 2, targetMZ + 20);

                var monoPeakXYData = xydata.TrimData(targetMZ - 0.1, targetMZ + 0.1);

                double intensityMono = monoPeakXYData.getMaxY();

                graphGenerator.GenerateGraph(xydata.Xvalues, xydata.Yvalues);
                var line = graphGenerator.GraphPane.CurveList[0] as LineItem;
                line.Line.IsVisible = true;
                line.Symbol.Size = 2;
                line.Symbol.Type = SymbolType.None;
                graphGenerator.GraphPane.XAxis.Title.Text = "m/z";
                graphGenerator.GraphPane.YAxis.Title.Text = "intensity";
                graphGenerator.GraphPane.XAxis.Title.FontSpec.Size = 12;
                graphGenerator.GraphPane.YAxis.Title.FontSpec.Size = 12;
                graphGenerator.GraphPane.XAxis.Scale.FontSpec.Size = 12;
                graphGenerator.GraphPane.YAxis.Scale.FontSpec.Size = 12;
                graphGenerator.GraphPane.X2Axis.IsVisible = false;
                graphGenerator.GraphPane.Y2Axis.IsVisible = false;

                graphGenerator.GraphPane.YAxis.Scale.Max = intensityMono + intensityMono*0.2;    
                graphGenerator.AddAnnotationAbsoluteXRelativeY("*", targetMZ-0.1, 0.075);
               

                string currentOutputFolder = outputFolder + "\\" + "Bin" + binCounter.ToString().PadLeft(2, '0');

                if (!Directory.Exists(currentOutputFolder)) Directory.CreateDirectory(currentOutputFolder);

                string outputImagename = currentOutputFolder + "\\" + "Feature" + filteredUmc.UMCIndex.ToString().PadLeft(5, '0') + "_scan"+ scan.PrimaryScanNumber + "_mz"+ targetMZ.ToString("0.000")  + "_MS.png";

                graphGenerator.SaveGraph(outputImagename);


            }







        }



    }
}
