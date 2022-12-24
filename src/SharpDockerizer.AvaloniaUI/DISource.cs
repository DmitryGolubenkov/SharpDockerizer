using Avalonia.Markup.Xaml;
using System;

namespace SharpDockerizer.AvaloniaUI;
#pragma warning disable CS8618
public class DISource : MarkupExtension
{
    public static Func<Type, object> Resolver { get; set; }
    public Type Type { get; set; }
    public override object ProvideValue(IServiceProvider serviceProvider) => Resolver?.Invoke(Type);
}
