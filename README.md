# SharpDockerizer

SharpDockerizer can build a Dockerfile for your ASP.NET projects using solution data. Simply choose a solution on your PC, choose a project, add required data and click `Generate`, and your Dockerfile is ready!

## Preview
![SharpDockerizer Main UI Screenshot](https://github.com/DmitryGolubenkov/SharpDockerizer/blob/master/img/main-ui.jpg)

## Features
- Crossplatform (Windows, Linux, MacOS);
- Load .NET solution and parse included projects;
- Auto-parse project references;
- Generate Dockerfiles for .NET projects;
- Add NuGet repos that are required to build the project (No authorization / basic auth with password passed as docker build argument)

## Installation
This application is still in development, so no builds are avaliable. You will have to build it from source.

### Requirements:
- Git
- .NET 7 SDK
- .NET 7 runtime
### Steps
1. Clone the repository.
2. Open terminal in `src/SharpDockerizer.AvaloniaUI`.
3. Use `dotnet publish --configuration Release` to build the application.
4. Launch application. Built application will be avaliable in `src/SharpDockerizer.AvaloniaUI/bin/Release/net7.0/publish` folder, executable is named `SharpDockerizer.AvaloniaUI.exe`.

From here you can rename `publish` folder and copy it to preferred location.

### 
## License
SharpDockerizer is distributed under the [MIT](https://github.com/DmitryGolubenkov/SharpDockerizer/blob/master/LICENSE.txt) license.

# Technical details
SharpDockerizer is built using .NET 7 and is split into several parts:

- SharpDockerizer.AvaloniaUI: Avalonia application that uses Material.Avalonia styling. Application frontend.
- SharpDockerizer.AppLayer: Contains contracts, services and generators that are used by GUI to work.
- SharpDockerizer.Core: Contains domain models. Doesn't have any dependencies.

