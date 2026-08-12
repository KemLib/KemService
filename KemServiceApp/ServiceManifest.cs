using KemServiceApp.Utilities;
using KemLibCore;
using System.Text.Json;

namespace KemServiceApp
{
    internal class ServiceManifest
    {
        #region Properties
        private const string PATH_SETTING_SERVICE = "ServiceManifest.json";

        public AppManifest[]? Apps
        {
            get;
            init;
        }
        #endregion

        #region Construction
        public ServiceManifest()
        {

        }
        #endregion

        #region Method
        public static async Task<Result<ServiceManifest>> GetInstance(string executablePath, CancellationToken cancellationToken = default)
        {
            try
            {
                string json = await FileIO.ReadTextAsync(executablePath + PATH_SETTING_SERVICE, cancellationToken);
                if (FromJson(json, out ServiceManifest? instance, out string? error))
                    return Result.Success(instance);
                else
                    return Result.Fail<ServiceManifest>(error);
            }
            catch (Exception ex)
            {
                return Result.Fail<ServiceManifest>(ex.Message);
            }
        }
        #endregion

        #region Json
        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }
        public static bool FromJson(string json, [NotNullWhen(true)] out ServiceManifest? manifest, [NotNullWhen(false)] out string? error)
        {
            try
            {
                manifest = JsonSerializer.Deserialize<ServiceManifest>(json);
                if (manifest == null)
                {
                    error = IResult.ERROR_MESSAGE_UNKNOWN;
                    return false;
                }
                error = null;
                return true;
            }
            catch (Exception ex)
            {
                manifest = null;
                error = ex.Message;
                return false;
            }
        }
        #endregion
    }
}
