namespace mauiapp;

public partial class MainPage : ContentPage
{
	private IRestService _restService;
	private MainPageViewModel _cdv;

	public MainPage(IRestService restService, MainPageViewModel cdv)
	{
		InitializeComponent();
        _restService = restService;
		_cdv = cdv;
		BindingContext = _cdv;
	}

	protected async override void OnAppearing()
	{
		base.OnAppearing();

		List<MTG_Set> setList = await _restService.GetAllSets();
		foreach (MTG_Set set in setList)
			_cdv.SetList.Add(set);

		List<CardData> cardList = await _restService.GetCardsInSet(setList[0].Code);
		foreach (CardData card in cardList)
			_cdv.CardList.Add(card);

		setPicker.SelectedIndex = 0;
	}

    protected async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

		_cdv.CardList.Clear();
        if (selectedIndex != -1)
        {
			var selectedSet = (MTG_Set)picker.SelectedItem;
            List<CardData> cardList = await _restService.GetCardsInSet(selectedSet.Code);
            foreach (CardData card in cardList)
                _cdv.CardList.Add(card);
        }
    }
}

