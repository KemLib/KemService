using KemLibCore;
using KemLibCore.Concurrent.Inter;
using KemLibCore.Concurrent.Locker;
using KemLibCore.DebugLog;

namespace KemServiceApp
{
    public abstract class App
    {
        #region Properties
        private const int STATE_NONE = 0,
            STATE_INIT = 1,
            STATE_RUN = 2,
            STATE_STOP = 3;
        private const string ERROR_IS_INIT = "App has been init",
            ERROR_NOT_INIT = "App not init",
            ERROR_IS_RUNED = "App has been running",
            ERROR_IS_STOPED = "App has been stoped",
            ERROR_ACCESS_LOCKER_FAIL = "Access locker fail";

        public readonly AppSetting AppSetting;
        private InterIntStruct interState;
        private readonly TicketLock lockState;

        public bool IsInit => interState.Value > STATE_NONE;
        public bool IsRun => interState.Value == STATE_RUN;
        private bool IsStop => interState.Value == STATE_STOP;
        #endregion

        #region Construction
        public App(AppSetting appSetting)
        {
            AppSetting = appSetting;
            interState = new();
            lockState = new();
        }
        #endregion

        #region Method
        internal async Task<Result> Init(ILogHander? logHander = null, CancellationToken cancellationToken = default)
        {
            if (IsInit)
                return Result.Fail(ERROR_IS_INIT);
            //
            TicketAccept ticket = await lockState.WaitAsync(cancellationToken);
            if (!ticket.IsAccept)
                return Result.Fail(ERROR_ACCESS_LOCKER_FAIL);
            //
            try
            {
                if (IsInit)
                    return Result.Fail(ERROR_IS_INIT);
                //
                Result result = await OnInit(logHander, cancellationToken);
                //
                if (result.IsSuccess)
                    interState.Exchange(STATE_INIT);
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
            if (!IsInit)
                return Result.Fail(ERROR_NOT_INIT);
            if (IsRun)
                return Result.Fail(ERROR_IS_RUNED);
            if (IsStop)
                return Result.Fail(ERROR_IS_STOPED);
            //
            TicketAccept ticket = await lockState.WaitAsync(cancellationToken);
            if (!ticket.IsAccept)
                return Result.Fail(ERROR_ACCESS_LOCKER_FAIL);
            //
            try
            {
                if (!IsInit)
                    return Result.Fail(ERROR_NOT_INIT);
                if (IsRun)
                    return Result.Fail(ERROR_IS_RUNED);
                if (IsStop)
                    return Result.Fail(ERROR_IS_STOPED);
                //
                interState.Exchange(STATE_RUN);
                return await OnRun(logHander, cancellationToken);
            }
            catch (Exception ex)
            {
                return Result.Fail(ex.Message);
            }
            finally
            {
                interState.Exchange(STATE_INIT);
                ticket.Release();
            }
        }
        internal async Task<Result> Stop(ILogHander? logHander = null, CancellationToken cancellationToken = default)
        {
            if (!IsInit)
                return Result.Fail(ERROR_NOT_INIT);
            if (IsRun)
                return Result.Fail(ERROR_IS_RUNED);
            if (IsStop)
                return Result.Fail(ERROR_IS_STOPED);
            //
            TicketAccept ticket = await lockState.WaitAsync(cancellationToken);
            if (!ticket.IsAccept)
                return Result.Fail(ERROR_ACCESS_LOCKER_FAIL);
            //
            try
            {
                if (!IsInit)
                    return Result.Fail(ERROR_NOT_INIT);
                if (IsRun)
                    return Result.Fail(ERROR_IS_RUNED);
                if (IsStop)
                    return Result.Fail(ERROR_IS_STOPED);
                //
                interState.Exchange(STATE_STOP);
                return await OnStop(logHander, cancellationToken);
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
        protected abstract Task<Result> OnInit(ILogHander? logHander = null, CancellationToken cancellationToken = default);
        protected abstract Task<Result> OnRun(ILogHander? logHander = null, CancellationToken cancellationToken = default);
        protected abstract Task<Result> OnStop(ILogHander? logHander = null, CancellationToken cancellationToken = default);
        #endregion
    }
}
