using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace mauiapp.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<CardData> cardList;

    [ObservableProperty]
    ObservableCollection<MTG_Set> setList;

    [ObservableProperty]
    public MTG_Set selectedSet;

    [ObservableProperty]
    public string countFilter = "All";

    [ObservableProperty]
    public string priceFilter = "All";

    [ObservableProperty]
    public string rarityFilter = "All";

    public List<CardData> FullCardList { get; set; }

    public MainPageViewModel()
    {
        CardList = new();
        SetList = new();
        FullCardList = new();
    }

    [RelayCommand]
    public async Task TapCard(CardData cardData)
    {
        await Shell.Current.GoToAsync(nameof(CTCPage), new Dictionary<string, object>()
        {
            {"cardData", cardData}
        });
    }
}