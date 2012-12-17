using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Sipper.Scripts.SipperPaper
{
    public class Script_GetAminoAcidFrequencies
    {

        public static void DisplayAminoAcidFrequencies(IEnumerable<string>peptideSequences)
        {

            Console.WriteLine("Total sequences = \t" + peptideSequences.Count());


            string aminoacidCodes = "GALMFWKQESPVICYHRNDT";


            //string aggregateSequence = peptideSequences.Aggregate((i, j) => i + j);

            StringBuilder stringBuilder = new StringBuilder();
            foreach (var peptideSequence in peptideSequences)
            {
                stringBuilder.Append(peptideSequence);
            }

            string aggregateSequence = stringBuilder.ToString();

            // Console.WriteLine(aggregateSequence);
            Console.WriteLine();
            Console.WriteLine("AminoAcid\tFreq");
            foreach (var aminoacidCode in aminoacidCodes)
            {
                int freq = aggregateSequence.Count(p => p == aminoacidCode);
                Console.WriteLine(aminoacidCode + "\t" + freq);
            }

            Console.WriteLine();
            Console.WriteLine();
            
        }


        public static void DisplayAminoAcidFrequencies(string datafile)
        {
            Console.WriteLine("-----------------------------------------------------------------------------------");
            Console.WriteLine("Filename = \t" + Path.GetFileName(datafile));

            List<string> peptideSequences = new List<string>();

            using (StreamReader reader = new StreamReader(datafile))
            {
                while (reader.Peek() != -1)
                {
                    peptideSequences.Add(reader.ReadLine());
                }

                reader.Close();
            }

            DisplayAminoAcidFrequencies(peptideSequences);

            //peptideSequences = peptideSequences.Take(2).ToList();

            //peptideSequences = peptideSequences.Distinct().ToList();


        }
    }
}
