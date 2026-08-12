using KemServiceApp.DebugLog;
using System.Text.Json;

namespace KemServiceApp
{
    public class AppManifest
    {
        #region Properties
        public string StartupPath
        {
            get;
            init;
        }
        public string LibraryPart
        {
            get;
            init;
        }
        public string ClassName
        {
            get;
            init;
        }
        public DebugSetting? Debug
        {
            get;
            init;
        }
        public string CustomSetting
        {
            get;
            init;
        }
        #endregion

        #region Construction
        public AppManifest()
        {
            LibraryPart = string.Empty;
            ClassName = string.Empty;
            StartupPath = string.Empty;
            Debug = null;
            CustomSetting = string.Empty;
        }
        #endregion

        #region Json
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
        public static bool FromJson(string json, [NotNullWhen(true)] out AppManifest? appManifest)
        {
            try
            {
                appManifest = JsonSerializer.Deserialize<AppManifest>(json);
                if (appManifest == null)
                    return false;
                return true;
            }
            catch (Exception)
            {
                appManifest = null;
                return false;
            }
        }
        #endregion
    }
}
