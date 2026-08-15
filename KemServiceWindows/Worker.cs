using KemLibCore.DebugLog;
using KemServiceApp;

namespace KemServiceWindows
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
            switch (log.Type)
            {
                case LogType.Message:
                    if (_logger.IsEnabled(LogLevel.Information))
                        _logger.LogInformation("{message}", log.Title);
                    break;
                case LogType.Warning:
                    if (_logger.IsEnabled(LogLevel.Warning))
                        _logger.LogWarning("{message}", log.Title);
                    break;
                case LogType.Error:
                    if (_logger.IsEnabled(LogLevel.Error))
                        _logger.LogError("{message}", log.Title);
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
