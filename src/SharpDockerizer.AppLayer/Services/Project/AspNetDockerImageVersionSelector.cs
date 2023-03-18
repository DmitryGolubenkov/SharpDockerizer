using SharpDockerizer.AppLayer.Contracts;

namespace SharpDockerizer.AppLayer.Services.Project;
public class AspNetDockerImageVersionSelector : IAspNetDockerImageVersionSelector
{
    // TODO: temporary implementation for generic purposes. Maybe provide some way to choose exact
    // version in GUI? Make GET requests to docker hub API to find avaliable images?  
    public string GetLinkToImageForVersion(string version) => version is not null ? version.Trim() switch
    {
        "netcoreapp3.1" => "mcr.microsoft.com/dotnet/aspnet:3.1",
        "net5.0" => "mcr.microsoft.com/dotnet/aspnet:5.0",
        "net6.0" => "mcr.microsoft.com/dotnet/aspnet:6.0",
        "net7.0" => "mcr.microsoft.com/dotnet/aspnet:7.0",
        "net8.0" => "mcr.microsoft.com/dotnet/aspnet:8.0",
        "net9.0" => "mcr.microsoft.com/dotnet/aspnet:9.0",
        _ => "mcr.microsoft.com/dotnet/aspnet"
    } : "mcr.microsoft.com/dotnet/aspnet";

}
