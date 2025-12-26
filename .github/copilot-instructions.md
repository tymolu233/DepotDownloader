# DepotDownloader AI Coding Instructions

## Project Overview

DepotDownloader is a CLI tool for downloading Steam depot content, workshop items, and manifests via the Steam network using SteamKit2. Built on .NET 9.0 as a cross-platform console application.

## Architecture

### Core Components
- **[Program.cs](DepotDownloader/Program.cs)**: Entry point handling CLI argument parsing with custom `GetParameter`/`HasParameter` helpers that mark consumed args
- **[ContentDownloader.cs](DepotDownloader/ContentDownloader.cs)**: Main download orchestration - manages depot downloads, chunk verification, file filtering, and concurrent downloads
- **[Steam3Session.cs](DepotDownloader/Steam3Session.cs)**: SteamKit2 session manager - handles authentication (username/password/QR), callbacks, reconnection logic, and license checks
- **[CDNClientPool.cs](DepotDownloader/CDNClientPool.cs)**: CDN server pool with weighted load balancing and penalty tracking via `AccountSettingsStore`
- **[DownloadConfig.cs](DepotDownloader/DownloadConfig.cs)**: Global config singleton (`ContentDownloader.Config`) storing parsed CLI options

### Key Patterns
- **Singleton Configuration**: `ContentDownloader.Config` is accessed throughout as a static property - avoid passing config objects
- **Argument Parsing**: Custom case-insensitive helpers with `consumedArgs[]` tracking - never use standard libraries
- **Authentication Flow**: Uses SteamKit2's `AuthSession` with `IAuthenticator` interface for 2FA/QR codes (see [ConsoleAuthenticator.cs](DepotDownloader/ConsoleAuthenticator.cs))
- **Persistent Settings**: `AccountSettingsStore` uses IsolatedStorage for login tokens, guard data, and CDN server penalties (protobuf-net serialization)
- **File Filtering**: Supports HashSet matching AND regex via `-filelist` with `regex:` prefix pattern

## Development Workflows

### Build & Run
```powershell
# Standard build
dotnet build DepotDownloader/DepotDownloader.csproj -c Debug

# Run with args (note the --)
dotnet run --project DepotDownloader/DepotDownloader.csproj -- -app 730 -depot 731 -dir ./output

# Publish self-contained (see .github/workflows/build.yml for CI examples)
dotnet publish DepotDownloader/DepotDownloader.csproj -c Release -p:PublishSingleFile=true --self-contained --runtime win-x64 -o publish/win-x64
```

### SDK Version
Requires .NET 9.0.100 (specified in [global.json](global.json)) with `rollForward: latestMinor`

### Debug Mode
Enable with `-debug` flag - activates `DebugLog.Enabled` and `HttpDiagnosticEventListener` for SteamKit2 diagnostics

## Code Conventions

### Static Utilities
Most classes are static (`ContentDownloader`, `Util`, `PlatformUtilities`, `Ansi`) - avoid instance methods where possible

### Async Patterns
Uses async/await extensively with `TaskCompletionSource` for Steam callbacks (see `Steam3Session.CDNAuthTokens`)

### Exception Handling
Custom `ContentDownloaderException` for domain errors - thrown on invalid depot keys, license failures, etc.

### Console Output
- **ANSI Support**: [Ansi.cs](DepotDownloader/Ansi.cs) uses Spectre.Console and platform detection - progress bars Windows/macOS only
- **Platform Checks**: `OperatingSystem.IsWindowsVersionAtLeast()` pattern used, NOT runtime detection

### Dependencies
- **SteamKit2**: Core Steam protocol (never access internals directly)
- **protobuf-net**: For `AccountSettingsStore` and `ProtoManifest` serialization
- **QRCoder**: QR code generation for mobile auth
- **Microsoft.Windows.CsWin32**: Source-gen PInvoke - definitions in [NativeMethods.txt](DepotDownloader/NativeMethods.txt)

## Critical Gotchas

1. **Argument Parsing**: All args case-insensitive EXCEPT `-V` vs `--version` (checked before `HasParameter`)
2. **Directory Structure**: Downloads go to `depots/{depotId}/{version}/` by default - config stored in `.DepotDownloader/` subdirectory
3. **CDN Auth Tokens**: Cached per `(appId, host)` tuple in `ConcurrentDictionary<>` - must request before downloads
4. **Chunk Matching**: Reuses existing depot chunks on updates via `ChunkMatch` - checksums verified if `-validate` specified
5. **Concurrent Downloads**: Default 8 (`-max-downloads`), auto-increased to 16 for Lancache detection
6. **File List Regex**: Patterns compile with `RegexOptions.Compiled | RegexOptions.IgnoreCase` - apply to forward-slash paths

## Testing
No automated tests - manual testing via Steam app/depot IDs. Use anonymous login (appId 17906) for testing public content.

## CI/CD
[.github/workflows/build.yml](.github/workflows/build.yml) builds Debug+Release on Windows/macOS/Ubuntu, publishes 6 platform targets (win-x64/arm64, linux-x64/arm/arm64, osx-x64/arm64) with `PublishSingleFile=true`
