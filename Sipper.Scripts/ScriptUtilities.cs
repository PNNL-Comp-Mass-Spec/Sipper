using System.IO;

namespace Sipper.Scripts
{
    public class ScriptUtilities
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods


        public static void OutputStringToFile(string filename, string stringToWrite)
        {
            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.Write(stringToWrite);
                writer.Close();
            }
            
            
        }


        #endregion

        #region Private Methods

        #endregion

    }
}
