using KemServiceApp;
using KemLibCore.DebugLog;

namespace KemServiceLinux
{
    public class Worker : BackgroundService
    {
        #region Properties
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly ILogger<Worker> _logger;
        private readonly Service service;
        #endregion

        #region Construction
        public Worker(IHostApplicationLifetime hostApplicationLifetime, ILogger<Worker> logger)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _logger = logger;
            service = new Service(OnLog);
        }
        #endregion

        #region Method
        private void OnLog(LogData log)
        {
            string messsage = log.ToString();
            switch (log.Type)
            {
                case LogType.Message:
                    _logger.LogInformation("{message}", messsage);
                    break;
                case LogType.Warning:
                    _logger.LogWarning("{message}", messsage);
                    break;
                case LogType.Error:
                    _logger.LogError("{message}", messsage);
                    break;
            }
        }
        #endregion

        #region Worker
        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await service.StartAsync(cancellationToken);
            await base.StartAsync(cancellationToken);
        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await service.StopAsync(cancellationToken);
            await base.StopAsync(cancellationToken);
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            string executablePath = AppContext.BaseDirectory;
            await service.ExecuteAsync(executablePath, cancellationToken);

            // When completed, the entire app host will stop.
            _hostApplicationLifetime.StopApplication();
        }
        #endregion
    }
}
