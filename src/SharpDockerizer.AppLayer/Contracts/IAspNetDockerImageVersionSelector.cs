namespace SharpDockerizer.AppLayer.Contracts;
public interface IAspNetDockerImageVersionSelector
{
    /// <summary>
    /// Returns a link to an ASP.NET Core image that corresponds to .NET version string passed in <paramref name="version"/>
    /// </summary>
    /// <param name="version">.NET version as written in .csproj</param>
    /// <returns>A link to image such as "mcr.microsoft.com/dotnet/aspnet:7.0"</returns>
    string GetLinkToImageForVersion(string version);
}
