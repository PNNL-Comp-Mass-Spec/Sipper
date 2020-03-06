using System.Windows.Input;

namespace Sipper.Model
{
    public class CommandBase
    {

        #region Constructors
        #endregion

        #region Properties

        #endregion

        #region Public Methods

        public static RoutedCommand SetAnnotationToYes = new RoutedCommand();

        public static RoutedCommand SetAnnotationToNo = new RoutedCommand();

        public static RoutedCommand SetAnnotationToMaybe = new RoutedCommand();

        #endregion

        #region Private Methods

        #endregion
    }
}
