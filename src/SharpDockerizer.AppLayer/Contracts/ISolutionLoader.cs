namespace SharpDockerizer.AppLayer.Contracts;

/// <summary>
/// Loads .NET Solution into application
/// </summary>
public interface ISolutionLoader
{
    /// <summary>
    /// Loads .NET solution
    /// </summary>
    /// <param name="solutionFilePath">Path to .sln file</param>
    Task LoadSolution(string solutionFilePath);
}
