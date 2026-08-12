using KemLibCore;
using KemLibCore.Concurrent.Inter;
using KemLibCore.Concurrent.Locker;
using KemLibCore.DebugLog;

namespace KemServiceApp
{
    public abstract class App : IDisposable
    {
        #region Properties
        private const string ERROR_OBJECT_DISPOSED = "App has been disposed",
            ERROR_IS_INIT = "App has been init",
            ERROR_NOT_INIT = "App not init",
            ERROR_IS_RUNED = "App has been running",
            ERROR_ACCESS_LOCKER_FAIL = "Access locker fail";

        public readonly AppSetting AppSetting;
        private InterBoolStruct interInit,
            isRun,
            isDisposed;
        private readonly TicketLock lockState;

        public bool IsInit => interInit.Value;
        public bool IsRun => isRun.Value;
        public bool IsDisposed => isDisposed.Value;
        #endregion

        #region Construction
        public App(AppSetting appSetting)
        {
            AppSetting = appSetting;
            interInit = new();
            isRun = new();
            isDisposed = new();
            lockState = new();
        }
        ~App()
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
        protected void Dispose(bool disposing)
        {
            if (!isDisposed.TryExchange(true))
                return;
            //
            OnDispose(disposing);
        }
        protected abstract void OnDispose(bool disposing);
        #endregion

        #region Method
        internal async Task<Result> Init(CancellationToken cancellationToken = default)
        {
            if (IsDisposed)
                return Result.Fail(ERROR_OBJECT_DISPOSED);
            if (IsInit)
                return Result.Fail(ERROR_IS_INIT);
            //
            TicketAccept ticket = await lockState.WaitAsync(cancellationToken);
            if (!ticket.IsAccept)
                return Result.Fail(ERROR_ACCESS_LOCKER_FAIL);
            //
            try
            {
                if (IsDisposed)
                    return Result.Fail(ERROR_OBJECT_DISPOSED);
                if (IsInit)
                    return Result.Fail(ERROR_IS_INIT);
                //
                Result result = await OnInit(cancellationToken);
                //
                interInit.Exchange(result.IsSuccess);
                return result;
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
            finally
            {
                ticket.Release();
            }
        }
        internal async Task<Result> Run(ILogHander? logHander = null, CancellationToken cancellationToken = default)
        {
            if (IsDisposed)
                return Result.Fail(ERROR_OBJECT_DISPOSED);
            if (!IsInit)
                return Result.Fail(ERROR_NOT_INIT);
            if (IsRun)
                return Result.Fail(ERROR_IS_RUNED);
            //
            TicketAccept ticket = await lockState.WaitAsync(cancellationToken);
            if (!ticket.IsAccept)
                return Result.Fail(ERROR_ACCESS_LOCKER_FAIL);
            //
            try
            {
                if (IsDisposed)
                    return Result.Fail(ERROR_OBJECT_DISPOSED);
                if (!IsInit)
                    return Result.Fail(ERROR_NOT_INIT);
                if (IsRun)
                    return Result.Fail(ERROR_IS_RUNED);
                //
                isRun.Exchange(true);
                return await OnRun(logHander, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
            finally
            {
                isRun.Exchange(false);
                ticket.Release();
            }
        }
        protected abstract Task<Result> OnInit(CancellationToken cancellationToken = default);
        protected abstract Task<Result> OnRun(ILogHander? logHander = null, CancellationToken cancellationToken = default);
        #endregion
    }
}
