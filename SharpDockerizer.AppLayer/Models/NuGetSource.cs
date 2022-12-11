namespace SharpDockerizer.AppLayer.Models;
public class NuGetSource
{
    /// <summary>
    /// NuGet source name
    /// </summary>
    public required string Name { get; set; }
    /// <summary>
    /// Link to NuGet repository's index.json
    /// </summary>
    public required string Link { get; set; }
    /// <summary>
    /// Does NuGet source require authentication?
    /// </summary>
    public required bool AuthenticationRequired { get; set; }
    /// <summary>
    /// Username that is used to authenticate in NuGet repository
    /// </summary>
    public string? Username { get; set; }
}
