namespace mauiapp;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(nameof(CTCPage), typeof(CTCPage));
    }
}
