using System;

using Abacus;

using Avalonia;

using CutsceneMakerUI;



namespace CutsceneMaker;

internal class Program
{
	public static string VERSION = "2.1.1";
	public static AutoCompletionData AutoCompletion = new();

	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args) => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
		.UsePlatformDetect()
		.WithInterFont()
		.LogToTrace();
}
