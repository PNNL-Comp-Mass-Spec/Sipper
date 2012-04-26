using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DeconTools.Workflows.Backend.Utilities;
using NUnit.Framework;

namespace Sipper.Scripts
{
    [TestFixture]
    public class ResultHandingScripts
    {
        [Test]
        public void mergeResults1()
        {
            string resultFolder = @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\Yellow_C12";
            string outputFile = @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration05_Sum05_newColumns\Yellow_C12\_Yellow_C12_MergedResults.txt";
            ResultUtilities.MergeResultFiles(resultFolder, outputFile);

        }


        [Test]
        public void EditHeaderLine1()
        {
            string newHeader = @"Dataset	MassTagID	ChargeState	Scan	ScanStart	ScanEnd	NumMSSummed	NET	NETError	NumChromPeaksWithinTol	NumQualityChromPeaksWithinTol	MonoisotopicMass	MonoisotopicMassCalibrated	MassErrorInPPM	MonoMZ	IntensityRep	FitScore	IScore	FailureType	ErrorDescription	MatchedMassTagID	AreaDifferenceCurve	AreaRatioCurve	AreaRatioCurveRevised	RSquared	ChromCorrMin	ChromCorrMax	ChromCorrAverage	ChromCorrMedian";

            string resultFolder = @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration03_Sum05\Yellow_C13";
            string editedResultFolder = @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results\Iteration03_Sum05\Yellow_C13_edited";


            if (!Directory.Exists(editedResultFolder)) Directory.CreateDirectory(editedResultFolder);

            DirectoryInfo resultDirinfo = new DirectoryInfo(resultFolder);

            FileInfo[] resultFiles = resultDirinfo.GetFiles("*_results.txt");

            foreach (var file in resultFiles)
            {
                using (StreamReader reader = new StreamReader(file.FullName))
                {

                    string newOutputfileName = editedResultFolder + Path.DirectorySeparatorChar + file.Name;

                    using (StreamWriter writer = new StreamWriter(newOutputfileName))
                    {

                        string line = reader.ReadLine();
                        writer.WriteLine(newHeader);


                        while (reader.Peek() != -1)
                        {
                            line = reader.ReadLine();
                            writer.WriteLine(line);
                        }


                        writer.Close();
                        
                    }

                    reader.Close();

   
                }


            }



        }



        [Test]
        public void checkForMissingResultsTest1()
        {
            string resultsFolder = @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\Results";
            SipperResultUtilities.CheckForMissingResults(resultsFolder);
        }


        [Test]
        public void displayDatasetPathsTest1()
        {
            SipperDatasetUtilities.DisplayDatasetPaths();
        }

    }
}
