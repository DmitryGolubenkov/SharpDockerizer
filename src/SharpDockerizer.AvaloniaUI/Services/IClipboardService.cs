using System.Threading.Tasks;

namespace SharpDockerizer.AvaloniaUI.Services;

public interface IClipboardService
{
    /// <summary>
    /// Sets text in clipboard.
    /// </summary>
    public Task SetTextAsync(string text);

    /// <summary>
    /// Gets data stored in clipboard. Can be <see langword="null"/>.
    /// </summary>
    public Task<string?> GetTextAsync();
}