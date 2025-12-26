# DepotDownloader

## Project Overview

DepotDownloader is a console-based utility for downloading Steam depots. It leverages the [SteamKit2](https://github.com/SteamRE/SteamKit) library to interact with the Steam network, allowing users to download game content, workshop items, and manifests directly from Steam servers.

The project is built with **.NET 9.0**.

## Architecture

*   **Type:** C# Console Application
*   **Framework:** .NET 9.0 (`net9.0`)
*   **Key Dependencies:**
    *   `SteamKit2`: Core library for Steam network interaction.
    *   `protobuf-net`: For handling Protocol Buffers.
    *   `QRCoder`: For generating login QR codes.
    *   `Microsoft.Windows.CsWin32`: Source generator for Windows PInvoke (likely for console/system integration).

## Building and Running

### Prerequisites

*   .NET 9.0 SDK (defined in `global.json`)

### Build Commands

The project uses standard `dotnet` CLI commands.

*   **Restore dependencies:**
    ```bash
    dotnet restore
    ```

*   **Build the project:**
    ```bash
    dotnet build DepotDownloader/DepotDownloader.csproj -c Debug
    ```

*   **Publish (Create self-contained executable):**
    The CI workflow (`.github/workflows/build.yml`) demonstrates publishing for multiple platforms.

    *   **Windows x64:**
        ```bash
        dotnet publish DepotDownloader/DepotDownloader.csproj -c Release -p:PublishSingleFile=true --self-contained --runtime win-x64 -o publish/win-x64
        ```
    *   **Linux x64:**
        ```bash
        dotnet publish DepotDownloader/DepotDownloader.csproj -c Release -p:PublishSingleFile=true --self-contained --runtime linux-x64 -o publish/linux-x64
        ```
    *   **macOS x64:**
        ```bash
        dotnet publish DepotDownloader/DepotDownloader.csproj -c Release -p:PublishSingleFile=true --self-contained --runtime osx-x64 -o publish/osx-x64
        ```

### Running the Application

Since this is a CLI tool, it requires arguments to function.

**Basic Syntax:**
```bash
./DepotDownloader [options]
```

**Common Examples:**

*   **Download a specific app and depot:**
    ```bash
    dotnet run --project DepotDownloader/DepotDownloader.csproj -- -app <APP_ID> -depot <DEPOT_ID> -manifest <MANIFEST_ID>
    ```

*   **Download with login:**
    ```bash
    dotnet run --project DepotDownloader/DepotDownloader.csproj -- -app <APP_ID> -username <USERNAME> -password <PASSWORD>
    ```

*   **Download Workshop Item:**
    ```bash
    dotnet run --project DepotDownloader/DepotDownloader.csproj -- -app <APP_ID> -pubfile <PUBFILE_ID>
    ```

*   **View Help/Version:**
    ```bash
    dotnet run --project DepotDownloader/DepotDownloader.csproj -- --version
    ```

## Key Files and Directories

*   `DepotDownloader/`: Source code directory.
    *   `Program.cs`: Main entry point. Handles argument parsing and setup.
    *   `ContentDownloader.cs`: Likely contains the core logic for downloading depot chunks.
    *   `DepotDownloader.csproj`: Project configuration file.
    *   `Steam3Session.cs`: Manages the Steam session and connection.
*   `.github/workflows/build.yml`: CI/CD configuration for building and releasing.
*   `global.json`: Specifies the .NET SDK version.

## Development Conventions

*   **Code Style:** Standard C# coding conventions.
*   **Configuration:** The project uses `DepotDownloader.sln` as the solution file.
*   **CI:** GitHub Actions is used for continuous integration, running builds on Windows, Linux, and macOS.
