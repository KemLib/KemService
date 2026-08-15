using KemServiceApp.Utilities;
using System.Text.Json;

namespace KemServiceApp.DebugLog
{
    public class DebugSetting
    {
        #region Properties
        private const string FILE_NAME_FORMAT = "{0}.log",
            FILE_NAME_ID_FORMAT = "{0}_{1}.log";
        private const string TIME_CULTURE_INFO = "dd_MM_yyyy_HH_mm_ss_ffff";

        public const int SAVE_NUMBER_MIN = 100,
            SAVE_TIME_MIN = 60;
        public const int DEFAULT_SAVE_NUMBER = 1000,
            DEFAULT_SAVE_TIME = 3600;

        public int SaveNumber
        {
            get;
            init;
        }
        public int SaveTime
        {
            get;
            init;
        }
        public string SaveFolder
        {
            get;
            init;
        }
        public string SaveName
        {
            get;
            init;
        }
        #endregion

        #region Construction
        public DebugSetting()
        {
            SaveNumber = DEFAULT_SAVE_NUMBER;
            SaveTime = DEFAULT_SAVE_TIME;
            SaveFolder = string.Empty;
            SaveName = string.Empty;
        }
        public DebugSetting(int saveNumber, int saveTime, string? saveFolder = null, string? saveName = null)
        {
            SaveNumber = Math.Max(SAVE_NUMBER_MIN, saveNumber);
            SaveTime = Math.Max(SAVE_TIME_MIN, saveTime);
            SaveFolder = string.IsNullOrEmpty(saveFolder) ? string.Empty : saveFolder;
            SaveName = string.IsNullOrEmpty(saveName) ? string.Empty : saveName;
        }
        #endregion

        #region Method
        public bool TryGet_FilePath(DateTime currentTime, string saveFolder, [NotNullWhen(true)] out string? path)
        {
            if (!PathUtilities.Directory_Exists(saveFolder, out string directory, true))
            {
                path = null;
                return false;
            }
            path = PathUtilities.CombinePath(directory, Get_FileName(currentTime));
            return true;
        }
        private string Get_FileName(DateTime currentTime)
        {
            if (string.IsNullOrEmpty(SaveName))
                return string.Format(FILE_NAME_FORMAT, currentTime.ToString(TIME_CULTURE_INFO));
            return string.Format(FILE_NAME_ID_FORMAT, SaveName, currentTime.ToString(TIME_CULTURE_INFO));
        }
        #endregion

        #region Json
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
        public static bool FromJson(string json, [NotNullWhen(true)] out DebugSetting? debugSetting)
        {
            try
            {
                debugSetting = JsonSerializer.Deserialize<DebugSetting>(json);
                if (debugSetting == null)
                    return false;
                return true;
            }
            catch (Exception)
            {
                debugSetting = null;
                return false;
            }
        }
        #endregion
    }
}
