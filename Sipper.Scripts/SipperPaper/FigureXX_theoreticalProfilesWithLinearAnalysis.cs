using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using DeconTools.Backend.Core.Results;
using DeconTools.Backend.ProcessingTasks.Quantifiers;
using DeconTools.Backend.Runs;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using IsoBlender.Model;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Random;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FigureXX_theoreticalProfilesWithLinearAnalysis
    {
        

        [Test]
        public void showFullLabeling()
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

            TestUtilities.DisplayIsotopicProfileData(fullyLabelled);

        }
        
        
        
        [Test]
        public void OutputResultsForFraction0pt1()
        {

            double fractionUnlabelled = 0.9;
            double fractionLabelled = 0.1;

            OutputResultDetailsForSpecificFractionLabelling(fractionUnlabelled, fractionLabelled);

        }


        [Test]
        public void OutputResultsForFraction0pt1_addInterferenceToMiddlePeaks()
        {

            double fractionUnlabelled = 0.95;
            double fractionLabelled = 0.05;

            string testDataset = @"F:\Yellowstone\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            PeptideUtils peptideUtils = new PeptideUtils();
            var empFormula=   peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptideSeq);

            Console.WriteLine(peptideSeq);
            Console.WriteLine("num carbons= " + new PeptideUtils().GetNumAtomsForElement(elementLabelled, empFormula));

            double percentLabelling = 0;
            var unlabelledTheor = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);

            TestUtilities.DisplayIsotopicProfileData(unlabelledTheor);
            percentLabelling = 0.5;

            double[] percentLabelingVals = {0.5, 1, 2, 4, 5, 6, 7, 8, 9, 10};

            foreach (var percentLabelingVal in percentLabelingVals)
            {
                var labelled4percent = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelingVal, chargeState);

                //TestUtilities.DisplayIsotopicProfileData(labelled4percent);

                SipperQuantifier quantifier = new SipperQuantifier();
                quantifier.IsChromatogramCorrelationPerformed = false;

                TargetBase target = new LcmsFeatureTarget();
                target.IsotopicProfile = unlabelledTheor;
                target.Code = peptideSeq;

                run.CurrentMassTag = target;

                var mixer = new IsotopicProfileMixture();
                mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
                mixer.AddIsotopicProfile(labelled4percent, fractionLabelled);

                var mixture_90to10_0_4 = mixer.GetMixedIsotopicProfile();



                SipperLcmsTargetedResult result = new SipperLcmsTargetedResult(target);
                result.Score = 0.0001;
                result.ScanSet = new ScanSet(1);
                run.ResultCollection.CurrentTargetedResult = result;
                result.IsotopicProfile = mixture_90to10_0_4;

                result.ChromCorrelationData = new ChromCorrelationData();
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);



                quantifier.Execute(run.ResultCollection);

                var output = SipperResultUtilities.ResultDetailsToString(result, false, true, false, false);

                Console.WriteLine("percentLabel = " + percentLabelingVal + " -  no noise");
                Console.WriteLine(output);
            }


            

            

            return;

            //int peakToAlter = 7;

            //peptideSeq = "SR";
            //chargeState = 3;
            //var interferenceIso = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
            //                                                                   heavyIsotope, percentLabelling, chargeState);

            //interferenceIso.Peaklist[0].XValue = result.IsotopicProfile.Peaklist[peakToAlter-1].XValue;
           
            //for (int i = 0; i < interferenceIso.Peaklist.Count; i++)
            //{
            //    interferenceIso.Peaklist[i].XValue = interferenceIso.Peaklist[0].XValue+i*1.00245/chargeState;
            //}

            //double fractionInterference = 0.5;
            //mixer = new IsotopicProfileMixture();
            //mixer.AddIsotopicProfile(mixture_90to10_0_4, 1-fractionInterference);
            //mixer.AddIsotopicProfile(interferenceIso, fractionInterference);

            //var mixedIso = mixer.GetMixedIsotopicProfile();

            //result.IsotopicProfile = mixedIso;
             
            
            //quantifier.Execute(run.ResultCollection);
            
            //output = SipperResultUtilities.ResultDetailsToString(result,false,false,false);

            //Console.WriteLine("MIX = 90%Unlabelled + 10% Of 4%Labelled + fractionInterference= " + fractionInterference + " starting on peak " + peakToAlter);
            //Console.WriteLine(output);

            

        }



        [Test]
        public void OutputResultsForFraction0pt0_5()
        {

            double fractionUnlabelled = 0.95;
            double fractionLabelled = 0.05;

            OutputResultDetailsForSpecificFractionLabelling(fractionUnlabelled, fractionLabelled);

        }

        [Test]
        public void OutputResultsForFraction0pt0_25()
        {

            double fractionUnlabelled = 0.975;
            double fractionLabelled = 0.025;

            OutputResultDetailsForSpecificFractionLabelling(fractionUnlabelled, fractionLabelled);

        }

        [Test]
        public void OutputResultsForFraction0pt0_10()
        {

            double fractionUnlabelled = 0.99;
            double fractionLabelled = 0.01;

            OutputResultDetailsForSpecificFractionLabelling(fractionUnlabelled, fractionLabelled);

        }



        [Test]
        public void OutputResultsForFraction0pt15()
        {

            double fractionUnlabelled = 0.85;
            double fractionLabelled = 0.15;

            OutputResultDetailsForSpecificFractionLabelling(fractionUnlabelled, fractionLabelled);

        }

        [Test]
        public void CreateNoiseTest1()
        {
            double x = 0;


            int numPoints = 1000;

            List<double> data = new List<double>();


            Random random = new Random();
            

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < numPoints   ; i++)
            {
                //byte[] randomNumber = new byte[1];
                var val = random.NextFullRangeInt32()/(double) int.MaxValue;

                
                sb.Append(val);
                sb.Append(Environment.NewLine);

            }

            Console.WriteLine(sb.ToString());






        }

        [Test]
        public void CreateNoiseTest2()
        {
            double x = 0;


            int numPoints = 10000;

            List<double> data = new List<double>();


            MathNet.Numerics.Distributions.Normal normalDist = new Normal();
            
            

            Random rand = new Random();

            double mean = 100;
            double stdDev = 10d;

            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < numPoints; i++)
            {
               
                double u1 = rand.NextDouble(); //these are uniform(0,1) random doubles
                double u2 = rand.NextDouble();
                double randStdNormal = Math.Sqrt(-2.0 * Math.Log(u1)) *
                             Math.Sin(2.0 * Math.PI * u2); //random normal(0,1)
                double randNormal =
                             mean + stdDev * randStdNormal; //random normal(mean,stdDev^2)

                sb.Append(randNormal);
                sb.Append(Environment.NewLine);

            }

            Console.WriteLine(sb.ToString());






        }

        [Test]
        public void CreateGaussianNoiseTest3()
        {
            int numPoints = 10000;

            double mean = 100;
            double stdDev = 10;

            MathNet.Numerics.Distributions.Normal normalDist = new Normal(mean, stdDev);
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < numPoints; i++)
            {
                double randomGaussianValue = normalDist.Sample();

                sb.Append(randomGaussianValue);
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());

        }


        [Test]
        public void CreateIsotopicProfilesWithNoise()
        {
            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);


            int numIterations = 1000;

            List<IsotopicProfile> isoList = new List<IsotopicProfile>();
            double amountNoiseToAdd = 0.5;
            double noiseStDev = 0.5;

            double fractionUnlabelled = 0.9;
            double fractionlabelled = 0.1;
                

            MathNet.Numerics.Distributions.Normal normalDist = new Normal(amountNoiseToAdd, noiseStDev);

            percentLabelling = 4;
            var labelled4percent = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);

            PeakUtilities.TrimIsotopicProfile(labelled4percent, 0.01);


            for (int i = 0; i < numIterations; i++)
            {

                IsotopicProfileMixture mixer = new IsotopicProfileMixture();
                mixer.AddIsotopicProfile(unlabelled, fractionUnlabelled);
                mixer.AddIsotopicProfile(labelled4percent, fractionlabelled);

                var mixedIso = mixer.GetMixedIsotopicProfile();

                addNoiseToIsotopicProfile(mixedIso, normalDist);

                isoList.Add(mixedIso);
            }

            StringBuilder sb = new StringBuilder();

            foreach (var isotopicProfile in isoList)
            {
                //int numPeaksToReport = isotopicProfile.Peaklist.Count ;
                //for (int i = 0; i < numPeaksToReport; i++)
                //{
                //    sb.Append(isotopicProfile.Peaklist[i].Height);
                //    sb.Append("\t");
                //}

                //sb.Append(Environment.NewLine);



                SipperQuantifier quantifier = new SipperQuantifier();
                quantifier.IsChromatogramCorrelationPerformed = false;

                TargetBase target = new LcmsFeatureTarget();
                target.IsotopicProfile = unlabelled;
                target.Code = peptideSeq;

                run.CurrentMassTag = target;


                SipperLcmsTargetedResult result = new SipperLcmsTargetedResult(target);
                result.Score = 0.0001;
                result.ScanSet = new ScanSet(1);
                result.IsotopicProfile = isotopicProfile;
                run.ResultCollection.CurrentTargetedResult = result;

                result.ChromCorrelationData = new ChromCorrelationData();
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
                quantifier.Execute(run.ResultCollection);


                //sb.Append(result.RSquaredValForRatioCurve);
                //sb.Append("\t");
                sb.Append(result.AreaUnderRatioCurveRevised);
                sb.Append("\t");
                sb.Append(result.PercentPeptideLabelled);
                sb.Append("\t");
                sb.Append(result.NumCarbonsLabelled);
                sb.Append(Environment.NewLine);

                string output = SipperResultUtilities.ResultDetailsToString(result);



            }

            Console.WriteLine(sb.ToString());

            //TestUtilities.DisplayIsotopicProfileData(unlabelled);

        }


        [Test]
        public void CreateIsotopicProfilesWithNoise2()
        {
            string testDataset =
                @"\\protoapps\DataPkgs\Public\2012\601_Sipper_paper_data_processing_and_analysis\RawData\Yellow_C13_070_23Mar10_Griffin_10-01-28.raw";
            Run run = new RunFactory().CreateRun(testDataset);


            IsoBlender.Model.IsotopicProfileCreator isocreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";
            int lightIsotope = 12;
            int heavyIsotope = 13;
            int chargeState = 2;

            double percentLabelling = 0;
            var unlabelled = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                               heavyIsotope, percentLabelling, chargeState);


            int numIterations = 1000;

            List<IsotopicProfile> isoList = new List<IsotopicProfile>();
            double amountNoiseToAdd = 0.01;
            double noiseStDev = 0.01;

            double fractionUnlabelled = 0.5;
            double fractionlabelled = 1- fractionUnlabelled;


            MathNet.Numerics.Distributions.Normal normalDist = new Normal(amountNoiseToAdd, noiseStDev);

            percentLabelling = 30;
            var labeledProfile = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);

            PeakUtilities.TrimIsotopicProfile(labeledProfile, 0.01);


            for (int i = 0; i < numIterations; i++)
            {

                IsotopicProfileMixture mixer = new IsotopicProfileMixture();
                mixer.AddIsotopicProfile(unlabelled, fractionUnlabelled);
                mixer.AddIsotopicProfile(labeledProfile, fractionlabelled);

                var mixedIso = mixer.GetMixedIsotopicProfile();

                addNoiseToIsotopicProfile(mixedIso, normalDist);

                isoList.Add(mixedIso);
            }

            StringBuilder sb = new StringBuilder();

            var firstIso = isoList.First();

            TestUtilities.DisplayIsotopicProfileData(firstIso);


            foreach (var isotopicProfile in isoList)
            {
                int numPeaksToReport = 35;
                for (int i = 0; i < numPeaksToReport; i++)
                {
                    if (i>=isotopicProfile.Peaklist.Count)
                    {
                        sb.Append(0);
                    }
                    else
                    {
                        sb.Append(isotopicProfile.Peaklist[i].Height);    
                    }
                    
                    sb.Append("\t");
                }

                sb.Append(Environment.NewLine);
                
            }

            Console.WriteLine(sb.ToString());

            //TestUtilities.DisplayIsotopicProfileData(unlabelled);

        }




        private void addNoiseToIsotopicProfile(IsotopicProfile iso, Normal normalDist)
        {
            foreach (var msPeak in iso.Peaklist)
            {
                msPeak.Height = (float) (msPeak.Height + normalDist.Sample());

                msPeak.Height = Math.Max(0, msPeak.Height);

            }
        }

       


        private static void OutputResultDetailsForSpecificFractionLabelling(double fractionUnlabelled, double fractionLabelled)
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

            percentLabelling = 0;
            var labelled0percent = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);


            percentLabelling = 0.5;
            var labelled0pt5percent = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                   heavyIsotope, percentLabelling, chargeState);


            percentLabelling = 1;
            var labelled1percent = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);


            percentLabelling = 2;
            var labelled2percent = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 4;
            var labelled4percent = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 6;
            var labelled6percent = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 8;
            var labelled8percent = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 10;
            var labelled10percent = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                 heavyIsotope, percentLabelling, chargeState);

            percentLabelling = 16;
            var labelled16percent = isocreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope,
                                                                                 heavyIsotope, percentLabelling, chargeState);


            IsotopicProfileMixture mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(labelled0pt5percent, fractionLabelled);

            var mixture_90to10_0_0pt5 = mixer.GetMixedIsotopicProfile();


            //TestUtilities.DisplayIsotopicProfileData(mixture_90to10_0_2);

            SipperQuantifier quantifier = new SipperQuantifier();
            quantifier.IsChromatogramCorrelationPerformed = false;

            TargetBase target = new LcmsFeatureTarget();
            target.IsotopicProfile = unlabelledTheor;
            target.Code = peptideSeq;

            run.CurrentMassTag = target;


            SipperLcmsTargetedResult result = new SipperLcmsTargetedResult(target);
            result.Score = 0.0001;
            result.ScanSet = new ScanSet(1);
            result.IsotopicProfile = mixture_90to10_0_0pt5;
            run.ResultCollection.CurrentTargetedResult = result;

            result.ChromCorrelationData = new ChromCorrelationData();
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            result.ChromCorrelationData.AddCorrelationData(1, 0, 0.9999);
            quantifier.Execute(run.ResultCollection);


            string output = SipperResultUtilities.ResultDetailsToString(result);

            Console.WriteLine("MIX = 90%Unlabelled + 10% Of 0.5%Labelled");
            Console.WriteLine(output);

            //----------------------------------------------------------------------------
            mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(labelled1percent, fractionLabelled);

            var mixture_90to10_0_1 = mixer.GetMixedIsotopicProfile();
            result.IsotopicProfile = mixture_90to10_0_1;
            quantifier.Execute(run.ResultCollection);

            output = SipperResultUtilities.ResultDetailsToString(result);

            Console.WriteLine("MIX = 90%Unlabelled + 10% Of 1%Labelled");
            Console.WriteLine(output);


            //----------------------------------------------------------------------------
            mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(labelled2percent, fractionLabelled);

            var mixture_90to10_0_2 = mixer.GetMixedIsotopicProfile();
            result.IsotopicProfile = mixture_90to10_0_2;
            quantifier.Execute(run.ResultCollection);

            output = SipperResultUtilities.ResultDetailsToString(result);

            Console.WriteLine("MIX = 90%Unlabelled + 10% Of 2%Labelled");
            Console.WriteLine(output);


            //----------------------------------------------------------------------------
            mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(labelled4percent, fractionLabelled);

            var mixture_90to10_0_4 = mixer.GetMixedIsotopicProfile();
            result.IsotopicProfile = mixture_90to10_0_4;
            quantifier.Execute(run.ResultCollection);

            output = SipperResultUtilities.ResultDetailsToString(result);

            Console.WriteLine("MIX = 90%Unlabelled + 10% Of 4%Labelled");
            Console.WriteLine(output);

            //----------------------------------------------------------------------------
            mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(labelled6percent, fractionLabelled);

            var mixture_90to10_0_6 = mixer.GetMixedIsotopicProfile();
            result.IsotopicProfile = mixture_90to10_0_6;
            quantifier.Execute(run.ResultCollection);

            output = SipperResultUtilities.ResultDetailsToString(result);

            Console.WriteLine("MIX = 90%Unlabelled + 10% Of 6%Labelled");
            Console.WriteLine(output);


            //----------------------------------------------------------------------------
            mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(labelled8percent, fractionLabelled);

            var mixture_90to10_0_8 = mixer.GetMixedIsotopicProfile();
            result.IsotopicProfile = mixture_90to10_0_8;
            quantifier.Execute(run.ResultCollection);

            output = SipperResultUtilities.ResultDetailsToString(result);

            Console.WriteLine("MIX = 90%Unlabelled + 10% Of 8%Labelled");
            Console.WriteLine(output);


            //----------------------------------------------------------------------------
            mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(labelled10percent, fractionLabelled);

            var mixture_90to10_0_10 = mixer.GetMixedIsotopicProfile();
            result.IsotopicProfile = mixture_90to10_0_10;
            quantifier.Execute(run.ResultCollection);

            output = SipperResultUtilities.ResultDetailsToString(result);

            Console.WriteLine("MIX = 90%Unlabelled + 10% Of 10%Labelled");
            Console.WriteLine(output);


            //----------------------------------------------------------------------------
            mixer = new IsotopicProfileMixture();
            mixer.AddIsotopicProfile(unlabelledTheor, fractionUnlabelled);
            mixer.AddIsotopicProfile(labelled0percent, fractionLabelled);

            var mixture_90to10_0_0 = mixer.GetMixedIsotopicProfile();
            result.IsotopicProfile = mixture_90to10_0_0;
            quantifier.Execute(run.ResultCollection);

            output = SipperResultUtilities.ResultDetailsToString(result);

            Console.WriteLine("MIX = 90%Unlabelled + 10% Of 0%Labelled");
            Console.WriteLine(output);
        }
    }
}
