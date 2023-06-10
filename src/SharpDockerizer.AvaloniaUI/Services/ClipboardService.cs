using Avalonia.Input.Platform;
using System.Threading.Tasks;

namespace SharpDockerizer.AvaloniaUI.Services;

/// <summary>
/// Wrapper around clipboard.
/// </summary>
internal class ClipboardService : IClipboardService
{
    private readonly IClipboard _clipboard;

    public ClipboardService(MainWindow window)
    {
        _clipboard = window.Clipboard;
    }

    /// <summary>
    /// Gets data stored in clipboard. Can be <see langword="null"/>.
    /// </summary>
    public async Task<string?> GetTextAsync()
    {
        return await _clipboard.GetTextAsync();
    }

    /// <summary>
    /// Sets text in clipboard.
    /// </summary>
    public async Task SetTextAsync(string text)
    {
        await _clipboard.SetTextAsync(text);
    }
}
