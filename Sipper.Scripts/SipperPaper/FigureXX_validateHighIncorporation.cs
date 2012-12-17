using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Utilities;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FigureXX_validateHighIncorporation
    {
        [Test]
        public void Test1()
        {
            string testDataset =
          @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            
            string outputFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ValidateHighIncorporation";


            var run = RunUtilities.CreateAndLoadPeaks(testDataset);

            Figure03_FeatureElutionScripts.ElutionInputData f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            f1.FeatureID = 12379;
            f1.MinScan = 6250;
            f1.MaxScan = 6450;
            f1.MinMZ = 953;
            f1.MaxMZ = 960;

            Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);

            f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            f1.FeatureID = 12379999;
            f1.MinScan = 6250;
            f1.MaxScan = 6450;
            f1.MinMZ = 950;
            f1.MaxMZ = 954;

            Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);



        }

        [Test]
        public void Test2()
        {
            string testDataset =
          @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            string outputFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ValidateHighIncorporation";


            var run = RunUtilities.CreateAndLoadPeaks(testDataset);

            Figure03_FeatureElutionScripts.ElutionInputData f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            f1.FeatureID = 11249;
            f1.MinScan = 8560;
            f1.MaxScan = 8800;
            f1.MinMZ = 890;
            f1.MaxMZ = 895;

            Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);

            f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            f1.FeatureID = 11249999;
            f1.MinScan = 8560;
            f1.MaxScan = 8800;
            f1.MinMZ = 893;
            f1.MaxMZ = 900;

            Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);



        }

        [Test]
        public void Test3()
        {
            string testDataset =
          @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            string outputFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ValidateHighIncorporation";


            var run = RunUtilities.CreateAndLoadPeaks(testDataset);

            Figure03_FeatureElutionScripts.ElutionInputData f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            f1.FeatureID = 8910;
            f1.MinScan = 6920;
            f1.MaxScan = 7200;
            f1.MinMZ = 786;
            f1.MaxMZ = 789;

            Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);

            f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            f1.FeatureID = 89109999;
            f1.MinScan = 6920;
            f1.MaxScan = 7200;
            f1.MinMZ = 788;
            f1.MaxMZ = 796;

            Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);



        }

        [Test]
        public void Test4()
        {
            string testDataset =
          @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";

            string outputFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ValidateHighIncorporation";


            var run = RunUtilities.CreateAndLoadPeaks(testDataset);

            Figure03_FeatureElutionScripts.ElutionInputData f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            f1.FeatureID = 7167;
            f1.MinScan = 7100;
            f1.MaxScan = 7350;
            f1.MinMZ = 844;
            f1.MaxMZ = 848;

            Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);

            f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            f1.FeatureID = 71679999;
            f1.MinScan = 7100;
            f1.MaxScan = 7350;
            f1.MinMZ = 846;
            f1.MaxMZ = 854;

            Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);



        }


        [Test]
        public void Test5()
        {
            string testDataset =
                @"F:\Yellowstone\RawData\Yellow_SIP_13-11_HL_14Aug12_Falcon_12-06-02.RAW";

            string outputFolder =
                @"C:\Users\d3x720\Documents\PNNL\My_Manuscripts\Manuscript08_Sipper_C13\Data_Analysis\FigureXX_ValidateHighIncorporation";


            var run = RunUtilities.CreateAndLoadPeaks(testDataset);

            Figure03_FeatureElutionScripts.ElutionInputData f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            f1.FeatureID = 13414;
            f1.MinScan = 5450;
            f1.MaxScan = 5650;
            f1.MinMZ = 406;
            f1.MaxMZ = 418;

            Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);

            //f1 = new Figure03_FeatureElutionScripts.ElutionInputData();
            //f1.FeatureID = 71679999;
            //f1.MinScan = 7100;
            //f1.MaxScan = 7350;
            //f1.MinMZ = 846;
            //f1.MaxMZ = 854;

            //Figure03_FeatureElutionScripts.ExtractElutionProfile(outputFolder, run, f1);



        }

        [Test]
        public void HeavyLabelingInTimeCourse1()
        {
            //feature 16865 in Yellow_SIP_13-11_HL_14Aug12_Falcon_12-06-02_results - Copy

            DeconTools.Backend.Utilities.PeptideUtils peptideUtils = new PeptideUtils();

            string seq = "IDRNLLR";
            var empFormula=  peptideUtils.GetEmpiricalFormulaForPeptideSequence(seq);
            var numCarbons = peptideUtils.GetNumAtomsForElement("C", empFormula);

            Console.WriteLine(seq + "\tnum carbons= " + numCarbons);

            seq = "IDRNILR";
            empFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(seq);
            numCarbons = peptideUtils.GetNumAtomsForElement("C", empFormula);

            Console.WriteLine(seq + "\tnum carbons= " + numCarbons);

            seq = "RILENVR";
            empFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(seq);
            numCarbons = peptideUtils.GetNumAtomsForElement("C", empFormula);

            Console.WriteLine(seq + "\tnum carbons= " + numCarbons);

            seq = "RLLENVR";
            empFormula = peptideUtils.GetEmpiricalFormulaForPeptideSequence(seq);
            numCarbons = peptideUtils.GetNumAtomsForElement("C", empFormula);

            Console.WriteLine(seq + "\tnum carbons= " + numCarbons);


        }

        




    }
}
