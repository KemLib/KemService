using KemLibCore;
using KemLibCore.DebugLog;
using KemServiceApp.DebugLog;
using KemServiceApp.Utilities;

namespace KemServiceApp
{
    public class Service
    {
        #region Properties
        private const string MESSAGE_SERVICE_START = "Service start",
            MESSAGE_SERVICE_START_DONE = "Service start done",
            MESSAGE_SERVICE_STOP = "Service stop",
            MESSAGE_SERVICE_STOP_DONE = "Service stop done",
            MESSAGE_SERVICE_EXECUTE = "Service execute",
            MESSAGE_SERVICE_EXECUTE_DONE = "Service execute done",
            MESSAGE_SERVICE_LOAD_MANIFEST = "Service load manifest",
            MESSAGE_SERVICE_LOAD_MANIFEST_SUCCESS = "Service manifest load success",
            MESSAGE_SERVICE_LOAD_MANIFEST_FAIL = "Service manifest load fail: {0}",
            MESSAGE_SERVICE_LOAD_MANIFEST_FAIL_INSTANCE_NULL = "instance is null",
            MESSAGE_SERVICE_APP_RUN = "Service app run",
            MESSAGE_SERVICE_APP_RUN_COMPLETE = "Service app run complete";
        private const string SOURCE = "Service",
            TITLE_APP_CREATE = "App create",
            TITLE_APP_CREATE_SUCCESS = "App create success",
            TITLE_APP_CREATE_FAIL = "App create fail",
            TITLE_APP_INIT = "App init",
            TITLE_APP_INIT_SUCCESS = "App init success",
            TITLE_APP_INIT_FAIL = "App init fail",
            TITLE_APP_RUN = "App run",
            TITLE_APP_RUN_SUCCESS = "App run success",
            TITLE_APP_RUN_FAIL = "App run fail",
            TITLE_APP_STOP = "App stop",
            TITLE_APP_STOP_SUCCESS = "App stop success",
            TITLE_APP_STOP_FAIL = "App stop fail",
            MESSAGE_LIBRARY_NOT_FOUND = "Library not found \"{0}\"",
            MESSAGE_GET_APP_TYPE_FAIL = "Get app type fail \"{0}\"",
            MESSAGE_CREATE_APP_INSTANCE_FAIL = "Create app instance fail \"{0}\"";

        private readonly Action<LogData>? onLog;
        #endregion

        #region Construction
        public Service(Action<LogData>? onLog = null)
        {
            this.onLog = onLog;
        }
        #endregion

        #region Log
        protected void OnLog_Message(string log)
        {
            if (onLog == null)
                return;
            //
            try
            {
                onLog(new LogData(LogType.Message, title: log));
            }
            catch (Exception)
            {

            }
        }
        protected void OnLog_Warning(string log)
        {
            if (onLog == null)
                return;
            //
            try
            {
                onLog(new LogData(LogType.Warning, title: log));
            }
            catch (Exception)
            {

            }
        }
        protected void OnLog_Error(string log)
        {
            if (onLog == null)
                return;
            //
            try
            {
                onLog(new LogData(LogType.Error, title: log));
            }
            catch (Exception)
            {

            }
        }
        #endregion

        #region Service
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            OnLog_Message(MESSAGE_SERVICE_START);
            try
            {
                await Task.CompletedTask.WaitAsync(cancellationToken);
            }
            catch (Exception)
            {

            }
            OnLog_Message(MESSAGE_SERVICE_START_DONE);
        }
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            OnLog_Message(MESSAGE_SERVICE_STOP);
            //
            try
            {
                await Task.CompletedTask.WaitAsync(cancellationToken);
            }
            catch (Exception)
            {

            }
            //
            OnLog_Message(MESSAGE_SERVICE_STOP_DONE);
        }
        public async Task ExecuteAsync(string executablePath, CancellationToken cancellationToken = default)
        {
            OnLog_Message(MESSAGE_SERVICE_EXECUTE);
            //
            OnLog_Message(MESSAGE_SERVICE_LOAD_MANIFEST);
            Result<ServiceManifest> result = await ServiceManifest.GetInstance(executablePath, cancellationToken);
            if (!result.IsSuccess)
            {
                string error = string.Format(MESSAGE_SERVICE_LOAD_MANIFEST_FAIL, result.ErrorMessage);
                OnLog_Error(error);
                return;
            }
            if (result.Value == null)
            {
                string error = string.Format(MESSAGE_SERVICE_LOAD_MANIFEST_FAIL, MESSAGE_SERVICE_LOAD_MANIFEST_FAIL_INSTANCE_NULL);
                OnLog_Error(error);
                return;
            }
            OnLog_Message(MESSAGE_SERVICE_LOAD_MANIFEST_SUCCESS);
            //
            OnLog_Message(MESSAGE_SERVICE_APP_RUN);
            ServiceManifest? serviceManifest = result.Value;
            await Runing(serviceManifest, executablePath, cancellationToken);
            OnLog_Message(MESSAGE_SERVICE_APP_RUN_COMPLETE);
            //
            OnLog_Message(MESSAGE_SERVICE_EXECUTE_DONE);
        }
        #endregion

        #region Method
        private static async Task Runing(ServiceManifest serviceManifest, string executablePath, CancellationToken cancellationToken)
        {
            int count = serviceManifest.Apps.Length;
            Task[] tasks = new Task[count];
            for (int i = 0; i < count; i++)
            {
                tasks[i] = Runing(serviceManifest.Apps[i], executablePath, cancellationToken);
            }
            await Task.WhenAll(tasks);
        }
        private static async Task Runing(AppManifest appManifest, string executablePath, CancellationToken cancellationToken)
        {
            string startupPath;
            if (string.IsNullOrEmpty(appManifest.StartupPath))
                startupPath = executablePath;
            else if (PathUtilities.CheckPathFullyQualified(appManifest.StartupPath))
                startupPath = appManifest.StartupPath;
            else
                startupPath = PathUtilities.CombinePath(executablePath, appManifest.StartupPath);
            //
            while (true)
            {
                ILogHander logHander;
                DebugManager? debugManager;
                if (appManifest.Debug == null)
                {
                    debugManager = null;
                    logHander = ILogHander.LogHanderNone;
                }
                else
                {
                    debugManager = new(appManifest.Debug, startupPath);
                    await debugManager.StartAsync(cancellationToken);
                    logHander = await debugManager.CreateLogHanderAsync();
                }
                //
                await logHander.LogMessageAsync(SOURCE, TITLE_APP_CREATE);
                if (TryCreateApp(logHander, appManifest, executablePath, startupPath, out App? app))
                {
                    await logHander.LogMessageAsync(SOURCE, TITLE_APP_CREATE_SUCCESS);
                    //
                    await Runing(logHander, app, cancellationToken);
                }
                else
                {
                    await logHander.LogWarningAsync(SOURCE, TITLE_APP_CREATE_FAIL);
                }
                //
                if (debugManager != null)
                {
                    await debugManager.StopAsync(CancellationToken.None);
                    debugManager.Dispose();
                }
                //
                if (appManifest.ResetTime < 0 || cancellationToken.IsCancellationRequested)
                    break;
                if (appManifest.ResetTime > 0)
                {
                    try
                    {
                        await Task.Delay(appManifest.ResetTime * 1000, cancellationToken);
                    }
                    catch (Exception)
                    {
                        break;
                    }
                }
            }
        }
        private static async Task Runing(ILogHander logHander, App app, CancellationToken cancellationToken)
        {
            await logHander.LogMessageAsync(SOURCE, TITLE_APP_INIT);
            Result result_init = await app.Init(logHander, cancellationToken);
            if (result_init.IsSuccess)
            {
                await logHander.LogMessageAsync(SOURCE, TITLE_APP_INIT_SUCCESS);
            }
            else
            {
                await logHander.LogWarningAsync(SOURCE, TITLE_APP_INIT, result_init.ErrorMessage);
                await logHander.LogWarningAsync(SOURCE, TITLE_APP_INIT_FAIL);
                //
                return;
            }
            //
            await logHander.LogMessageAsync(SOURCE, TITLE_APP_RUN);
            Result result_run = await app.Run(logHander, cancellationToken);
            if (result_run.IsSuccess)
            {
                await logHander.LogMessageAsync(SOURCE, TITLE_APP_RUN_SUCCESS);
            }
            else
            {
                await logHander.LogWarningAsync(SOURCE, TITLE_APP_RUN, result_run.ErrorMessage);
                await logHander.LogWarningAsync(SOURCE, TITLE_APP_RUN_FAIL);
            }
            //
            await logHander.LogMessageAsync(SOURCE, TITLE_APP_STOP);
            Result result_stop = await app.Stop(logHander, cancellationToken);
            if (result_stop.IsSuccess)
            {
                await logHander.LogMessageAsync(SOURCE, TITLE_APP_STOP_SUCCESS);
            }
            else
            {
                await logHander.LogWarningAsync(SOURCE, TITLE_APP_STOP, result_stop.ErrorMessage);
                await logHander.LogWarningAsync(SOURCE, TITLE_APP_STOP_FAIL);
            }
        }
        public static bool TryCreateApp(ILogHander logHander, AppManifest appManifest, string executablePath, string startupPath, [NotNullWhen(true)] out App? app)
        {
            string libraryPart;
            if (string.IsNullOrEmpty(appManifest.LibraryPart))
                libraryPart = string.Empty;
            else if (PathUtilities.CheckPathFullyQualified(appManifest.LibraryPart))
                libraryPart = appManifest.LibraryPart;
            else
                libraryPart = PathUtilities.CombinePath(startupPath, appManifest.LibraryPart);
            //
            if (!PathUtilities.File_Exists(libraryPart, out string fileLibraryPart, false))
            {
                string message = string.Format(MESSAGE_LIBRARY_NOT_FOUND, fileLibraryPart);
                logHander.LogErrorAsync(SOURCE, TITLE_APP_CREATE, message);
                //
                app = null;
                return false;
            }
            if (!TryGetType(fileLibraryPart, appManifest.ClassName, out Type? type, out string? error))
            {
                string message = string.Format(MESSAGE_GET_APP_TYPE_FAIL, error);
                logHander.LogErrorAsync(SOURCE, TITLE_APP_CREATE, message);
                //
                app = null;
                return false;
            }
            //
            AppSetting appSetting = new(executablePath, startupPath, libraryPart, appManifest.CustomSetting);
            if (TryGetApp(type, appSetting, out app, out error))
            {
                return true;
            }
            else
            {
                string message = string.Format(MESSAGE_CREATE_APP_INSTANCE_FAIL, error);
                logHander.LogErrorAsync(SOURCE, TITLE_APP_CREATE, message);
                //
                return false;
            }
        }
        public static bool TryGetType(string libraryPart, string className, [NotNullWhen(true)] out Type? type, [NotNullWhen(false)] out string? error)
        {
            if (!TypeUtilities.TryGetType(libraryPart, className, out type, out error))
            {
                type = null;
                return false;
            }
            if (!TypeUtilities.CheckInherit(type, typeof(App), out error))
            {
                type = null;
                return false;
            }
            return true;
        }
        public static bool TryGetApp(Type type, AppSetting appSetting, [NotNullWhen(true)] out App? app, [NotNullWhen(false)] out string? error)
        {
            object[] parameters = [appSetting];
            if (!TypeConstructor<App>.TryCreateConstructor(type, parameters, out TypeConstructor<App>? constructor, out error))
            {
                app = null;
                return false;
            }
            if (!constructor.TryCreateInstance(out app, out error))
            {
                return false;
            }
            return true;
        }
        #endregion
    }
}
