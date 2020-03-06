using DeconTools.Workflows.Backend.FileIO;
using NUnit.Framework;
using Sipper.Model;

namespace Sipper.UnitTesting.ModelTests
{
    public class HtmlGeneratorTests
    {

        [Test]
        [Ignore("Local file paths")]
        public void generateHtmlReportTest1()
        {

            var fileInputs = new FileInputsInfo {
                ResultImagesFolderPath = @"C:\Data\Temp\Results\Visuals"
            };

            var resultFile =  @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_testing_results.txt";

            var importer = new SipperResultFromTextImporter(resultFile);
                var repo = importer.Import();

            fileInputs.TargetsFilePath =
                @"\\protoapps\UserData\Slysz\Standard_Testing\Targeted_FeatureFinding\SIPPER_standard_testing\Yellow_C13_070_23Mar10_Griffin_10-01-28_testing_results.txt";

            var reportGenerator = new HTMLReportGenerator(repo, fileInputs);
            reportGenerator.GenerateHTMLReport();
        }
    }
}
