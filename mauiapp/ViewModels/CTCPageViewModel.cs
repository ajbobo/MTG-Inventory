using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace mauiapp.ViewModels;

[QueryProperty("CardData", "cardData")]
public partial class CTCPageViewModel : ObservableObject
{
    [ObservableProperty]
    CardData cardData;

    [ObservableProperty]
    ObservableCollection<CardTypeCount> ctcs;

    private IRestService _restService;

    public CTCPageViewModel(IRestService restService)
    {
        _restService = restService;
    }

    [RelayCommand]
    async Task GoBack()
    {
        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    void PlusOne(CardTypeCount ctc)
    {

        ctc.Count++;
        UpdateView(ctc);

        _restService.UpdateCardData(CardData);
    }

    [RelayCommand]
    void MinusOne(CardTypeCount ctc)
    {
        ctc.Count--;
        UpdateView(ctc);

        _restService.UpdateCardData(CardData);
    }

    [RelayCommand]
    void EqualsFour(CardTypeCount ctc)
    {
        ctc.Count = 4;
        UpdateView(ctc);

        _restService.UpdateCardData(CardData);
    }

    private void UpdateView(CardTypeCount ctc)
    {
        int index = Ctcs.IndexOf(ctc);
        Ctcs.RemoveAt(index);
        Ctcs.Insert(index, new CardTypeCount() // Have to create a new one because of some caching (or something) in the UI layer
        {
            CardType = ctc.CardType,
            Count = ctc.Count
        });

        // Keep the CTC in the CardData up-to-date with the one in the other observable list
        CardData.CTCs.RemoveAt(index);
        CardData.CTCs.Insert(index, ctc);
    }


}
