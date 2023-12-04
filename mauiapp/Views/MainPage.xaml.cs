﻿using CommunityToolkit.Mvvm.Input;
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

    protected async void OnFilterSelectedIndexChanged(object sender, EventArgs e)
    {
        if (_cdv.SelectedSet != null)
            await PopulateCardLists(_cdv.SelectedSet.Code);
    }

    private async Task PopulateCardLists(string setCode)
    {
        activityIndicator.IsRunning = true;
        _cdv.CardList.Clear();
        _cdv.FullCardList.Clear();
        var count = _cdv.CountFilter.Equals("All") ? "" : _cdv.CountFilter;
        var price = _cdv.PriceFilter.Equals("All") ? "" : _cdv.PriceFilter;
        var rarity = _cdv.RarityFilter.Equals("All") ? "" : _cdv.RarityFilter.Substring(0,1);
        List<CardData> cardList = await _restService.GetCardsInSet(setCode, count, price, rarity);
        foreach (CardData card in cardList)
        {
            _cdv.CardList.Add(card);
            _cdv.FullCardList.Add(card);
        }
        activityIndicator.IsRunning = false;
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

    protected async void OnEntryCompleted(object sender, EventArgs e)
    {
        // Open whatever card is at the top of the visible list
        CardData topCard = _cdv.CardList[0];
        await _cdv.TapCard(topCard);
    }
}

