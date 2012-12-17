using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Core;
using DeconTools.Backend.Utilities;
using GWSGraphLibrary.GraphGenerator;
using NUnit.Framework;

namespace Sipper.Scripts
{
    [TestFixture]
    public class ChromatogramDisplayScripts
    {
        private List<int> msLevel1ScanList;

        [Test]
        public void Test1()
        {

            string datasetFile = @"D:\Data\Temp\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            string peakFile = @"D:\Data\Temp\Yellow_C13_070_23Mar10_Griffin_10-01-28_peaks.txt";

            string outputFolder = @"D:\Data\Temp\Visuals";

            if (!Directory.Exists(outputFolder)) Directory.CreateDirectory(outputFolder);

            //initialize run
            Run run=  RunUtilities.CreateAndLoadPeaks(datasetFile, peakFile);

            //load results
            var results=   SipperResultUtilities.LoadResultsForDataset(run.DatasetName);

            var peakChromGen = new ChromatogramGenerator();


            //results.Results = results.Results.Take(100).ToList();

            results.Results = results.Results.Where(p => p.TargetID == 8517).ToList();


            //iterate over results 
            foreach (var r in results.Results)
            {
                //generate chromatogram

                int LCElutionWindowWidth = 300;
                int startScan = Math.Max(run.MinLCScan, r.ScanLC - LCElutionWindowWidth / 2);
                int stopScan = Math.Min(run.MaxLCScan, r.ScanLC + LCElutionWindowWidth / 2);

                run.XYData = peakChromGen.GenerateChromatogram(run.ResultCollection.MSPeakResultList, startScan, stopScan, r.MonoMZ, 20);

                filterOutMSMSData(run, run.XYData);


                ChromatogramGraphGenerator gen = new ChromatogramGraphGenerator();
                gen.XAxisTitle = "LC index";
                gen.SymbolSize = 3;

                gen.Width = 500;
                gen.Height = 400;


                double lcScanWidthToDisplay = 300;

                double lcScanMin = r.ScanLC - lcScanWidthToDisplay / 2;
                double lcScanMax = r.ScanLC + lcScanWidthToDisplay / 2;


                if (run.XYData!=null)
                {
                    gen.GenerateGraph(run.XYData.Xvalues, run.XYData.Yvalues, lcScanMin, lcScanMax);
                    gen.AddVerticalLineToGraph(r.ScanLC);

                    gen.AddRectangleToGraph(r.ScanLCStart, r.ScanLCEnd);

                    string outputFilename = outputFolder + Path.DirectorySeparatorChar + r.TargetID + ".png";
                    if (File.Exists(outputFilename)) File.Delete(outputFilename);
                    gen.SaveGraph(outputFilename);
                    
                }

                

                
            }


            



        }



        private void filterOutMSMSData(Run run, DeconTools.Backend.XYData xyData)
        {

            if (xyData == null || xyData.Xvalues == null) return;

            if (run.ContainsMSMSData)
            {
                if (msLevel1ScanList == null)
                {
                    msLevel1ScanList = run.GetMSLevelScanValues();
                }

                Dictionary<int, double> filteredChromVals = new Dictionary<int, double>();



                for (int i = 0; i < xyData.Xvalues.Length; i++)
                {
                    int currentScanVal = (int)xyData.Xvalues[i];

                    if (msLevel1ScanList.Contains(currentScanVal))
                    {
                        filteredChromVals.Add(currentScanVal, xyData.Yvalues[i]);
                    }
                }


                xyData.Xvalues = filteredChromVals.Keys.Select<int, double>(i => i).ToArray();
                xyData.Yvalues = filteredChromVals.Values.ToArray();

            }
        }


    }
}
