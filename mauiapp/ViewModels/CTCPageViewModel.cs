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
    void PlusOne(CardTypeCount ctc)
    {
        ctc.Count++;
    }

    [RelayCommand]
    void MinusOne(CardTypeCount ctc)
    {
        ctc.Count--;
    }

    [RelayCommand]
    void EqualsFour(CardTypeCount ctc)
    {
        ctc.Count = 4;
    }

    [RelayCommand]
    void Remove(CardTypeCount ctc)
    {
        Ctcs.Remove(ctc);
        CardData.CTCs.Remove(ctc);
    }

    [RelayCommand]
    void NewCTC()
    {
        CardTypeCount ctc = new()
        {
            CardType = Ctcs.Count == 0 ? "Standard" : "foil",
            Count = 1
        };

        Ctcs.Add(ctc);
        CardData.CTCs.Add(ctc);
    }

    [RelayCommand]
    async Task Save()
    {
        await _restService.UpdateCardData(CardData);
        await Shell.Current.GoToAsync("..");
    }
}
