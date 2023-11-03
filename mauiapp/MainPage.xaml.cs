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
		List<CardData> cardList = await _restService.GetCardsInSet("dom");
		foreach (CardData card in cardList)
			_cdv.CardList.Add(card);
	}
}

