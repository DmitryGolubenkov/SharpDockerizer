namespace SharpDockerizer.AvaloniaUI.Models;
public class ExposedPort
{
    public required int Port { get; set; }
    public static ExposedPort Http { get => new ExposedPort() { Port = 80 }; }
}
