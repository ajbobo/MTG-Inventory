using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace mauiapp;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    ObservableCollection<CardData> cardList;

    [ObservableProperty]
    ObservableCollection<MTG_Set> setList;

    [ObservableProperty]
    public MTG_Set selectedSet;

    public List<CardData> FullCardList { get; set; }

    public MainPageViewModel()
    {
        CardList = new();
        SetList = new();
        FullCardList = new();
    }
}