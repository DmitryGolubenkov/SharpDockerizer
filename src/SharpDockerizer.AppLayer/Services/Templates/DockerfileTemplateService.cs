namespace SharpDockerizer.AppLayer.Services.Templates;

/// <summary>
/// A service that helps with managing dockerfile templates on disk
/// </summary>
public class DockerfileTemplateService
{
    #region Consts

    /// <summary>
    /// Extension which is used to determine Dockerfile template files
    /// </summary>
    private const string _extension = ".sdtemplate";

    #endregion

    #region Methods

    /// <summary>
    /// Checks if a SharpDockerizer template exists in directory
    /// </summary>
    /// <param name="path">Path to directory. Throws if doesn't exist.</param>
    public bool CheckIfPathContainsTemplate(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            throw new ArgumentException("Directory does not exist!", nameof(directoryPath));

        return Directory.EnumerateFiles(directoryPath).Any(x=>x.Contains(_extension));
    }

    /// <summary>
    /// Loads dockerfile template from file and returns it.
    /// </summary>
    public string LoadTemplate(string directoryPath)
    {
        if (!Directory.Exists(directoryPath))
            throw new ArgumentException("Directory does not exist!", nameof(directoryPath));
        
        // Try to find a file with extension. The first file in alphabetical order is used.
        var filePath = Directory.EnumerateFiles(directoryPath)
            .Where(x=>x.Contains(_extension))
            .Order().FirstOrDefault();

        if (filePath is null)
            throw new InvalidOperationException($"No template exists at path {directoryPath}!");

        return File.ReadAllText(filePath);
    }

    #endregion
}
