# KemService

KemService is a .NET 10 hosted service framework for launching and managing reusable application modules from a manifest-driven configuration. It provides a common lifecycle abstraction for background apps and exposes native Windows and Linux hosting wrappers so the same app logic can run as a Windows Service or a systemd service.

The project is built around a small runtime in `KemServiceApp` that loads a .NET assembly, creates an app instance, and calls its lifecycle methods: `Init`, `Run`, and `Stop`. This makes it a practical foundation for long-running background services, scheduled workers, or daemon-like modules that must be restarted and monitored cleanly.

## Overview

The repository contains three main parts:

- `KemServiceApp`: reusable service runtime, manifest models, logging helpers, and the abstract `App` base class.
- `KemServiceWindows`: Windows Service host using `Microsoft.Extensions.Hosting.WindowsServices`.
- `KemServiceLinux`: Linux host using `Microsoft.Extensions.Hosting.Systemd`.
- `TestAppNone`: sample app implementation used as a reference for how to plug custom modules into the framework.

## Features

- Manifest-based service configuration via `ServiceManifest.json`
- Dynamic loading of app libraries and classes
- Common lifecycle contract for every app: `OnInit`, `OnRun`, `OnStop`
- Concurrent execution of multiple configured apps
- Restart support with configurable `ResetTime`
- Optional debug log management and log file rotation
- Cross-platform hosting for Windows Services and Linux systemd

## Project structure

```text
KemService/
├── KemServiceApp/              # Core runtime and abstraction layer
│   ├── App.cs                  # Base class for service applications
│   ├── AppSetting.cs           # Runtime context passed to an app
│   ├── AppManifest.cs          # Manifest entry model
│   ├── Service.cs              # Load + create + run apps
│   ├── ServiceManifest.cs       # Service-level manifest loader
│   └── Utilities/              # Helper methods for file and path handling
├── KemServiceWindows/          # Windows-hosted worker service
│   ├── Program.cs              # Host bootstrap
│   ├── Worker.cs               # Background service wrapper around AppService
│   ├── ServiceManifest.json    # Windows sample config
│   └── *.bat                   # Service install/start/stop scripts
├── KemServiceLinux/            # Linux-hosted worker service
│   ├── Program.cs              # Host bootstrap
│   ├── MyService.service       # systemd definition example
│   └── ServiceManifest.json    # Linux sample config
├── TestAppNone/                # Example app implementation
├── KemService.slnx             # .NET solution definition
├── LICENSE                     # MIT license
└── README.md                  # Project documentation
```

## How it works

At runtime, the host loads `ServiceManifest.json` from the executable folder. The manifest lists one or more app entries, each with a startup path, library file, class name, reset policy, and optional debug settings.

The flow is:

1. Read `ServiceManifest.json`
2. Resolve the application library and class type
3. Create an instance of the app with `AppSetting`
4. Call `Init()`
5. Call `Run()`
6. Call `Stop()`
7. Optionally restart the app according to `ResetTime`

This makes the service suitable for continuously running long-lived modules while keeping the logic inside each app isolated and testable.

## App lifecycle

Every custom app inherits from `App` and implements three protected methods:

```csharp
public abstract class App
{
    protected abstract Task<Result> OnInit(ILogHander? logHander = null, CancellationToken cancellationToken = default);
    protected abstract Task<Result> OnRun(ILogHander? logHander = null, CancellationToken cancellationToken = default);
    protected abstract Task<Result> OnStop(ILogHander? logHander = null, CancellationToken cancellationToken = default);
}
```

A minimal implementation looks like this:

```csharp
public class MyApp : App
{
    public MyApp(AppSetting appSetting) : base(appSetting) { }

    protected override async Task<Result> OnInit(ILogHander? logHander = null, CancellationToken cancellationToken = default)
    {
        return Result.Success();
    }

    protected override async Task<Result> OnRun(ILogHander? logHander = null, CancellationToken cancellationToken = default)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            // application work
            await Task.Delay(1000, cancellationToken);
        }
        return Result.Success();
    }

    protected override async Task<Result> OnStop(ILogHander? logHander = null, CancellationToken cancellationToken = default)
    {
        return Result.Success();
    }
}
```

The sample project `TestAppNone/AppNone.cs` demonstrates this pattern in a realistic use case.

## Service manifest configuration

Each service instance expects a `ServiceManifest.json` file with the following structure:

```json
{
  "Apps": [
    {
      "StartupPath": "../../../../TestAppNone/bin/Debug/net10.0",
      "LibraryPart": "TestAppNone.dll",
      "ClassName": "TestAppNone.AppNone",
      "ResetTime": -1,
      "Debug": {
        "SaveNumber": 10000,
        "SaveTime": 43200,
        "SaveFolder": "Logs",
        "SaveName": "AppServer"
      },
      "CustomSetting": ""
    }
  ]
}
```

### Fields

- `StartupPath`: Base path used when the app starts. This is where the app binary or working directory is located.
- `LibraryPart`: DLL file name or library path that contains the app type.
- `ClassName`: Full type name of the app class, including namespace, for example `TestAppNone.AppNone`.
- `ResetTime`: Restart interval in seconds. `-1` means no automatic reset; a positive value triggers a loop delay between runs.
- `Debug`: Optional logging/debug configuration for file output.
- `CustomSetting`: Arbitrary application-specific string passed through `AppSetting`.

## Running the project

### Prerequisites

- .NET 10 SDK
- Windows or Linux environment depending on the host project

### Restore and build

```bash
dotnet restore
dotnet build KemService.slnx
```

### Run the Windows host

```bash
dotnet run --project KemServiceWindows
```

The Windows project uses `UseWindowsService` and starts as a background worker with a Windows Service name of `KemService Windows`.

### Run the Linux host

```bash
dotnet run --project KemServiceLinux
```

The Linux host registers a `Systemd` lifetime and is designed to work with `MyService.service`.

## Windows service installation

The project provides helper scripts in `KemServiceWindows/`:

```bat
service_create.bat
service_config.bat
service_start.bat
service_stop.bat
service_delete.bat
```

These scripts use `sc.exe` to create, configure, start, and delete the Windows service. Example:

```bat
cd KemServiceWindows
service_create.bat
service_config.bat
service_start.bat
```

## Linux systemd installation

The file `KemServiceLinux/MyService.service` is a template for systemd. Update the `ExecStart` and `WorkingDirectory` paths to the published output directory, then run:

```bash
sudo systemctl daemon-reload
sudo systemctl enable MyService.service
sudo systemctl start MyService.service
sudo systemctl status MyService.service
```

## Example: launching the sample app

The repository includes a simple example: `TestAppNone/AppNone.cs`. It logs application details such as executable path, startup path, library name, and custom setting, then sleeps for a random interval before returning success.

This sample demonstrates the intended pattern for app authors:

- implement a class derived from `App`
- provide constructor accepting `AppSetting`
- implement `OnInit`, `OnRun`, and `OnStop`
- register the class in `ServiceManifest.json`

## License

This project is licensed under the MIT License. See [LICENSE](./LICENSE) for details.

## Notes

KemService is intentionally small and flexible. It is a good starting point for building service-style applications that need dynamic modules, manifest-driven configuration, and platform-specific hosting without coupling runtime behavior to a single operating system.
