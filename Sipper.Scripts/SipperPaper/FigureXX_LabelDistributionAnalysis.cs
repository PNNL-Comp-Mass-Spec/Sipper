using System;
using System.Collections.Generic;
using System.Linq;
using DeconTools.Backend.Algorithms;
using DeconTools.Backend.Utilities;
using DeconTools.UnitTesting2;
using IsoBlender.Model;
using NUnit.Framework;

namespace Sipper.Scripts.SipperPaper
{
    [TestFixture]
    public class FigureXX_LabelDistributionAnalysis
    {
        [Test]
        public void twoDistributionsTest1()
        {
            IsotopicProfileCreator isoCreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";

            int lightIsotope = 12;
            int heavyIsotope = 13;

            double percentLabelled1 = 0;        // first peptide population is unlabelled (0%)
            double percentLabelled2 = 8;       // second peptide polulation has 8% of its carbons labelled. 
            double fractionPopulationLabelled = 0.2;     // fraction of peptides that have heavy label. 

            var iso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled1);
            var labelledIso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled2);

            var isomixture = new IsotopicProfileMixture();
            isomixture.AddIsotopicProfile(iso, 1 - fractionPopulationLabelled, "unlabelled");
            isomixture.AddIsotopicProfile(labelledIso, fractionPopulationLabelled, "labelled");

            var mixedIso = isomixture.GetMixedIsotopicProfile();


            LabelingDistributionCalculator calc = new LabelingDistributionCalculator();

            List<double> unlabelledIntensities = iso.Peaklist.Select(p => (double)p.Height).ToList();
            var labeledIntensities = mixedIso.Peaklist.Select(p => (double)p.Height).ToList();


            //calc.CalculateLabelingDistribution(unlabelledIntensities,labeledIntensities,0.01,0.01)

            double[] numLabelVals, labelDistributionVals;
            calc.CalculateLabelingDistribution(unlabelledIntensities, labeledIntensities, 0.0001, 0.0001, out numLabelVals,
                                               out labelDistributionVals);

            double fractionUnlabelled, fractionLabelled, averageLabelsIncorporated;
            calc.OutputLabelingInfo(labelDistributionVals.ToList(), out fractionUnlabelled, out fractionLabelled,
                                    out averageLabelsIncorporated);

            TestUtilities.DisplayIsotopicProfileData(mixedIso);


            Console.WriteLine();
            Console.WriteLine("-------------Label distribution Values ---------------");
            int peakCounter = 0;
            foreach (var item in labelDistributionVals)
            {
                Console.WriteLine(peakCounter + "\t" + item);
                peakCounter++;
            }



            DeconTools.Backend.Utilities.PeptideUtils peptideUtils = new PeptideUtils();
            string empiricalForm = peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptideSeq);

            int numCarbons = peptideUtils.GetNumAtomsForElement("C", empiricalForm);

            double percentIncorp = averageLabelsIncorporated/ numCarbons;



            Console.WriteLine("fractionUnLabelled=\t" + fractionUnlabelled);
            Console.WriteLine("fractionLabelled=\t" + fractionLabelled);
            Console.WriteLine("AverageLabelsIncorp=\t" + averageLabelsIncorporated);
            Console.WriteLine("PercentLabelIncorp=\t" + percentIncorp);
            Console.WriteLine("EmpiricalFormula=\t" + empiricalForm);

            
            

        }


        [Test]
        public void threeDistributionsTest1()
        {
            IsotopicProfileCreator isoCreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "C";

            int lightIsotope = 12;
            int heavyIsotope = 13;

            double percentLabelled1 = 0;        // first peptide population is unlabelled (0%)
            double percentLabelled2 = 8;       // second peptide polulation has 8% of its carbons labelled. 
            double percentLabelled3 = 16;
            
            var iso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled1);
            var labelledIso8Percent = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled2);
            var labelledIso16Percent = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled3);


            var isomixture = new IsotopicProfileMixture();
            isomixture.AddIsotopicProfile(iso, 0.80, "unlabelled");
            isomixture.AddIsotopicProfile(labelledIso8Percent, 0.15, "labelled");
            isomixture.AddIsotopicProfile(labelledIso16Percent, 0.05, "labelled16");



            var mixedIso = isomixture.GetMixedIsotopicProfile();


            LabelingDistributionCalculator calc = new LabelingDistributionCalculator();

            List<double> unlabelledIntensities = iso.Peaklist.Select(p => (double)p.Height).ToList();
            var labeledIntensities = mixedIso.Peaklist.Select(p => (double)p.Height).ToList();


            //calc.CalculateLabelingDistribution(unlabelledIntensities,labeledIntensities,0.01,0.01)

            double[] numLabelVals, labelDistributionVals;
            calc.CalculateLabelingDistribution(unlabelledIntensities, labeledIntensities, 0.0001, 0.0001, out numLabelVals,
                                               out labelDistributionVals);

            double fractionUnlabelled, fractionLabelled, averageLabelsIncorporated;
            calc.OutputLabelingInfo(labelDistributionVals.ToList(), out fractionUnlabelled, out fractionLabelled,
                                    out averageLabelsIncorporated);

           // TestUtilities.DisplayIsotopicProfileData(mixedIso);
            TestUtilities.DisplayIsotopicProfileData(labelledIso8Percent);

            Console.WriteLine();
            Console.WriteLine("-------------Label distribution Values ---------------");
            int peakCounter = 0;
            foreach (var item in labelDistributionVals)
            {
                Console.WriteLine(peakCounter + "\t" + item);
                peakCounter++;
            }



            DeconTools.Backend.Utilities.PeptideUtils peptideUtils = new PeptideUtils();
            string empiricalForm = peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptideSeq);

            int numCarbons = peptideUtils.GetNumAtomsForElement("C", empiricalForm);

            double percentIncorp = averageLabelsIncorporated / numCarbons;



            Console.WriteLine("fractionUnLabelled=\t" + fractionUnlabelled);
            Console.WriteLine("fractionLabelled=\t" + fractionLabelled);
            Console.WriteLine("AverageLabelsIncorp=\t" + averageLabelsIncorporated);
            Console.WriteLine("PercentLabelIncorp=\t" + percentIncorp);
            Console.WriteLine("EmpiricalFormula=\t" + empiricalForm);




        }


        [Test]
        public void huttlinStandardNitrogenTest1()
        {
            IsotopicProfileCreator isoCreator = new IsotopicProfileCreator();

            string peptideSeq = "SAMPLERSAMPLER";
            string elementLabelled = "N";

            int lightIsotope = 14;
            int heavyIsotope = 15;

            double percentLabelled1 = 0;        // first peptide population is unlabelled (0%)
            double percentLabelled2 = 5.2;       // second peptide polulation has 8% of its carbons labelled. 
            
            var iso = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled1);
            var labelledIso8Percent = isoCreator.CreateIsotopicProfileFromSequence(peptideSeq, elementLabelled, lightIsotope, heavyIsotope, percentLabelled2);
           
            var isomixture = new IsotopicProfileMixture();
            isomixture.AddIsotopicProfile(iso, 0.50, "unlabelled");
            isomixture.AddIsotopicProfile(labelledIso8Percent, 0.50, "labelled");
           


            var mixedIso = isomixture.GetMixedIsotopicProfile();


            LabelingDistributionCalculator calc = new LabelingDistributionCalculator();

            List<double> unlabelledIntensities = iso.Peaklist.Select(p => (double)p.Height).ToList();
            var labeledIntensities = mixedIso.Peaklist.Select(p => (double)p.Height).ToList();


            //calc.CalculateLabelingDistribution(unlabelledIntensities,labeledIntensities,0.01,0.01)

            double[] numLabelVals, labelDistributionVals;
            calc.CalculateLabelingDistribution(unlabelledIntensities, labeledIntensities, 0.0001, 0.0001, out numLabelVals,
                                               out labelDistributionVals);

            double fractionUnlabelled, fractionLabelled, averageLabelsIncorporated;
            calc.OutputLabelingInfo(labelDistributionVals.ToList(), out fractionUnlabelled, out fractionLabelled,
                                    out averageLabelsIncorporated);

             TestUtilities.DisplayIsotopicProfileData(mixedIso);
            //TestUtilities.DisplayIsotopicProfileData(labelledIso8Percent);

            Console.WriteLine();
            Console.WriteLine("-------------Label distribution Values ---------------");
            int peakCounter = 0;
            foreach (var item in labelDistributionVals)
            {
                Console.WriteLine(peakCounter + "\t" + item);
                peakCounter++;
            }



            DeconTools.Backend.Utilities.PeptideUtils peptideUtils = new PeptideUtils();
            string empiricalForm = peptideUtils.GetEmpiricalFormulaForPeptideSequence(peptideSeq);

            int numCarbons = peptideUtils.GetNumAtomsForElement("C", empiricalForm);

            double percentIncorp = averageLabelsIncorporated / numCarbons;



            Console.WriteLine("fractionUnLabelled=\t" + fractionUnlabelled);
            Console.WriteLine("fractionLabelled=\t" + fractionLabelled);
            Console.WriteLine("AverageLabelsIncorp=\t" + averageLabelsIncorporated);
            Console.WriteLine("PercentLabelIncorp=\t" + percentIncorp);
            Console.WriteLine("EmpiricalFormula=\t" + empiricalForm);




        }

    }
}
