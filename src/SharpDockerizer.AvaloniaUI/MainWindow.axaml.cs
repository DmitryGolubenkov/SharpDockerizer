using Avalonia;
using Avalonia.Controls;
namespace SharpDockerizer.AvaloniaUI;
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

#if DEBUG
        this.AttachDevTools();
#endif
    }
}
