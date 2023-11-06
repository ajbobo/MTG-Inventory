using mauiapp.ViewModels;
using System.Formats.Tar;

namespace mauiapp;

public partial class CTCPage : ContentPage
{
    CTCPageViewModel _vm;

	public CTCPage(CTCPageViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
        _vm = vm;
        _vm.Ctcs = new();
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();

        foreach (CardTypeCount ctc in _vm.CardData.CTCs)
            _vm.Ctcs.Add(ctc);
    }
}