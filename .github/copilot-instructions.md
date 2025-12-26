# DepotDownloader — AI Coding Guide

Concise guidance for AI agents working in this repo. Focus on the actual patterns used here; avoid introducing new frameworks or generic conventions.

## Big Picture
- **Purpose**: CLI to download Steam depots/workshop content via SteamKit2.
- **Runtime**: .NET SDK 9.0.100 (see [global.json](global.json)); cross‑platform console.
- **Flow**: [Program.cs](DepotDownloader/Program.cs) parses args → [DownloadConfig.cs](DepotDownloader/DownloadConfig.cs) populates singleton → [Steam3Session.cs](DepotDownloader/Steam3Session.cs) authenticates → [ContentDownloader.cs](DepotDownloader/ContentDownloader.cs) orchestrates downloads using [CDNClientPool.cs](DepotDownloader/CDNClientPool.cs).

## Core Files & Roles
- [Program.cs](DepotDownloader/Program.cs): Custom arg parsing via `GetParameter()`/`HasParameter()` with consumed‑args tracking; handle `-V`/`--version` early.
- [DownloadConfig.cs](DepotDownloader/DownloadConfig.cs): Global singleton accessed as `ContentDownloader.Config` (do not pass config objects).
- [Steam3Session.cs](DepotDownloader/Steam3Session.cs): Auth/session lifecycle (username/password/QR, callbacks, reconnection, license checks). Uses `ConsoleAuthenticator` and caches CDN tokens.
- [ContentDownloader.cs](DepotDownloader/ContentDownloader.cs): Core orchestration, chunk verification, file filtering, concurrency, and manifest‑only mode.
- [CDNClientPool.cs](DepotDownloader/CDNClientPool.cs): Weighted server selection + penalty tracking via `AccountSettingsStore`.
- [AccountSettingsStore.cs](DepotDownloader/AccountSettingsStore.cs): IsolatedStorage + protobuf‑net for login keys, guard, CDN penalties.

## Project‑Specific Patterns
- **Singleton config**: Use `ContentDownloader.Config` everywhere.
- **Arg parsing**: Case‑insensitive except `-V` vs `--version`; rely on helpers and consumed‑args array.
- **File filtering**: `-filelist` supports `regex:` prefix; paths normalized to forward slashes; `RegexOptions.Compiled|IgnoreCase`.
- **ANSI/UI**: [Ansi.cs](DepotDownloader/Ansi.cs) via Spectre.Console; progress bars enabled on Windows/macOS; platform checks use `OperatingSystem.IsWindowsVersionAtLeast()`.
- **Async callbacks**: `TaskCompletionSource` patterns for Steam events (e.g., CDN auth tokens).
- **Errors**: Throw `ContentDownloaderException` for domain failures (keys, licenses, etc.).

## Build, Run, Publish
```powershell
# Build (Debug)
dotnet build DepotDownloader/DepotDownloader.csproj -c Debug
# Run (note the -- before app args)
dotnet run --project DepotDownloader/DepotDownloader.csproj -- -app 730 -depot 731 -dir ./out
# Publish single-file (example: Windows x64)
dotnet publish DepotDownloader/DepotDownloader.csproj -c Release -p:PublishSingleFile=true --self-contained --runtime win-x64 -o publish/win-x64
```

## Critical Gotchas
- **Version flag**: `-V` and `--version` handled before standard parsing.
- **Default paths**: Downloads under `depots/{depotId}/{version}/`; config in `.DepotDownloader/`.
- **CDN tokens**: Cached per `(appId, host)`; fetch before downloads.
- **Chunk reuse**: `ChunkMatch` reuses existing data; use `-validate` to checksum existing files.
- **Concurrency**: Default `-max-downloads 8`; may bump to 16 when Lancache detected.

## Testing & CI
- **No unit tests**: Validate manually using real app/depot IDs; anon login app ID 17906 for public content.
- **CI workflows**: See [.github/workflows/build.yml](.github/workflows/build.yml) for multi‑platform publish.

If anything here feels unclear or missing (e.g., arg nuances, session caching details), tell me what you need and I’ll refine this guide.
