using KemServiceApp.DebugLog;

namespace KemServiceApp
{
    internal class AppBag
    {
        #region Properties
        public readonly App App;
        public readonly int Index;
        public readonly DebugSetting? Debug;
        #endregion

        #region Construction
        public AppBag(App app, int index, DebugSetting? debug)
        {
            App = app;
            Index = index;
            Debug = debug;
        }
        #endregion

        #region Methods

        #endregion
    }
}
