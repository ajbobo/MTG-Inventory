using CommunityToolkit.Mvvm.Input;
using mauiapp.ViewModels;

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

        cardEntry.Text = "";

        List<MTG_Set> setList = await _restService.GetAllSets();
        foreach (MTG_Set set in setList)
            _cdv.SetList.Add(set);

        if (setPicker.SelectedIndex == -1)
            setPicker.SelectedIndex = 0;

        await PopulateCardLists(setList[setPicker.SelectedIndex].Code);

        cardEntry.Focus();
    }

    protected async void OnPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        var picker = (Picker)sender;
        int selectedIndex = picker.SelectedIndex;

        if (selectedIndex != -1)
        {
            var selectedSet = (MTG_Set)picker.SelectedItem;
            await PopulateCardLists(selectedSet.Code);
        }
    }

    private async Task PopulateCardLists(string setCode)
    {
        _cdv.CardList.Clear();
        _cdv.FullCardList.Clear();
        List<CardData> cardList = await _restService.GetCardsInSet(setCode);
        foreach (CardData card in cardList)
        {
            _cdv.CardList.Add(card);
            _cdv.FullCardList.Add(card);
        }
    }

    protected void OnEntryTextChanged(object sender, EventArgs e)
    {
        var entry = (Entry)sender;
        string text = entry.Text.ToLower();

        _cdv.CardList.Clear();

        foreach (CardData card in _cdv.FullCardList)
        {
            string cardName = card.Card.Name.ToLower();
            if (cardName.StartsWith(text))
                _cdv.CardList.Add(card);
        }
    }
}

