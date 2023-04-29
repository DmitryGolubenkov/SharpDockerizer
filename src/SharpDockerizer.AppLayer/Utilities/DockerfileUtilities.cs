using SharpDockerizer.AppLayer.Models;
using SharpDockerizer.Core.Models;
using System.Text;

namespace SharpDockerizer.AppLayer.Utilities;
internal static class DockerfileUtilities
{
    /// <summary>
    /// Converts list of strings ("ARG ..."; "ARG...") into one string, each string on new line.
    /// </summary>
    public static string GetArgumentsString(List<string> dockerfileArgumentsList)
    {
        var sb = new StringBuilder();

        foreach (string arg in dockerfileArgumentsList)
        {
            sb.AppendLine($@"ARG {arg}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates NuGet instructions for simple authentication.
    /// </summary>
    /// <param name="nuGetSources">List of NuGet sources.</param>
    /// <param name="dockerfileArgumentsList">List of strings, that will be modified.</param>
    /// <returns>"RUN dotnet nuget add source ..."  "RUN dotnet nuget update source..." as one multiline string. </returns>
    public static string GetNuGetInstructions(List<NuGetSource> nuGetSources, ref List<string> dockerfileArgumentsList)
    {
        var sb = new StringBuilder();

        // TODO: Maybe don't store tokens in args? They are displayed in docker history. There should be a way to use secrets.
        foreach (var source in nuGetSources)
        {
            var argName = source.Name.ToLowerInvariant() + "token";
            dockerfileArgumentsList.Add(argName);
            sb.AppendLine($@"RUN dotnet nuget add source --name {source.Name} {source.Link}");
            // Authenticate if needed
            if (source.AuthenticationRequired)
            {
                sb.AppendLine(
                    $@"RUN dotnet nuget update source {source.Name} --store-password-in-clear-text --valid-authentication-types basic --username {source.Username} --password ${argName}"
                    );
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="exposedPorts">List of ports to expose.</param>
    /// <returns>A multiline string like "EXPOSE 80 \n EXPOSE 443"</returns>
    public static string GetExposedPorts(List<int> exposedPorts)
    {
        var sb = new StringBuilder();

        foreach (var port in exposedPorts)
        {
            sb.AppendLine($@"EXPOSE {port}");
        }

        return sb.ToString();
    }

    public static string GetCopyProjFilesDockerfileInstructions(List<ProjectData> projectDependencies)
    {
        var sb = new StringBuilder();

        foreach (ProjectData projectData in projectDependencies)
        {
            var projectFolderRelativePath = Path.GetDirectoryName(projectData.RelativePath);
            sb.AppendLine($@"COPY [""{projectData.RelativePath}"", ""{projectFolderRelativePath}/""]");
        }

        return sb.ToString();
    }

    public static string GetCopyEverythingDockerfileInstructions(List<ProjectData> projectDependencies)
    {
        var sb = new StringBuilder();

        foreach (ProjectData projectData in projectDependencies)
        {
            var projectFolderRelativePath = Path.GetDirectoryName(projectData.RelativePath);
            sb.AppendLine($@"COPY [""{projectFolderRelativePath}/"", ""{projectFolderRelativePath}/""]");
        }

        return sb.ToString();

    }
}
