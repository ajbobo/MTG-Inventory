using mauiapp.ViewModels;

namespace mauiapp;

public partial class CTCPage : ContentPage
{
	public CTCPage(CTCPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}