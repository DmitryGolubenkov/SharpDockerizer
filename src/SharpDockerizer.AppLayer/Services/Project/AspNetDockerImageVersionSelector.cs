using SharpDockerizer.AppLayer.Contracts;

namespace SharpDockerizer.AppLayer.Services.Project;

public class AspNetDockerImageVersionSelector : IAspNetDockerImageVersionSelector
{
    private const string BaseUrl = "mcr.microsoft.com/dotnet/aspnet:";

    public string GetLinkToImageForVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return BaseUrl;
        }

        version = version.Trim().ToLowerInvariant();

        if ((version.StartsWith("net") 
            && version.Length > 3 
            && version[3..].All(char.IsDigit)) 
            || version.StartsWith("netcoreapp"))
        {
            string versionNumber = version.StartsWith("netcoreapp") ? version["netcoreapp".Length..] : version[3..];
            return $"{BaseUrl}{versionNumber}";
        }

        return BaseUrl;
    }
}