using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DeconTools.Backend.Core;
using GwsDMSUtilities;
using NUnit.Framework;

namespace Sipper.Scripts
{
    [TestFixture]
    public class GordPaperScripts
    {
        [Test]
        public void ExamineGroL()
        {
            int refID1 = 38803;
            //int refID2 = 39890;

            refID1 = 40198;  //GroL

            string dbname = "MT_Yellowstone_Communities_P627";
            string serverName = "pogo";
            var infoExtractor = new PeptideInfoExtractor(serverName, dbname);


            List<PeptideTarget> allPeptides = new List<PeptideTarget>();

            var peptideList = infoExtractor.GetMassTagIDsForGivenProtein(refID1);
            allPeptides.AddRange(peptideList);


            StringBuilder sb = new StringBuilder();

            foreach (var peptideTarget in peptideList)
            {

                sb.Append(peptideTarget.ID);
                sb.Append(',');
                sb.Append(Environment.NewLine);
            }

            Console.WriteLine(sb.ToString());



        }

    }
}
