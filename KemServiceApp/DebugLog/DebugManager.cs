using KemServiceApp.Utilities;
using KemLibCore.Concurrent.Inter;
using KemLibCore.Concurrent.Locker;
using KemLibCore.DebugLog;
using System.Text;

namespace KemServiceApp.DebugLog
{
    public class DebugManager : IDisposable
    {
        #region Properties
        private const int DELAY_TIME = 10;

        public readonly DebugSetting? Setting;
        public readonly string? StartupPath;
        private readonly LogQueue logQueue;
        private InterBoolStruct interRun;
        private InterBoolStruct isDisposed;
        private readonly TicketLock lockRun;
        private Task? taskRun;
        private InterObjectStruct<CancellationTokenSource> interCancellationTokenSource;

        public bool IsRun => interRun.Value;
        public bool IsDisposed => isDisposed.Value;
        #endregion

        #region Construction
        public DebugManager(DebugSetting? setting = null, string? startupPath = null)
        {
            Setting = setting;
            StartupPath = startupPath;
            logQueue = new();
            interRun = new();
            isDisposed = new();
            taskRun = null;
            lockRun = new();
            interCancellationTokenSource = new();
        }
        ~DebugManager()
        {
            Dispose(false);
        }
        #endregion

        #region Dispose
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        private void Dispose(bool disposing)
        {
            if (!isDisposed.TryExchange(true))
                return;
            //
            Stop();
            if (disposing)
            {

            }
        }
        #endregion

        #region Method
        public void Start()
        {
            if (IsDisposed)
                return;
            //
            Ticket ticket = lockRun.Wait();
            try
            {
                if (interRun.Value)
                    return;
                //
                interCancellationTokenSource.Exchange(new());
                taskRun = Task.Run(RunAsync);
            }
            catch (Exception)
            {

            }
            finally
            {
                ticket.Release();
            }
        }
        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            if (IsDisposed)
                return;
            //
            TicketAccept ticket = await lockRun.WaitAsync(cancellationToken);
            if (!ticket.IsAccept)
                return;
            //
            try
            {
                if (interRun.Value)
                    return;
                //
                interCancellationTokenSource.Exchange(new());
                taskRun = Task.Run(RunAsync, CancellationToken.None);
                //
                interRun.Value = true;
            }
            catch (Exception)
            {

            }
            finally
            {
                ticket.Release();
            }
        }
        public void Stop()
        {
            Ticket ticket = lockRun.Wait();
            //
            CancellationTokenSource? cancellationTokenSource = interCancellationTokenSource.Exchange(null);
            if (cancellationTokenSource != null)
            {
                try
                {
                    cancellationTokenSource.Cancel();
                }
                catch (Exception)
                {

                }
                finally
                {
                    cancellationTokenSource.Dispose();
                }
            }
            //
            if (taskRun != null)
            {
                try
                {
                    taskRun.Wait();
                }
                catch (Exception)
                {

                }
            }
            //
            ticket.Release();
        }
        public async Task StopAsync(CancellationToken cancellationToken = default)
        {
            TicketAccept ticket = await lockRun.WaitAsync(cancellationToken);
            if (!ticket.IsAccept)
                return;
            //
            CancellationTokenSource? cancellationTokenSource = interCancellationTokenSource.Exchange(null);
            if (cancellationTokenSource != null)
            {
                try
                {
                    await cancellationTokenSource.CancelAsync();
                }
                catch (Exception)
                {

                }
                finally
                {
                    cancellationTokenSource.Dispose();
                }
            }
            //
            if (taskRun != null)
            {
                try
                {
                    await taskRun;
                }
                catch (Exception)
                {

                }
            }
            //
            ticket.Release();
        }
        #endregion

        #region Log
        public ILogHander CreateLogHander()
        {
            return logQueue.CreateHander();
        }
        public async Task<ILogHander> CreateLogHanderAsync()
        {
            return await logQueue.CreateHanderAsync();
        }
        #endregion

        #region Run
        private async Task RunAsync()
        {
            interRun.Value = true;
            List<LogData> saveLogs = [];
            DateTime saveTime = DateTime.Now;
            string saveFolder;
            if (Setting == null || string.IsNullOrEmpty(Setting.SaveFolder))
                saveFolder = string.Empty;
            else if (PathUtilities.CheckPathFullyQualified(Setting.SaveFolder))
                saveFolder = Setting.SaveFolder;
            else
                saveFolder = PathUtilities.CombinePath(StartupPath, Setting.SaveFolder);
            //
            CancellationTokenSource? cancellationTokenSource = interCancellationTokenSource.Value;
            CancellationToken cancellationToken = cancellationTokenSource == null ? CancellationToken.None : cancellationTokenSource.Token;
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await Task.Delay(DELAY_TIME, cancellationToken);
                }
                catch (Exception)
                {
                    break;
                }
                //
                if (logQueue.Count > 0)
                {
                    LogData[] logs = await logQueue.DequeueAllAsync();
                    saveLogs.AddRange(logs);
                }
                //
                if (Setting != null && !string.IsNullOrEmpty(saveFolder))
                {
                    DateTime currentTime = DateTime.Now;
                    if (saveLogs.Count > 0 && (saveLogs.Count >= Setting.SaveNumber || (currentTime - saveTime).TotalSeconds >= Setting.SaveTime))
                    {
                        saveTime = currentTime;
                        await Log_Save(Setting, currentTime, saveFolder, saveLogs);
                        saveLogs.Clear();
                    }
                }
            }
            //
            if (logQueue.TryClear(out LogData[]? tmpLogs))
                saveLogs.AddRange(tmpLogs);
            if (Setting != null && !string.IsNullOrEmpty(saveFolder))
                await Log_Save(Setting, DateTime.Now, saveFolder, saveLogs);
            //
            interRun.Value = false;
        }
        private static async Task Log_Save(DebugSetting setting, DateTime currentTime, string saveFolder, List<LogData> logs)
        {
            if (!setting.TryGet_FilePath(currentTime, saveFolder, out string? filePath))
                return;
            //
            int stringCapacity = logs.Count * 128;
            StringBuilder stringBuilder = new(stringCapacity);
            foreach (var log in logs)
                stringBuilder.AppendLine(log.ToString());
            //
            await FileIO.WriteTextAsync(filePath, stringBuilder.ToString());
        }
        #endregion
    }
}
