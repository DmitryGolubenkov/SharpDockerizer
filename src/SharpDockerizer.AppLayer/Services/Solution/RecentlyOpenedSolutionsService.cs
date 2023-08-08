using SharpDockerizer.AppLayer.Contracts;
using SharpDockerizer.AppLayer.Models;
using System.Text;

namespace SharpDockerizer.AppLayer.Services.Solution;

/// <summary>
/// Controls recently opened solutions.
/// </summary>
public class RecentlyOpenedSolutionsService : IRecentlyOpenedSolutionsService
{

    #region Consts
    
    /// <summary>
    /// Save file name. It is placed in directory where SharpDockerizer is present
    /// </summary>
    private const string saveFileName = "recentsolutions";

    #endregion

    #region Properties

    /// <summary>
    /// Returns path to save file
    /// </summary>
    private string SaveFilePath { get => Path.Combine(AppContext.BaseDirectory, saveFileName); }

    #endregion

    #region Methods

    /// <summary>
    /// Returns list of recently opened solutions, parsed from local csv, if exists. 
    /// Returns empty list if file does not exist.
    /// </summary>
    /// <returns></returns>
    public List<RecentlyOpenedSolution> GetSolutions()
    {
        List<RecentlyOpenedSolution> result = new List<RecentlyOpenedSolution>();
        // If doesn't exist - then return empty list.
        if (!File.Exists(SaveFilePath))
        {
            return result;
        }

        // Read file and parse it
        try
        {
            var solutions = File.ReadAllLines(SaveFilePath);
            foreach (var solution in solutions)
            {
                var parts = solution.Split(';');
                result.Add(new RecentlyOpenedSolution()
                {
                    Name = parts[0],
                    AbsolutePath = parts[1],
                });
            }

            return result;
        }
        catch
        {
            // Cache file could be damaged. Remove it as we don't know what is really broken in it.
            File.Delete(SaveFilePath);
            return new List<RecentlyOpenedSolution>();
        }
    }


    public async Task Add(RecentlyOpenedSolution solution)
    {
        // First we get what is already saved on disk
        var recentSolutions = GetSolutions();

        // If this solution was already opened - remove it from list
        var maybeAddedIndex = recentSolutions
            .FindIndex(x => x.AbsolutePath == solution.AbsolutePath);

        if (maybeAddedIndex != -1)
            recentSolutions.RemoveAt(maybeAddedIndex);

        // Insert new solution at start of the list and save
        recentSolutions.Insert(0, solution);
        await Save(recentSolutions);
    }

    /// <summary>
    /// Removes a solution with concrete absolute path from recent solutions list.
    /// </summary>
    public async Task RemoveWithPath(string path)
    {
        var recentSolutions = GetSolutions();
        var maybeAddedIndex = recentSolutions.FindIndex(x => x.AbsolutePath == path);
        if (maybeAddedIndex != -1)
            recentSolutions.RemoveAt(maybeAddedIndex);

        await Save(recentSolutions);
    }

    /// <summary>
    /// Saves recent solutions to disk. Implemented as csv file.
    /// </summary>
    private async Task Save(List<RecentlyOpenedSolution> recentlyOpenedSolutions)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var solution in recentlyOpenedSolutions)
        {
            sb.AppendLine($"{solution.Name};{solution.AbsolutePath}");
        }

        await File.WriteAllTextAsync(SaveFilePath, sb.ToString());
    }

    #endregion

}
