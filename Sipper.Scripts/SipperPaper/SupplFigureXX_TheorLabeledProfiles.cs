using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.Backend.Utilities.IsotopeDistributionCalculation;
using DeconTools.UnitTesting2;
using IsoBlender.Model;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    public class SupplFigureXX_TheorLabeledProfiles
    {
        private double resolution = 40000;

        [Category("Paper")]
        [Test]
        public void GetInfoForFeature5905()
        {
            int testTarget = 5905;

            string sequence = "SAMPLERSAMPLER";

            int chargeState = 2;

            string empFormula = new PeptideUtils().GetEmpiricalFormulaForPeptideSequence(sequence);


            LcmsFeatureTarget target = new LcmsFeatureTarget();
            target.ID = 5905;
            target.ChargeState = 2;
            target.EmpiricalFormula = empFormula;
            target.Code = sequence;
            target.MonoIsotopicMass = EmpiricalFormulaUtilities.GetMonoisotopicMassFromEmpiricalFormula(empFormula);



            DeconTools.Backend.ProcessingTasks.TheorFeatureGenerator.JoshTheorFeatureGenerator theorFeatureGenerator =
                new JoshTheorFeatureGenerator();
            theorFeatureGenerator.LowPeakCutOff = 1e-10;

            theorFeatureGenerator.GenerateTheorFeature(target);
            TestUtilities.DisplayIsotopicProfileData(target.IsotopicProfile);


        }

        [Category("Paper")]
        [Test]
        public void profile0_unlabeledAndFullyLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 98.8;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0.5;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            DisplayIsotopicProfile(unlabelledTheor);

            //TestUtilities.DisplayIsotopicProfileData(unlabelledTheor);

            //TestUtilities.DisplayXYValues(xydata);

        }


        [Category("Paper")]
        [Test]
        public void profiles_partialLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            double[] labelPercents = { 0, 2, 8, 16 };

            foreach (var labelPercent in labelPercents)
            {
                var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                    heavyIsotope, labelPercent, chargeState);

                double fractionUnlabelled = 0;
                double fractionLabelled = 1 - fractionUnlabelled;

                var mixer = new IsotopicProfileMixture();
                mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
                mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

                var mixedIso = mixer.GetMixedIsotopicProfile();


                Console.WriteLine("------\t" + labelPercent);
                DisplayIsotopicProfile(mixedIso);
                Console.WriteLine();
            }

            foreach (var labelPercent in labelPercents)
            {
                var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                    heavyIsotope, labelPercent, chargeState);


                double fractionUnlabelled = 0.8;
                double fractionLabelled = 1 - fractionUnlabelled;

                var mixer = new IsotopicProfileMixture();
                mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
                mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

                var mixedIso = mixer.GetMixedIsotopicProfile();


                Console.WriteLine("------\t" + labelPercent);
                DisplayIsotopicProfile(mixedIso);
                Console.WriteLine();

            }



        }



        [Category("Paper")]
        [Test]
        public void profiles_partialLabeled90To10Mix()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            double[] labelPercents = { 0, 2, 8, 16 };

            foreach (var labelPercent in labelPercents)
            {
                var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                    heavyIsotope, labelPercent, chargeState);

                double fractionUnlabelled = 0;
                double fractionLabelled = 1 - fractionUnlabelled;

                var mixer = new IsotopicProfileMixture();
                mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
                mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

                var mixedIso = mixer.GetMixedIsotopicProfile();


                Console.WriteLine("------\t" + labelPercent);
                DisplayIsotopicProfile(mixedIso);
                Console.WriteLine();
            }

            foreach (var labelPercent in labelPercents)
            {
                var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                    heavyIsotope, labelPercent, chargeState);


                double fractionUnlabelled = 0.9;
                double fractionLabelled = 1 - fractionUnlabelled;

                var mixer = new IsotopicProfileMixture();
                mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
                mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

                var mixedIso = mixer.GetMixedIsotopicProfile();


                Console.WriteLine("------\t" + labelPercent);
                DisplayIsotopicProfile(mixedIso);
                Console.WriteLine();

            }



        }




        private void DisplayIsotopicProfile(IsotopicProfile unlabelledTheor)
        {
            StringBuilder sb = new StringBuilder();


            foreach (var msPeak in unlabelledTheor.Peaklist)
            {
                sb.Append(msPeak.XValue);
                sb.Append("\t");
                sb.Append(msPeak.Height);
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());
        }

        [Test]
        public void profile1_4percentLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 4;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);

            DisplayIsotopicProfile(mixedIso);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            //TestUtilities.DisplayXYValues(xydata);

        }

        [Test]
        public void profile2_f50unlabeled_f50_4percentLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 4;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0.5;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            TestUtilities.DisplayXYValues(xydata);

        }

        [Test]
        public void profile3_f90unlabeled_f10_4percentLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 4;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0.9;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            TestUtilities.DisplayXYValues(xydata);

        }

        [Test]
        public void profile4_f99unlabeled_f1_4percentLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 4;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0.99;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            TestUtilities.DisplayXYValues(xydata);

        }

        [Test]
        public void profile5_f0unlabeled_f100_8percentLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 8;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            TestUtilities.DisplayXYValues(xydata);

        }

        [Test]
        public void profile6_f50unlabeled_f50_8percentLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 8;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0.5;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            TestUtilities.DisplayXYValues(xydata);

        }

        [Test]
        public void profile7_f90unlabeled_f10_8percentLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 8;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0.9;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            TestUtilities.DisplayXYValues(xydata);

        }

        [Test]
        public void profile8_f99unlabeled_f1_8percentLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 8;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0.99;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            TestUtilities.DisplayXYValues(xydata);

        }

        [Test]
        public void profile9_f80unlabeled_f20_4percentLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 4;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0.80;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            TestUtilities.DisplayXYValues(xydata);

        }


        [Test]
        public void profile10_f80unlabeled_f20_8percentLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 8;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0.80;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            TestUtilities.DisplayXYValues(xydata);

        }


        [Test]
        public void profile11_f80unlabeled_f20_8percentLabeled()
        {
            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 8;
            var fullyLabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);
            double fractionUnlabelled = 0.80;
            double fractionLabelled = 1 - fractionUnlabelled;

            var mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(fullyLabelled, fractionLabelled);

            var mixedIso = mixer.GetMixedIsotopicProfile();

            //TestUtilities.DisplayIsotopicProfileData(fullyLabelled);


            double mz = mixedIso.getMonoPeak().XValue;

            double fwhm = mz / resolution;
            var xydata = TheorXYDataCalculationUtilities.GetTheoreticalIsotopicProfileXYData(mixedIso, fwhm);

            TestUtilities.DisplayXYValues(xydata);

        }


    }
}
