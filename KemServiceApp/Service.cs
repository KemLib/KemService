using KemServiceApp.DebugLog;
using KemServiceApp.Utilities;
using KemLibCore;
using KemLibCore.DebugLog;

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
            MESSAGE_SERVICE_CREATE_APPS = "Service create Apps",
            MESSAGE_SERVICE_INIT_APPS = "Service init Apps",
            MESSAGE_SERVICE_INIT_APPS_SUCCESS = "Service init Apps success",
            MESSAGE_SERVICE_INIT_EXCEPTION_UNDEFINED = "Service init Apps exception undefined",
            MESSAGE_SERVICE_RUN_APPS = "Service run Apps",
            MESSAGE_SERVICE_RUN_APPS_SUCCESS = "Service run Apps success",
            MESSAGE_SERVICE_RUN_EXCEPTION_UNDEFINED = "Apps run Apps exception undefined",
            MESSAGE_APP_INIT_SUCCESS = "App[{0}] init success",
            MESSAGE_APP_INIT_FAIL = "App[{0}] init fail: {1}",
            MESSAGE_APP_RUN_SUCCESS = "App[{0}] run success",
            MESSAGE_APP_RUN_FAIL = "App[{0}] run fail: {1}";
        private const string ERROR_SERVICE_MANIFEST_LOAD_FAIL = "Service manifest load fail: {0}",
            ERROR_SERVICE_MANIFEST_LOAD_FAIL_INSTANCE_NULL = "instance is null",
            ERROR_APPS_EMPTY = "App is empty",
            ERROR_GET_APP_TYPE_FAIL = "Get app[{0}] type fail: {1}",
            ERROR_GET_APP_TYPE_FAIL_LIBRARY_NOT_FOUND = "Library not found \"{0}\"",
            ERROR_CREATE_APP_INSTANCE_FAIL = "Create app[{0}] fail: {1}",
            ERROR_CREATE_APP_INSTANCE_FAIL_STARTUP_PATH_NOT_FOUND = "startup path not found \"{0}\"";

        private readonly Action<LogData>? onLog;
        #endregion

        #region Construction
        public Service(Action<LogData>? onLog = null)
        {
            this.onLog = onLog;
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
                string error = string.Format(ERROR_SERVICE_MANIFEST_LOAD_FAIL, result.ErrorMessage);
                OnLog_Error(error);
                return;
            }
            if (result.Value == null)
            {
                string error = string.Format(ERROR_SERVICE_MANIFEST_LOAD_FAIL, ERROR_SERVICE_MANIFEST_LOAD_FAIL_INSTANCE_NULL);
                OnLog_Error(error);
                return;
            }
            OnLog_Message(MESSAGE_SERVICE_LOAD_MANIFEST_SUCCESS);
            ServiceManifest? serviceManifest = result.Value;
            //
            if (TryCreateApp(serviceManifest, executablePath, out List<AppBag>? appBags))
            {
                await AppInit(appBags, cancellationToken);
                await AppRun(appBags, cancellationToken);
                foreach (var bag in appBags)
                    bag.App.Dispose();
            }
            //
            OnLog_Message(MESSAGE_SERVICE_EXECUTE_DONE);
        }
        #endregion

        #region Method
        private bool TryCreateApp(ServiceManifest serviceManifest, string executablePath, [NotNullWhen(true)] out List<AppBag>? appBags)
        {
            OnLog_Message(MESSAGE_SERVICE_CREATE_APPS);
            //
            if (serviceManifest.Apps == null || serviceManifest.Apps.Length == 0)
            {
                OnLog_Error(ERROR_APPS_EMPTY);
                appBags = null;
                return false;
            }
            //
            appBags = [];
            for (int appIndex = 0; appIndex < serviceManifest.Apps.Length; appIndex++)
            {
                AppManifest appManifest = serviceManifest.Apps[appIndex];
                string? error;
                //
                string pathFolder;
                if (string.IsNullOrEmpty(appManifest.StartupPath))
                    pathFolder = executablePath;
                else if (PathUtilities.CheckPathFullyQualified(appManifest.StartupPath))
                    pathFolder = appManifest.StartupPath;
                else
                    pathFolder = PathUtilities.CombinePath(executablePath, appManifest.StartupPath);
                if (!PathUtilities.Directory_Exists(pathFolder, out string startupPath, false))
                {
                    error = string.Format(ERROR_CREATE_APP_INSTANCE_FAIL_STARTUP_PATH_NOT_FOUND, startupPath);
                    string log = string.Format(ERROR_CREATE_APP_INSTANCE_FAIL, appIndex, error);
                    OnLog_Error(log);
                    continue;
                }
                //
                if (!TryGetType(startupPath, appManifest.LibraryPart, appManifest.ClassName, out string libraryPart, out Type? type, out error))
                {
                    string log = string.Format(ERROR_GET_APP_TYPE_FAIL, appIndex, error);
                    OnLog_Error(log);
                    continue;
                }
                //
                AppSetting appSetting = new(executablePath, startupPath, libraryPart, appManifest.CustomSetting);
                if (TryGetApp(type, appSetting, out App? app, out error))
                {
                    AppBag appBag = new(app, appIndex, appManifest.Debug);
                    appBags.Add(appBag);
                }
                else
                {
                    string log = string.Format(ERROR_CREATE_APP_INSTANCE_FAIL, appIndex, error);
                    OnLog_Error(log);
                }
            }
            //
            return appBags.Count > 0;
        }
        private async Task AppInit(List<AppBag> appBags, CancellationToken cancellationToken = default)
        {
            OnLog_Message(MESSAGE_SERVICE_INIT_APPS);
            //
            List<Task<Result>> listTaskIniting = new(appBags.Count);
            List<AppBag> listAppBagIniting = new(appBags.Count);
            foreach (var appBag in appBags)
            {
                listTaskIniting.Add(appBag.App.Init(cancellationToken));
                listAppBagIniting.Add(appBag);
            }
            //
            while (listTaskIniting.Count > 0)
            {
                int index = await WhenAny(listTaskIniting);
                //
                if (index < 0)
                {
                    OnLog_Message(MESSAGE_SERVICE_INIT_EXCEPTION_UNDEFINED);
                    //
                    await Task.WhenAll(listTaskIniting);
                    break;
                }
                else
                {
                    Result result = listTaskIniting[index].Result;
                    AppBag appBag = listAppBagIniting[index];
                    string message;
                    if (result.IsSuccess)
                        message = string.Format(MESSAGE_APP_INIT_SUCCESS, appBag.Index);
                    else
                        message = string.Format(MESSAGE_APP_INIT_FAIL, appBag.Index, result.ErrorMessage);
                    OnLog_Message(message);
                    //
                    listTaskIniting.RemoveAt(index);
                    listAppBagIniting.RemoveAt(index);
                }
            }
            //
            OnLog_Message(MESSAGE_SERVICE_INIT_APPS_SUCCESS);
        }
        private async Task AppRun(List<AppBag> appBags, CancellationToken cancellationToken = default)
        {
            OnLog_Message(MESSAGE_SERVICE_RUN_APPS);
            //
            List<Task<Result>> listTaskRuning = new(appBags.Count);
            List<AppBag> listAppBagRuning = new(appBags.Count);
            List<DebugManager?> listDebugManagerRuning = new(appBags.Count);
            foreach (var appBag in appBags)
            {
                ILogHander logHander;
                if (appBag.Debug == null)
                {
                    logHander = ILogHander.LogHanderNone;
                    listDebugManagerRuning.Add(null);
                }
                else
                {
                    DebugManager debugManager = new(appBag.Debug, appBag.App.AppSetting.StartupPath);
                    await debugManager.StartAsync(cancellationToken);
                    logHander = await debugManager.CreateLogHanderAsync();
                    listDebugManagerRuning.Add(debugManager);
                }
                //
                listTaskRuning.Add(appBag.App.Run(logHander, cancellationToken));
                listAppBagRuning.Add(appBag);
            }
            //
            while (listTaskRuning.Count > 0)
            {
                int index = await WhenAny(listTaskRuning);
                //
                if (index < 0)
                {
                    OnLog_Message(MESSAGE_SERVICE_RUN_EXCEPTION_UNDEFINED);
                    //
                    await Task.WhenAll(listTaskRuning);
                    break;
                }
                else
                {
                    Result result = listTaskRuning[index].Result;
                    AppBag appBag = listAppBagRuning[index];
                    DebugManager? debugManager = listDebugManagerRuning[index];
                    string message;
                    if (result.IsSuccess)
                        message = string.Format(MESSAGE_APP_RUN_SUCCESS, appBag.Index);
                    else
                        message = string.Format(MESSAGE_APP_RUN_FAIL, appBag.Index, result.ErrorMessage);
                    OnLog_Message(message);
                    //
                    if (debugManager != null)
                    {
                        await debugManager.StopAsync(cancellationToken);
                        debugManager?.Dispose();
                    }
                    //
                    listTaskRuning.RemoveAt(index);
                    listAppBagRuning.RemoveAt(index);
                    listDebugManagerRuning.RemoveAt(index);
                }
            }
            //
            OnLog_Message(MESSAGE_SERVICE_RUN_APPS_SUCCESS);
        }
        private static async Task<int> WhenAny<T>(List<Task<T>> listTask)
        {
            Task<T> task = await Task.WhenAny(listTask);
            //
            int index = 0;
            while (index < listTask.Count)
            {
                if (listTask[index] == task)
                    return index;
                index++;
            }
            return -1;
        }
        public static bool TryGetType(string startupPath, string libraryPart, string className, out string fileLibraryPart, [NotNullWhen(true)] out Type? type, [NotNullWhen(false)] out string? error)
        {
            string part;
            if (string.IsNullOrEmpty(libraryPart))
                part = string.Empty;
            else if (PathUtilities.CheckPathFullyQualified(libraryPart))
                part = libraryPart;
            else
                part = PathUtilities.CombinePath(startupPath, libraryPart);
            //
            if (!PathUtilities.File_Exists(part, out fileLibraryPart, false))
            {
                type = null;
                error = string.Format(ERROR_GET_APP_TYPE_FAIL_LIBRARY_NOT_FOUND, fileLibraryPart);
                return false;
            }
            //
            if (!TypeUtilities.TryGetType(fileLibraryPart, className, out type, out error))
            {
                return false;
            }
            if (!TypeUtilities.CheckInherit(type, typeof(App), out error))
            {
                type = null;
                return false;
            }
            //
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
    }
}
