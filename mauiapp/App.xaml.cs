namespace mauiapp;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

		MainPage = new AppShell();
	}

#if WINDOWS
	protected override Window CreateWindow(IActivationState activationState)
	{
		Window window = base.CreateWindow(activationState);

		window.Height = 800;
		window.Width = 600;

		return window;
	}
#endif
}
