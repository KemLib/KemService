using KemServiceWindows;

IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options =>
    {
        options.ServiceName = "KServiceWindows";
    })
    .ConfigureServices(services =>
    {
        services.AddHostedService<Worker>();
        services.Configure<HostOptions>(hostOptions =>
        {
            hostOptions.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.StopHost;
        });
    })
    .Build();

await host.RunAsync();
