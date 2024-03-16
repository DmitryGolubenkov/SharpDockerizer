using SharpDockerizer.AppLayer.Contracts;

namespace SharpDockerizer.AppLayer.Services.Project;


public class DotNetSdkImageVersionSelector : IDotNetSdkImageVersionSelector
{
    private const string BaseUrl = "mcr.microsoft.com/dotnet/sdk:";

    public string GetLinkToImageForVersion(string version)
    {
        if (string.IsNullOrWhiteSpace(version))
        {
            return $"{BaseUrl}latest";
        }

        version = version.Trim().ToLowerInvariant();

        if (version.StartsWith("net") && version.Length > 3 && version[3..].All(char.IsDigit))
        {
            var versionNumber = version[3..];
            return $"{BaseUrl}{versionNumber}.0";
        }
        else if (version.StartsWith("netcoreapp"))
        {
            var versionNumber = version["netcoreapp".Length..];
            return $"{BaseUrl}{versionNumber}";
        }

        return $"{BaseUrl}latest";
    }
}