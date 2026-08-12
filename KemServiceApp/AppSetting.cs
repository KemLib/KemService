namespace KemServiceApp
{
    public class AppSetting
    {
        #region Properties
        public readonly string ExecutablePath;
        public readonly string StartupPath;
        public readonly string LibraryPart;
        public readonly string CustomSetting;
        #endregion

        #region Constructions
        public AppSetting(string executablePath, string startupPath, string libraryPart, string customSetting)
        {
            ExecutablePath = executablePath;
            StartupPath = startupPath;
            LibraryPart = libraryPart;
            CustomSetting = customSetting;
        }
        #endregion
    }
}
