using KemServiceApp;
using KemLibCore;
using KemLibCore.DebugLog;

namespace TestAppNone
{
    public class AppNone : App
    {
        #region Properties
        private const string LOG_SOURCE = "AppNone",
            LOG_TITLE_DELAY = "Delay time",
            LOG_EXECUTABLE_PATH = "ExecutablePath",
            LOG_STARTUP_PATH = "StartupPath",
            LOG_LIBRARY_PART = "LibraryPart",
            LOG_CUSTOM_SETTING = "CustomSetting";
        #endregion

        #region Construction
        public AppNone(AppSetting appSetting) : base(appSetting)
        {

        }
        #endregion

        #region Methods
        protected override async Task<Result> OnInit(ILogHander? logHander = null, CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(100, cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }
        protected override async Task<Result> OnStop(ILogHander? logHander = null, CancellationToken cancellationToken = default)
        {
            try
            {
                await Task.Delay(100, cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }
        protected override async Task<Result> OnRun(ILogHander? logHander = null, CancellationToken cancellationToken = default)
        {
            logHander?.LogMessage(LOG_SOURCE, LOG_EXECUTABLE_PATH, AppSetting.ExecutablePath);
            logHander?.LogMessage(LOG_SOURCE, LOG_STARTUP_PATH, AppSetting.StartupPath);
            logHander?.LogMessage(LOG_SOURCE, LOG_LIBRARY_PART, AppSetting.LibraryPart);
            logHander?.LogMessage(LOG_SOURCE, LOG_CUSTOM_SETTING, AppSetting.CustomSetting);
            //
            Random rnd = new();
            int delay = rnd.Next(2000, 5000);
            logHander?.LogMessage(LOG_SOURCE, LOG_TITLE_DELAY, delay.ToString());
            //
            try
            {
                await Task.Delay(delay, cancellationToken);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
        }
        #endregion

    }
}
