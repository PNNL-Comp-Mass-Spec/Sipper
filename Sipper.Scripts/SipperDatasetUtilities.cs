using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using DeconTools.Workflows.Backend.Utilities;

namespace Sipper.Scripts
{
    public class SipperDatasetUtilities
    {
        public static List<string>GetDatasetNames()
        {
            string fileContainingDatasetnames =
                @"\\protoapps\UserData\Slysz\Data\Yellowstone\SIPPER\datasetNames_masterList.txt";

            List<string> datasetNameList = new List<string>();

            using (StreamReader reader=new StreamReader(fileContainingDatasetnames))
            {
                while (reader.Peek()!=-1)
                {
                    datasetNameList.Add(reader.ReadLine());
                }
                reader.Close();
            }

            return datasetNameList;


        }


        public static void DisplayDatasetPaths()
        {

            var datasetnames = GetDatasetNames();

            StringBuilder sb = new StringBuilder();
            foreach (var dn in datasetnames)
            {
                DatasetUtilities utilities = new DatasetUtilities();
                var datasetPath =   utilities.GetDatasetPath(dn);
                var archievePath = utilities.GetDatasetPathArchived(dn);


                sb.Append(dn);
                sb.Append("\t");
                sb.Append(datasetPath);
                sb.Append("\t");
                sb.Append(archievePath);
                sb.Append("\t");
                if (datasetPath.ToLower().Contains("purged"))
                {
                    sb.Append("TRUE");
                }
                else
                {
                    sb.Append("FALSE");
                }
                sb.Append(Environment.NewLine);

            }

            Console.WriteLine(sb.ToString());


            
        }

        public static int GetFractionNum(string datasetName)
        {
            string regexPattern = @"Yellow_C1[23]_(?<fraction>\d+)";
            var match= Regex.Match(datasetName, regexPattern);

            int fractionNum;
            if (match.Success)
            {
                string fractionNumString = match.Groups["fraction"].Value.TrimStart('0');
                fractionNum = Int32.Parse(fractionNumString);
                return fractionNum;

            }

            return -1;

        }
    }
}
