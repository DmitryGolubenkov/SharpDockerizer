# SharpDockerizer

SharpDockerizer can automatically build a Dockerfile for your ASP.NET projects using solution data. Simply choose a solution on your PC, choose a project, add required data such as NuGet sources and ports to expose and click `Generate Dockerfile`, and your Dockerfile is ready!

## Preview

![SharpDockerizer Main UI Screenshot](https://github.com/DmitryGolubenkov/SharpDockerizer/blob/master/img/main-ui.jpg)

## Features

- Load .NET solution and parse included projects;
- Crossplatform (Windows, Linux, MacOS);
- Auto-parse project references;
- Generate Dockerfiles for .NET projects;
- Add NuGet repos that are required to build the project (No authorization / basic auth with password passed as docker build argument)
- Detect `NuGet.config`
- Localization support
- [Template support](https://github.com/DmitryGolubenkov/SharpDockerizer/wiki/Templates)

## Installation

### Binaries

You can download SharpDockerizer from GitHub: [Releases](https://github.com/DmitryGolubenkov/SharpDockerizer/releases). 

### Build from source

#### Requirements:

- Git
- .NET 8 SDK
- .NET 8 runtime

#### Steps

1. Clone the repository.
2. Open terminal in `src/SharpDockerizer.AvaloniaUI`.
3. Use `dotnet publish --configuration Release` to build the application. Built application will be avaliable in `src/SharpDockerizer.AvaloniaUI/bin/Release/net8.0/publish` folder, executable is named `SharpDockerizer.exe`.
4. Launch application. 

From here you can rename `publish` folder and copy it to preferred location.

### 

## License

SharpDockerizer is distributed under the [MIT](https://github.com/DmitryGolubenkov/SharpDockerizer/blob/master/LICENSE.txt) license.

# Technical details

SharpDockerizer is built using .NET 8 and is split into several parts:

- `SharpDockerizer.AvaloniaUI`: Avalonia application with MVVM architecture that uses `Material.Avalonia` styling. Application frontend. MVVM framework is `CommunityToolkit.Mvvm`
- `SharpDockerizer.AppLayer`: Contains contracts, services, generators, etc. that are used by GUI to work.
- `SharpDockerizer.Core`: Contains domain models. Doesn't have any dependencies.
